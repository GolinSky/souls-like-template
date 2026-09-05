using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using PlayerCharacter = SoulsLike.Entities.Character.Character;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Enemy;
using SoulsLike.Items;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Ladder
{
    public sealed class LadderClimber : MonoBehaviour, IEntityComponent, IInitializable, IDisposable
    {
        private const string LADDER_LAYER = "Ladder";
        private const float TRANSITION_SECONDS = 0.08f;
        private const float ENTER_BOTTOM_SECONDS = 1.167f;
        private const float ENTER_TOP_SECONDS = 2.5f;
        private const float EXIT_TOP_SECONDS = 1.667f;
        private const float EXIT_BOTTOM_SECONDS = 1.167f;
        private const float UNLOCK_SECONDS = 1.5f;
        private const float PUNCH_SECONDS = 2f;
        private const float KICK_SECONDS = 2f;
        private const float DRINK_SECONDS = 3.333f;

        private static readonly int _idle = Animator.StringToHash("LadderIdle");
        private static readonly int _enterBottom = Animator.StringToHash("LadderEnterBottom");
        private static readonly int _enterTop = Animator.StringToHash("LadderEnterTop");
        private static readonly int _climbUp = Animator.StringToHash("LadderClimbUp");
        private static readonly int _climbDown = Animator.StringToHash("LadderClimbDown");
        private static readonly int _slide = Animator.StringToHash("LadderSlide");
        private static readonly int _exitTop = Animator.StringToHash("LadderExitTop");
        private static readonly int _exitBottom = Animator.StringToHash("LadderExitBottom");
        private static readonly int _punch = Animator.StringToHash("LadderPunch");
        private static readonly int _kick = Animator.StringToHash("LadderKick");
        private static readonly int _drink = Animator.StringToHash("LadderDrink");
        private static readonly int _unlock = Animator.StringToHash("LadderUnlock");

        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController characterController;
        [SerializeField, Min(0.1f)] private float climbSpeed = 2f;
        [SerializeField, Min(0.1f)] private float fastClimbSpeed = 3.5f;
        [SerializeField, Min(0.1f)] private float slideSpeed = 5f;
        [SerializeField, Min(0.1f)] private float dropGravity = 20f;
        [SerializeField, Min(0f)] private float punchStaminaCost = 16f;
        [SerializeField, Min(0f)] private float kickStaminaCost = 20f;
        [SerializeField, Min(0f)] private float punchDamage = 18f;
        [SerializeField, Min(0f)] private float kickDamage = 24f;
        [SerializeField, Min(0f)] private float attackRange = 2.1f;

        private Entity _entity;
        private IHealthComponent _health;
        private LadderSystem _ladderSystem;
        private EnemyNavigationMotor _enemyMotor;
        private EnemyActionExecutor _enemyExecutor;
        private CancellationTokenSource _operationCancellation;
        private int _operationGeneration;
        private int _rootMotionGeneration = -1;
        private int _ladderLayer = -1;
        private int _currentAnimation;
        private float _actionEndsAt;
        private float _impactAt;
        private float _dropVelocity;
        private bool _previousApplyRootMotion;
        private bool _isTransitioning;
        private bool _isExiting;
        private bool _isDropping;
        private bool _impactFired;
        private PendingAction _pendingAction;

        public bool IsAttached => CurrentLadder != null;
        public bool IsBusy => IsAttached || _isTransitioning || _isDropping;
        public bool IsReceivingLadderAttack { get; private set; }
        public LadderView CurrentLadder { get; private set; }
        public float DistanceOnLadder { get; private set; }
        public Entity Entity => _entity;

        [Inject]
        public void Construct(Entity entity, IHealthComponent health, LadderSystem ladderSystem)
        {
            _entity = entity;
            _health = health;
            _ladderSystem = ladderSystem;
        }

        public void Initialize()
        {
            _entity.RegisterComponent(this);
            _enemyMotor = GetComponent<EnemyNavigationMotor>();
            _enemyExecutor = GetComponentInChildren<EnemyActionExecutor>();
            _ladderLayer = animator.GetLayerIndex(LADDER_LAYER);
            if (_ladderLayer < 0) throw new InvalidOperationException(
                $"Animator '{animator.name}' requires the '{LADDER_LAYER}' layer.");
        }

        public void Dispose()
        {
            ForceDetach(LadderDetachReason.Disposed);
            _entity.UnRegisterComponent(this);
        }

        public bool CanUseLadder()
        {
            if (IsBusy || !_health.Stats.IsAlive) return false;
            PlayerCharacter character = GetComponent<PlayerCharacter>();
            if (character != null) return character.CanStartLadder;
            EnemyActor enemy = GetComponent<EnemyActor>();
            return enemy != null && enemy.BehaviourProfile.CanUseLadders;
        }

        public async UniTask AttachAsync(LadderView ladder, LadderEnd end, CancellationToken token)
        {
            if (!CanUseLadder() || !ladder.TryAcquire(this, end)) return;
            int generation = BeginOperation(token, out CancellationToken operationToken, out CancellationTokenSource operationCancellation);
            CurrentLadder = ladder;
            DistanceOnLadder = end == LadderEnd.Bottom ? 0f : ladder.Length;
            _isTransitioning = true;
            _enemyExecutor?.Interrupt(EnemyInterruptReason.Traversal);
            _enemyMotor?.SuspendForTraversal();
            SuppressRootMotion(generation);
            NotifyCharacterAttached();
            Play(end == LadderEnd.Bottom ? _enterBottom : _enterTop);
            try
            {
                await AlignToPositionAsync(ladder.SamplePosition(DistanceOnLadder), ladder.SampleRotation(),
                    end == LadderEnd.Bottom ? ENTER_BOTTOM_SECONDS : ENTER_TOP_SECONDS, operationToken);
                if (IsCurrent(generation))
                {
                    _isTransitioning = false;
                    Play(_idle);
                }
            }
            catch (OperationCanceledException) when (!IsCurrent(generation)) { }
            catch (OperationCanceledException)
            {
                ForceDetach(LadderDetachReason.Drop);
            }
            finally { EndOperation(generation, operationCancellation); }
        }

        public async UniTask UnlockAsync(LadderView ladder, CancellationToken token)
        {
            if (!CanUseLadder() || GetComponent<PlayerCharacter>() == null) return;
            int generation = BeginOperation(token, out CancellationToken operationToken, out CancellationTokenSource operationCancellation);
            _isTransitioning = true;
            SuppressRootMotion(generation);
            NotifyCharacterAttached();
            Play(_unlock);
            try
            {
                await AlignToPositionAsync(ladder.SamplePosition(ladder.Length), ladder.SampleRotation(),
                    ENTER_TOP_SECONDS, operationToken);
                if (!IsCurrent(generation)) return;
                await UniTask.Delay(TimeSpan.FromSeconds(UNLOCK_SECONDS), cancellationToken: operationToken);
                if (!IsCurrent(generation)) return;
                await _ladderSystem.UnlockAsync(ladder, operationToken);
                if (IsCurrent(generation))
                {
                    _isTransitioning = false;
                    FinishTraversal();
                    NotifyCharacterDetached();
                }
            }
            catch (OperationCanceledException) when (!IsCurrent(generation)) { }
            catch (OperationCanceledException)
            {
                ForceDetach(LadderDetachReason.Disposed);
            }
            finally { EndOperation(generation, operationCancellation); }
        }

        public void TickPlayer(in CharacterInput input, float deltaTime)
        {
            TickDrop(deltaTime);
            if (!IsAttached || _isTransitioning || _isExiting || TickPendingAction()) return;
            if (input.FirstAction.HasValue && HandlePlayerAction(input.FirstAction.Value)) return;
            if (input.SecondAction.HasValue && HandlePlayerAction(input.SecondAction.Value)) return;
            TickTraversal(input.MoveInput.y, input.SprintHeld, HasDropAction(input), deltaTime);
        }

        public void TickEnemy(Vector3 targetPosition, float deltaTime)
        {
            TickDrop(deltaTime);
            if (!IsAttached || _isTransitioning || _isExiting || TickPendingAction()) return;
            LadderClimber above = CurrentLadder.FindNearestOccupant(this, true);
            LadderClimber below = CurrentLadder.FindNearestOccupant(this, false);
            if (TryEnemyAttack(above, true) || TryEnemyAttack(below, false)) return;
            TickTraversal(targetPosition.y > transform.position.y ? 1f : -1f, false, false, deltaTime);
        }

        public void ForceDetach(LadderDetachReason reason)
        {
            CancelOperation();
            bool stopDrop = reason is LadderDetachReason.Death or LadderDetachReason.Disposed;
            if (!IsAttached)
            {
                _isTransitioning = false;
                _isExiting = false;
                if (stopDrop) _isDropping = false;
                if (!_isDropping)
                {
                    SetLayerWeight(0f);
                    _currentAnimation = 0;
                    RestoreRootMotion(_rootMotionGeneration);
                    NotifyCharacterDetached();
                }
                return;
            }

            LadderView ladder = CurrentLadder;
            CurrentLadder = null;
            ladder.Release(this);
            _isTransitioning = false;
            _isExiting = false;
            _pendingAction = PendingAction.None;
            _impactFired = false;
            _isDropping = reason is LadderDetachReason.Drop or LadderDetachReason.KnockOff;
            NotifyCharacterDetached();
            if (reason is LadderDetachReason.ExitTop or LadderDetachReason.ExitBottom)
            {
                Transform exit = ladder.GetExit(reason == LadderDetachReason.ExitTop ? LadderEnd.Top : LadderEnd.Bottom);
                transform.SetPositionAndRotation(exit.position, exit.rotation);
                FinishTraversal();
                return;
            }

            if (!_isDropping)
            {
                FinishTraversal();
                _enemyMotor?.ResumeAfterTraversal();
            }
        }

        private void TickTraversal(float input, bool sprintHeld, bool dropRequested, float deltaTime)
        {
            if (dropRequested) { ForceDetach(LadderDetachReason.Drop); return; }
            if (input > 0.01f) MoveAlongLadder((sprintHeld ? fastClimbSpeed : climbSpeed) * deltaTime, _climbUp);
            else if (input < -0.01f) MoveAlongLadder(-(sprintHeld ? slideSpeed : climbSpeed) * deltaTime,
                sprintHeld ? _slide : _climbDown);
            else Play(_idle);
        }

        private void MoveAlongLadder(float delta, int animation)
        {
            float requested = DistanceOnLadder + delta;
            DistanceOnLadder = CurrentLadder.ClampDistance(this, requested);
            Play(animation);
            SnapToLadder();
            if (requested >= CurrentLadder.Length && DistanceOnLadder >= CurrentLadder.Length)
                ExitAsync(LadderDetachReason.ExitTop).Forget();
            else if (requested <= 0f && DistanceOnLadder <= 0f)
                ExitAsync(LadderDetachReason.ExitBottom).Forget();
        }

        private async UniTask ExitAsync(LadderDetachReason reason)
        {
            if (_isExiting || !IsAttached) return;
            int generation = BeginOperation(CancellationToken.None, out CancellationToken operationToken, out CancellationTokenSource operationCancellation);
            _isExiting = true;
            _isTransitioning = true;
            Play(reason == LadderDetachReason.ExitTop ? _exitTop : _exitBottom);
            try
            {
                Transform exit = CurrentLadder.GetExit(reason == LadderDetachReason.ExitTop ? LadderEnd.Top : LadderEnd.Bottom);
                await AlignToPositionAsync(exit.position, exit.rotation,
                    reason == LadderDetachReason.ExitTop ? EXIT_TOP_SECONDS : EXIT_BOTTOM_SECONDS, operationToken);
                if (IsCurrent(generation)) ForceDetach(reason);
            }
            catch (OperationCanceledException) when (!IsCurrent(generation)) { }
            finally { EndOperation(generation, operationCancellation); }
        }

        private bool HandlePlayerAction(in CharacterAction action)
        {
            if (action.ActionKind == CharacterAction.Kind.Attack && action.Intent == CharacterAction.AttackIntent.Light)
                return BeginAttack(true);
            if (action.ActionKind == CharacterAction.Kind.Attack && action.Intent == CharacterAction.AttackIntent.Heavy)
                return BeginAttack(false);
            if (action.ActionKind == CharacterAction.Kind.Equipment
                && action.EquipmentAction == CharacterAction.EquipmentKind.UseQuickItem)
            {
                PlayerCharacter character = GetComponent<PlayerCharacter>();
                if (character != null && character.CanUseQuickItemOnLadder())
                {
                    BeginAction(PendingAction.Drink, _drink, DRINK_SECONDS, 0.55f);
                    return true;
                }
            }
            return false;
        }

        private bool TryEnemyAttack(LadderClimber target, bool above)
        {
            if (target == null || target.Entity.EntityType == _entity.EntityType
                || Mathf.Abs(target.DistanceOnLadder - DistanceOnLadder) > attackRange) return false;
            return BeginAttack(above);
        }

        private bool BeginAttack(bool punchAbove)
        {
            if (!_health.TryConsumeStamina(punchAbove ? punchStaminaCost : kickStaminaCost)) return false;
            BeginAction(punchAbove ? PendingAction.Punch : PendingAction.Kick, punchAbove ? _punch : _kick,
                punchAbove ? PUNCH_SECONDS : KICK_SECONDS, 0.45f);
            return true;
        }

        private void BeginAction(PendingAction action, int animation, float duration, float impactProgress)
        {
            _pendingAction = action;
            _impactFired = false;
            _actionEndsAt = Time.time + duration;
            _impactAt = Time.time + duration * impactProgress;
            Play(animation);
        }

        private bool TickPendingAction()
        {
            if (_pendingAction == PendingAction.None) return false;
            if (!_impactFired && Time.time >= _impactAt) { _impactFired = true; ResolveActionImpact(); }
            if (Time.time < _actionEndsAt) return true;
            _pendingAction = PendingAction.None;
            Play(_idle);
            return false;
        }

        private void ResolveActionImpact()
        {
            if (_pendingAction == PendingAction.Drink)
            {
                GetComponent<PlayerCharacter>()?.UseQuickItemOnLadder();
                return;
            }
            bool punchAbove = _pendingAction == PendingAction.Punch;
            LadderClimber target = CurrentLadder.FindNearestOccupant(this, punchAbove);
            if (target == null || Mathf.Abs(target.DistanceOnLadder - DistanceOnLadder) > attackRange
                || !target.Entity.TryGetComponent(out ResolveMeleeHitCommand resolveHit)) return;
            target.IsReceivingLadderAttack = true;
            MeleeHitResult result;
            try
            {
                result = resolveHit.Execute(new MeleeHitRequest(_entity.Id, ItemId.Fist,
                    Mathf.RoundToInt(Time.time * 1000f), transform.position, target.transform.position, 0,
                    new MeleeAttackData { ActionId = punchAbove ? CharacterActionId.LightAttack1 : CharacterActionId.HeavyAttack,
                        HealthDamage = punchAbove ? punchDamage : kickDamage, PoiseDamage = punchAbove ? 12f : 20f,
                        ImpactLevel = punchAbove ? ImpactLevel.Light : ImpactLevel.Heavy }));
            }
            finally { target.IsReceivingLadderAttack = false; }
            if ((!punchAbove && (result.Type is MeleeHitResultType.Hit or MeleeHitResultType.HitFromBack))
                || result.Type is MeleeHitResultType.PoiseStaggered or MeleeHitResultType.StanceBroken or MeleeHitResultType.GuardBroken)
                target.ForceDetach(LadderDetachReason.KnockOff);
        }

        private void TickDrop(float deltaTime)
        {
            if (!_isDropping) return;
            _dropVelocity -= dropGravity * deltaTime;
            characterController.Move(Vector3.up * (_dropVelocity * deltaTime));
            if (!characterController.isGrounded || _dropVelocity >= 0f) return;
            _isDropping = false;
            FinishTraversal();
            _enemyMotor?.ResumeAfterTraversal();
        }

        private int BeginOperation(
            CancellationToken callerToken,
            out CancellationToken token,
            out CancellationTokenSource cancellation)
        {
            CancelOperation();
            cancellation = CancellationTokenSource.CreateLinkedTokenSource(callerToken, destroyCancellationToken);
            _operationCancellation = cancellation;
            token = cancellation.Token;
            return ++_operationGeneration;
        }

        private void CancelOperation()
        {
            _operationGeneration++;
            CancellationTokenSource cancellation = _operationCancellation;
            _operationCancellation = null;
            cancellation?.Cancel();
        }

        private bool IsCurrent(int generation) => generation == _operationGeneration && _operationCancellation != null;

        private void EndOperation(int generation, CancellationTokenSource cancellation)
        {
            if (ReferenceEquals(_operationCancellation, cancellation))
            {
                _operationCancellation = null;
            }
            cancellation.Dispose();
        }

        private async UniTask AlignToPositionAsync(Vector3 position, Quaternion rotation, float duration, CancellationToken token)
        {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            for (float elapsed = 0f; elapsed < duration;)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                transform.SetPositionAndRotation(Vector3.Lerp(startPosition, position, progress), Quaternion.Slerp(startRotation, rotation, progress));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private void SnapToLadder() => transform.SetPositionAndRotation(CurrentLadder.SamplePosition(DistanceOnLadder), CurrentLadder.SampleRotation());

        private void Play(int stateHash)
        {
            SetLayerWeight(1f);
            if (_currentAnimation == stateHash) return;
            _currentAnimation = stateHash;
            animator.CrossFadeInFixedTime(stateHash, TRANSITION_SECONDS, _ladderLayer);
        }

        private void FinishTraversal()
        {
            SetLayerWeight(0f);
            _currentAnimation = 0;
            RestoreRootMotion(_rootMotionGeneration);
        }

        private void SetLayerWeight(float weight) => animator.SetLayerWeight(_ladderLayer, weight);

        private void SuppressRootMotion(int generation)
        {
            if (_rootMotionGeneration < 0)
            {
                _previousApplyRootMotion = animator.applyRootMotion;
                _rootMotionGeneration = generation;
            }
            animator.applyRootMotion = false;
        }

        private void RestoreRootMotion(int generation)
        {
            if (_rootMotionGeneration != generation) return;
            animator.applyRootMotion = _previousApplyRootMotion;
            _rootMotionGeneration = -1;
        }

        private void NotifyCharacterAttached()
        {
            PlayerCharacter character = GetComponent<PlayerCharacter>();
            if (character != null) character.OnLadderAttached();
        }

        private void NotifyCharacterDetached()
        {
            PlayerCharacter character = GetComponent<PlayerCharacter>();
            if (character != null) character.OnLadderDetached();
        }

        private static bool HasDropAction(in CharacterInput input) => input.FirstAction.HasValue
            && input.FirstAction.Value.ActionKind == CharacterAction.Kind.Roll;

        private enum PendingAction { None, Punch, Kick, Drink }
    }
}
