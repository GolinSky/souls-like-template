using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Ladder;
using SoulsLike.Entities.Combat;
using SoulsLike.Items;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyActionExecutor : MonoBehaviour, IInitializable
    {
        private const float ACTION_TRANSITION_SECONDS = 0.08f;
        private const float ACTION_ENTRY_TIMEOUT_SECONDS = 1f;
        private static readonly int SPEED = Animator.StringToHash("Speed");
        private static readonly int MOVE_X = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Y = Animator.StringToHash("MoveY");
        private static readonly int HIT_FRONT_TRIGGER = Animator.StringToHash("HitFront");
        private static readonly int HIT_BACK_TRIGGER = Animator.StringToHash("HitBack");
        private static readonly int HIT_LEFT_TRIGGER = Animator.StringToHash("HitLeft");
        private static readonly int HIT_RIGHT_TRIGGER = Animator.StringToHash("HitRight");
        private static readonly int BLOCKED_TRIGGER = Animator.StringToHash("Blocked");
        private static readonly int GUARD_BROKEN_TRIGGER = Animator.StringToHash("GuardBroken");
        private static readonly int PARRIED_TRIGGER = Animator.StringToHash("Parried");
        private static readonly int POISE_STAGGERED_TRIGGER = Animator.StringToHash("PoiseStaggered");
        private static readonly int STANCE_BROKEN_TRIGGER = Animator.StringToHash("StanceBroken");
        private static readonly int CRITICAL_HIT_ONE_HAND_TRIGGER = Animator.StringToHash("CriticalHitOneHand");
        private static readonly int CRITICAL_HIT_ONE_HAND_DIE_TRIGGER = Animator.StringToHash("CriticalHitOneHandDie");
        private static readonly int CRITICAL_HIT_TWO_HAND_TRIGGER = Animator.StringToHash("CriticalHitTwoHand");
        private static readonly int CRITICAL_HIT_TWO_HAND_DIE_TRIGGER = Animator.StringToHash("CriticalHitTwoHandDie");
        private static readonly int GET_UP_TRIGGER = Animator.StringToHash("GetUp");

        [SerializeField] private Animator animator;
        [SerializeField] private EnemyNavigationMotor motor;
        [SerializeField] private EnemyActor actor;
        [SerializeField] private MeleeHitboxController meleeHitbox;

        private IEntityLocator _entityLocator;
        private Entity _entity;
        private WeaponDatabase _weaponDatabase;
        private CombatDefenseComponent _defense;
        private IHealthComponent _health;
        private LadderClimber _ladderClimber;
        private EnemyMove _queuedMove;
        private CharacterActionDefinition _forcedAction;
        private bool _isInitialized;
        private bool _isMeleeHitboxOpen;
        private bool _currentMoveStarted;
        private float _pendingMoveEntryDeadline;
        private bool _recoveryRequested;
        private bool _criticalDeathCompleted;

        public EnemyMove CurrentMove { get; private set; }
        public bool CurrentMoveStarted => _currentMoveStarted;
        public CharacterActionDefinition CurrentAction => CurrentMove?.Action ?? _forcedAction;
        public EnemyExecutionMode Mode { get; private set; } = EnemyExecutionMode.Locomotion;
        public EnemyActionPhase Phase { get; private set; }
        public bool ComboWindowOpen { get; private set; }
        public bool TrackingOpen { get; private set; }
        public bool IsActionRunning => Mode == EnemyExecutionMode.Action;
        public bool IsHitReactionRunning => Mode == EnemyExecutionMode.Reaction;
        public bool IsCriticalVictimRunning => Mode == EnemyExecutionMode.CriticalVictim;
        public bool IsCriticalVictimLethal { get; private set; }
        public bool BlocksDecisions => Mode is EnemyExecutionMode.Action
            or EnemyExecutionMode.Reaction
            or EnemyExecutionMode.CriticalVictim
            or EnemyExecutionMode.GetUp
            or EnemyExecutionMode.Death;
        public event System.Action<EnemyMove> ActionStarted;
        public event System.Action<EnemyMove> ActionCompleted;
        public event System.Action<EnemyInterruptReason> Interrupted;

        public float CurrentTurnSpeed => Phase switch
        {
            EnemyActionPhase.Windup => CurrentAction?.WindupTurnSpeed ?? 0f,
            EnemyActionPhase.Active => CurrentAction?.ActiveTurnSpeed ?? 0f,
            EnemyActionPhase.Recovery => CurrentAction?.RecoveryTurnSpeed ?? 0f,
            _ => 0f
        };

        [Inject]
        public void Construct(
            IEntityLocator entityLocator,
            Entity entity,
            WeaponDatabase weaponDatabase,
            CombatDefenseComponent defense,
            IHealthComponent health,
            LadderClimber ladderClimber)
        {
            _entityLocator = entityLocator;
            _entity = entity;
            _weaponDatabase = weaponDatabase;
            _defense = defense;
            _health = health;
            _ladderClimber = ladderClimber;
        }

        public void Initialize()
        {
            animator.runtimeAnimatorController = actor.Moveset.AnimatorController;
            animator.applyRootMotion = true;
            animator.Rebind();
            animator.Update(0f);
            meleeHitbox.Initialize(
                _entityLocator,
                _entity.Id,
                actor.Moveset.WeaponId);
            meleeHitbox.OnHitResolved += OnMeleeHitResolved;
            _defense.OnHitResolved += OnDefenseHitResolved;
            _isInitialized = true;
        }

        public bool TryStart(EnemyMove move)
        {
            if (Mode != EnemyExecutionMode.Locomotion || move == null || move.Action == null)
            {
                return false;
            }

            BeginMove(move);
            return true;
        }

        public bool TryQueue(EnemyMove move)
        {
            if (Mode != EnemyExecutionMode.Action
                || !ComboWindowOpen
                || move == null
                || move.MoveUsage == EnemyMove.Usage.Opener
                || !IsAuthoredFollowUp(move))
            {
                return false;
            }

            _queuedMove = move;
            ComboWindowOpen = false;
            return true;
        }

        public void Tick(float now)
        {
            if (Mode == EnemyExecutionMode.Action
                && CurrentMove != null
                && !_currentMoveStarted
                && now >= _pendingMoveEntryDeadline)
            {
                Interrupt(EnemyInterruptReason.AnimatorEntryTimeout);
            }
        }

        public void SetLocomotion(Vector3 localVelocity)
        {
            Vector3 planarVelocity = new Vector3(localVelocity.x, 0f, localVelocity.z);
            animator.SetFloat(SPEED, planarVelocity.magnitude);
            animator.SetFloat(MOVE_X, planarVelocity.x);
            animator.SetFloat(MOVE_Y, planarVelocity.z);
        }

        public void ReportStateEntered(CharacterActionId actionId)
        {
            if (!IsExecutingAction())
            {
                return;
            }

            if (!IsCurrent(actionId))
            {
                ReportAnimatorMismatch(actionId);
                return;
            }

            Phase = EnemyActionPhase.Windup;
            motor.SetRootMotion(CurrentAction.UsesRootMotion);
            _pendingMoveEntryDeadline = 0f;
            if (Mode == EnemyExecutionMode.Action
                && CurrentMove != null
                && !_currentMoveStarted)
            {
                _currentMoveStarted = true;
                ActionStarted?.Invoke(CurrentMove);
            }
        }

        public void ReportActiveStarted(CharacterActionId actionId) =>
            ReportActiveStarted(actionId, 0);

        public void ReportActiveStarted(CharacterActionId actionId, int hitIndex)
        {
            if (!IsCurrentExecution(actionId))
            {
                return;
            }

            CharacterActionHitDefinition hitDefinition = CurrentAction.GetHitDefinition(hitIndex);
            if (CurrentAction.HitDefinitions != null
                && CurrentAction.HitDefinitions.Length > 0
                && (hitIndex < 0 || hitIndex >= CurrentAction.HitDefinitions.Length))
            {
                return;
            }

            Phase = EnemyActionPhase.Active;
            WeaponDefinition weaponDefinition = _weaponDatabase.GetRequired(actor.Moveset.WeaponId);
            meleeHitbox.Open(new MeleeAttackData
            {
                ActionId = actionId,
                HealthDamage = weaponDefinition.Stats.PhysicalAttack
                    * hitDefinition.DamageMultiplier,
                GuardDamage = hitDefinition.GuardDamage,
                PoiseDamage = hitDefinition.PoiseDamage,
                StanceDamage = hitDefinition.StanceDamage,
                ImpactLevel = hitDefinition.ImpactLevel,
                CanBeBlocked = hitDefinition.CanBeBlocked,
                CanBeParried = hitDefinition.CanBeParried
            });
            _isMeleeHitboxOpen = true;
        }

        public void ReportActiveEnded(CharacterActionId actionId)
        {
            if (IsCurrentExecution(actionId))
            {
                CloseMeleeHitbox();
            }
        }

        public void ReportComboWindow(CharacterActionId actionId, bool isOpen)
        {
            if (Mode == EnemyExecutionMode.Action
                && _currentMoveStarted
                && IsCurrent(actionId))
            {
                ComboWindowOpen = isOpen;
            }
        }

        public void ReportTrackingWindow(CharacterActionId actionId, bool isOpen)
        {
            if (IsCurrentExecution(actionId))
            {
                TrackingOpen = isOpen;
            }
        }

        public void ReportRecoveryStarted(CharacterActionId actionId)
        {
            if (IsCurrentExecution(actionId))
            {
                Phase = EnemyActionPhase.Recovery;
            }
        }

        public void ReportHyperArmor(bool isActive, float poiseBonus, bool canBeInterrupted)
        {
            if (Mode == EnemyExecutionMode.Action
                && _currentMoveStarted
                && CurrentAction != null)
            {
                _defense.SetHyperArmor(isActive, poiseBonus, canBeInterrupted);
            }
        }

        public void ReportStateExited(CharacterActionId actionId)
        {
            if (!IsCurrentExecution(actionId))
            {
                return;
            }

            CloseMeleeHitbox();
            ComboWindowOpen = false;
            TrackingOpen = false;
            _defense.SetHyperArmor(false);
            if (Mode == EnemyExecutionMode.Action && _queuedMove != null)
            {
                EnemyMove followUp = _queuedMove;
                _queuedMove = null;
                NotifyActionCompleted();
                BeginMove(followUp);
                return;
            }

            CompleteAction();
        }

        public void Interrupt(EnemyInterruptReason reason)
        {
            bool interruptedAction = Mode == EnemyExecutionMode.Action
                && CurrentMove != null;
            _queuedMove = null;
            CloseMeleeHitbox();
            ComboWindowOpen = false;
            TrackingOpen = false;
            motor.SetRootMotion(false);
            _defense.SetHyperArmor(false);
            _defense.SetHitReaction(false);
            _defense.SetParryStunned(false);
            _defense.SetParryWindowActive(false);
            ClearTransientProtection();
            Phase = EnemyActionPhase.None;
            CurrentMove = null;
            _forcedAction = null;
            _currentMoveStarted = false;
            _pendingMoveEntryDeadline = 0f;
            IsCriticalVictimLethal = false;
            _criticalDeathCompleted = false;
            Mode = EnemyExecutionMode.Locomotion;
            if (interruptedAction)
            {
                Interrupted?.Invoke(reason);
            }
        }

        public void PlayHit(in MeleeHitResult result)
        {
            Interrupt(EnemyInterruptReason.Reaction);
            Mode = EnemyExecutionMode.Reaction;
            motor.Stop();
            motor.SetRootMotion(true);
            _defense.SetHitReaction(true);
            if (result.Type == MeleeHitResultType.Parried)
            {
                _defense.SetParryStunned(true);
            }
            animator.SetTrigger(GetHitTrigger(result));
        }

        public void ReportHitEntered()
        {
            if (Mode != EnemyExecutionMode.Reaction)
            {
                return;
            }

            _defense.SetHitReaction(true);
            motor.SetRootMotion(true);
        }

        public void ReportHitExited()
        {
            if (Mode == EnemyExecutionMode.Reaction)
            {
                Interrupt(EnemyInterruptReason.ReactionComplete);
            }
        }

        public void PlayDeath()
        {
            if (Mode == EnemyExecutionMode.Death)
            {
                return;
            }

            Interrupt(EnemyInterruptReason.Death);
            BeginForcedAction(actor.Moveset.DeathAction, EnemyExecutionMode.Death);
        }

        public void BeginCriticalVictim(HandMode handMode, bool lethal)
        {
            Interrupt(EnemyInterruptReason.CriticalVictim);
            _defense.ResetStance();
            _defense.SetCriticalState(true);
            motor.Stop();
            motor.SetRootMotion(false);
            Mode = EnemyExecutionMode.CriticalVictim;
            IsCriticalVictimLethal = lethal;
            _recoveryRequested = false;
            _criticalDeathCompleted = false;
            ResetCriticalTriggers();
            animator.SetTrigger(ResolveCriticalVictimTrigger(handMode, lethal));
        }

        public void CompleteCriticalVictim()
        {
            if (Mode != EnemyExecutionMode.CriticalVictim)
            {
                return;
            }

            if (IsCriticalVictimLethal)
            {
                _criticalDeathCompleted = true;
                ClearTransientProtection();
                Mode = EnemyExecutionMode.Death;
            }
            else
            {
                _recoveryRequested = true;
                animator.SetTrigger(GET_UP_TRIGGER);
            }
        }

        public void ReportCriticalVictimEntered(bool lethal)
        {
            if (Mode != EnemyExecutionMode.CriticalVictim)
            {
                return;
            }

            _defense.SetCriticalState(true);
            motor.Stop();
            motor.SetRootMotion(false);
        }

        public void ReportCriticalVictimExited(bool lethal)
        {
            if (lethal)
            {
                _criticalDeathCompleted = true;
                if (Mode == EnemyExecutionMode.CriticalVictim || Mode == EnemyExecutionMode.Death)
                {
                    ClearTransientProtection();
                    Mode = EnemyExecutionMode.Death;
                }
            }
        }

        public void ReportGetUpEntered()
        {
            Mode = EnemyExecutionMode.GetUp;
            _defense.SetCriticalState(false);
            _health?.SetRecoveryInvulnerable(true);
            motor.Stop();
            motor.SetRootMotion(false);
            CloseMeleeHitbox();
            ComboWindowOpen = false;
            TrackingOpen = false;
            _defense.SetHyperArmor(false);
        }

        public void ReportGetUpExited()
        {
            if (Mode == EnemyExecutionMode.GetUp)
            {
                ClearTransientProtection();
                Mode = EnemyExecutionMode.Locomotion;
            }
        }

        private void ClearTransientProtection()
        {
            _health?.SetRecoveryInvulnerable(false);
            _defense?.SetCriticalState(false);
            _recoveryRequested = false;
            ResetCriticalTriggers();
        }

        private void OnAnimatorMove()
        {
            if (Mode is EnemyExecutionMode.CriticalVictim or EnemyExecutionMode.GetUp)
            {
                return;
            }

            if (!actor.BehaviourProfile.RemainsStationary
                && (Mode == EnemyExecutionMode.Reaction
                    || IsExecutingAction() && CurrentAction.UsesRootMotion))
            {
                motor.ApplyRootMotion(animator.deltaPosition);
            }
        }

        private void BeginMove(EnemyMove move)
        {
            CurrentMove = move;
            _forcedAction = null;
            _currentMoveStarted = false;
            _pendingMoveEntryDeadline = Time.time + ACTION_ENTRY_TIMEOUT_SECONDS;
            BeginAction(move.Action, EnemyExecutionMode.Action);
        }

        private void BeginForcedAction(CharacterActionDefinition action, EnemyExecutionMode mode)
        {
            CurrentMove = null;
            _forcedAction = action;
            _currentMoveStarted = false;
            _pendingMoveEntryDeadline = 0f;
            BeginAction(action, mode);
        }

        private void BeginAction(CharacterActionDefinition action, EnemyExecutionMode mode)
        {
            Mode = mode;
            Phase = EnemyActionPhase.Windup;
            ComboWindowOpen = false;
            TrackingOpen = false;
            motor.Stop();
            motor.SetRootMotion(action.UsesRootMotion);
            animator.CrossFadeInFixedTime(
                action.ActionId.ToString(),
                ACTION_TRANSITION_SECONDS);
        }

        private void CompleteAction()
        {
            EnemyMove completedMove = CurrentMove;
            bool completedStarted = _currentMoveStarted;
            motor.SetRootMotion(false);
            Phase = EnemyActionPhase.None;
            TrackingOpen = false;
            CurrentMove = null;
            _forcedAction = null;
            _currentMoveStarted = false;
            _pendingMoveEntryDeadline = 0f;
            Mode = EnemyExecutionMode.Locomotion;
            if (completedMove != null && completedStarted)
            {
                ActionCompleted?.Invoke(completedMove);
            }
        }

        private void NotifyActionCompleted()
        {
            if (CurrentMove != null && _currentMoveStarted)
            {
                ActionCompleted?.Invoke(CurrentMove);
            }
        }

        private bool IsExecutingAction() => Mode is EnemyExecutionMode.Action or EnemyExecutionMode.Death;

        private bool IsCurrentExecution(CharacterActionId actionId) =>
            IsExecutingAction()
            && IsCurrent(actionId)
            && (Mode != EnemyExecutionMode.Action || _currentMoveStarted);

        private bool IsCurrent(CharacterActionId actionId) =>
            CurrentAction != null && CurrentAction.ActionId == actionId;

        private void CloseMeleeHitbox()
        {
            if (!_isMeleeHitboxOpen)
            {
                return;
            }

            _isMeleeHitboxOpen = false;
            meleeHitbox.Close();
        }

        private bool IsAuthoredFollowUp(EnemyMove move)
        {
            if (CurrentMove == null
                || move == null
                || move.MoveUsage == EnemyMove.Usage.Opener)
            {
                return false;
            }

            foreach (CharacterActionDefinition followUp in CurrentAction.FollowUps)
            {
                if (followUp == move.Action)
                {
                    return true;
                }
            }

            return false;
        }

        private void ReportAnimatorMismatch(CharacterActionId actionId)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError($"Enemy action animator state '{actionId}' did not match the current action.", this);
#endif
            Interrupt(EnemyInterruptReason.AnimatorMismatch);
        }

        private static int ResolveCriticalVictimTrigger(HandMode handMode, bool lethal)
        {
            return handMode switch
            {
                HandMode.OneHanded => lethal
                    ? CRITICAL_HIT_ONE_HAND_DIE_TRIGGER
                    : CRITICAL_HIT_ONE_HAND_TRIGGER,
                HandMode.TwoHanded => lethal
                    ? CRITICAL_HIT_TWO_HAND_DIE_TRIGGER
                    : CRITICAL_HIT_TWO_HAND_TRIGGER,
                _ => throw new System.ArgumentOutOfRangeException(nameof(handMode), handMode, null)
            };
        }

        private void ResetCriticalTriggers()
        {
            animator.ResetTrigger(CRITICAL_HIT_ONE_HAND_TRIGGER);
            animator.ResetTrigger(CRITICAL_HIT_ONE_HAND_DIE_TRIGGER);
            animator.ResetTrigger(CRITICAL_HIT_TWO_HAND_TRIGGER);
            animator.ResetTrigger(CRITICAL_HIT_TWO_HAND_DIE_TRIGGER);
            animator.ResetTrigger(GET_UP_TRIGGER);
        }

        private void OnMeleeHitResolved(MeleeHitResult result)
        {
            if (IsAttackerParried(result.Type))
            {
                PlayHit(result);
            }
        }

        private static bool IsAttackerParried(MeleeHitResultType resultType) =>
            resultType == MeleeHitResultType.Parried;

        private void OnDefenseHitResolved(MeleeHitResult result)
        {
            switch (ResolveDefenderReaction(result.Type))
            {
                case DefenderReaction.None:
                    return;
                case DefenderReaction.Authored:
                case DefenderReaction.Forced:
                    if (result.Type is MeleeHitResultType.PoiseStaggered
                        or MeleeHitResultType.StanceBroken
                        or MeleeHitResultType.GuardBroken)
                    {
                        _ladderClimber.ForceDetach(LadderDetachReason.KnockOff);
                    }
                    PlayHit(result);
                    return;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        private static DefenderReaction ResolveDefenderReaction(MeleeHitResultType resultType) =>
            resultType switch
            {
                MeleeHitResultType.Blocked => DefenderReaction.Authored,
                MeleeHitResultType.PoiseStaggered
                    or MeleeHitResultType.StanceBroken
                    or MeleeHitResultType.GuardBroken => DefenderReaction.Forced,
                _ => DefenderReaction.None
            };

        private enum DefenderReaction { None, Authored, Forced }

        private static int GetHitTrigger(in MeleeHitResult result)
        {
            return result.Type switch
            {
                MeleeHitResultType.Blocked => BLOCKED_TRIGGER,
                MeleeHitResultType.GuardBroken => GUARD_BROKEN_TRIGGER,
                MeleeHitResultType.Parried => PARRIED_TRIGGER,
                MeleeHitResultType.PoiseStaggered => POISE_STAGGERED_TRIGGER,
                MeleeHitResultType.StanceBroken => STANCE_BROKEN_TRIGGER,
                _ => result.Direction switch
                {
                    HitDirection.Front => HIT_FRONT_TRIGGER,
                    HitDirection.Back => HIT_BACK_TRIGGER,
                    HitDirection.Left => HIT_LEFT_TRIGGER,
                    HitDirection.Right => HIT_RIGHT_TRIGGER,
                    _ => throw new System.ArgumentOutOfRangeException(
                        nameof(result.Direction), result.Direction, null)
                }
            };
        }

        private void OnDestroy()
        {
            if (!_isInitialized)
            {
                return;
            }

            Interrupt(EnemyInterruptReason.Despawned);
            meleeHitbox.OnHitResolved -= OnMeleeHitResolved;
            _defense.OnHitResolved -= OnDefenseHitResolved;
        }

        private void OnDisable()
        {
            if (_isInitialized)
            {
                Interrupt(EnemyInterruptReason.Disabled);
            }
        }
    }
}
