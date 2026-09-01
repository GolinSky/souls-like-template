using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Combat;
using SoulsLike.Items;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyAnimationController : MonoBehaviour, IInitializable
    {
        private const float ACTION_TRANSITION_SECONDS = 0.08f;
        private const string BASE_LAYER_PREFIX = "Base Layer.";
        private const string CRITICAL_HIT_ONE_HAND_STATE = "Base Layer.CriticalHitOneHand";
        private const string CRITICAL_HIT_ONE_HAND_DIE_STATE = "Base Layer.CriticalHitOneHandDie";
        private const string CRITICAL_HIT_TWO_HAND_STATE = "Base Layer.CriticalHitTwoHand";
        private const string CRITICAL_HIT_TWO_HAND_DIE_STATE = "Base Layer.CriticalHitTwoHandDie";
        private const string LOCOMOTION_STATE = "Base Layer.Locomotion";
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

        [SerializeField] private Animator animator;
        [SerializeField] private EnemyNavigationMotor motor;
        [SerializeField] private EnemyActor actor;
        [SerializeField] private MeleeHitboxController meleeHitbox;

        private IEntityLocator _entityLocator;
        private Entity _entity;
        private WeaponDatabase _weaponDatabase;
        private CombatDefenseComponent _defense;
        private CharacterActionDefinition _queuedFollowUp;

        public CharacterActionDefinition CurrentAction { get; private set; }
        public EnemyActionStatus Status { get; private set; } = EnemyActionStatus.Completed;
        public EnemyActionPhase Phase { get; private set; }
        public bool ComboWindowOpen { get; private set; }
        public bool IsActionRunning => Status == EnemyActionStatus.Running;
        public bool IsHitReactionRunning { get; private set; }
        public bool IsCriticalVictimRunning { get; private set; }
        public bool IsCriticalVictimLethal { get; private set; }

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
            CombatDefenseComponent defense)
        {
            _entityLocator = entityLocator;
            _entity = entity;
            _weaponDatabase = weaponDatabase;
            _defense = defense;
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
        }

        public bool PlayAction(CharacterActionDefinition action)
        {
            if (IsActionRunning)
            {
                return false;
            }

            BeginAction(action);
            return true;
        }

        public bool QueueFollowUp(CharacterActionDefinition action)
        {
            if (!IsActionRunning || !ComboWindowOpen || !IsAuthoredFollowUp(action))
            {
                return false;
            }

            _queuedFollowUp = action;
            ComboWindowOpen = false;
            return true;
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
            if (CurrentAction == null || CurrentAction.ActionId != actionId)
            {
                CurrentAction = actor.Moveset.GetAction(actionId);
            }

            Status = EnemyActionStatus.Running;
            Phase = EnemyActionPhase.Windup;
            motor.SetRootMotion(CurrentAction.UsesRootMotion);
        }

        public void ReportActiveStarted(CharacterActionId actionId)
        {
            if (!IsCurrent(actionId))
            {
                return;
            }

            Phase = EnemyActionPhase.Active;
            WeaponDefinition weaponDefinition = _weaponDatabase.GetRequired(actor.Moveset.WeaponId);
            meleeHitbox.Open(new MeleeAttackData
            {
                ActionId = actionId,
                HealthDamage = weaponDefinition.Stats.PhysicalAttack
                    * CurrentAction.DamageMultiplier,
                GuardDamage = CurrentAction.GuardDamage,
                PoiseDamage = CurrentAction.PoiseDamage,
                StanceDamage = CurrentAction.StanceDamage,
                ImpactLevel = CurrentAction.ImpactLevel,
                CanBeBlocked = CurrentAction.CanBeBlocked,
                CanBeParried = CurrentAction.CanBeParried
            });
        }

        public void ReportActiveEnded(CharacterActionId actionId)
        {
            if (!IsCurrent(actionId))
            {
                return;
            }

            meleeHitbox.Close();
        }

        public void ReportComboWindow(CharacterActionId actionId, bool isOpen)
        {
            if (IsCurrent(actionId))
            {
                ComboWindowOpen = isOpen;
            }
        }

        public void ReportRecoveryStarted(CharacterActionId actionId)
        {
            if (IsCurrent(actionId))
            {
                Phase = EnemyActionPhase.Recovery;
            }
        }

        public void ReportStateExited(CharacterActionId actionId)
        {
            if (!IsCurrent(actionId))
            {
                return;
            }

            meleeHitbox.Close();
            ComboWindowOpen = false;
            if (_queuedFollowUp != null)
            {
                CharacterActionDefinition followUp = _queuedFollowUp;
                _queuedFollowUp = null;
                BeginAction(followUp);
                return;
            }

            motor.SetRootMotion(false);
            Phase = EnemyActionPhase.None;
            Status = EnemyActionStatus.Completed;
            CurrentAction = null;
        }

        public void Interrupt()
        {
            _queuedFollowUp = null;
            meleeHitbox.Close();
            ComboWindowOpen = false;
            IsHitReactionRunning = false;
            motor.SetRootMotion(false);
            Phase = EnemyActionPhase.None;
            Status = EnemyActionStatus.Interrupted;
            CurrentAction = null;
        }

        public void PlayHit(in MeleeHitResult result)
        {
            Interrupt();
            motor.Stop();
            motor.SetRootMotion(true);
            IsHitReactionRunning = true;
            animator.SetTrigger(GetHitTrigger(result));
        }

        public void TriggerHitReaction(in MeleeHitResult result)
        {
            animator.SetTrigger(GetHitTrigger(result));
        }

        public void ReportHitEntered()
        {
            IsHitReactionRunning = true;
            _defense.SetHitReaction(true);
            motor.SetRootMotion(true);
        }

        public void ReportHitExited()
        {
            IsHitReactionRunning = false;
            _defense.SetHitReaction(false);
            _defense.SetParryStunned(false);
            motor.SetRootMotion(false);
        }

        public void PlayDeath()
        {
            Interrupt();
            BeginAction(actor.Moveset.GetAction(CharacterActionId.Death));
        }

        public void PlayCriticalVictim(HandMode handMode, bool lethal)
        {
            Interrupt();
            motor.Stop();
            motor.SetRootMotion(false);
            IsCriticalVictimRunning = true;
            IsCriticalVictimLethal = lethal;
            animator.CrossFadeInFixedTime(
                ResolveCriticalVictimState(handMode, lethal),
                ACTION_TRANSITION_SECONDS);
        }

        public void CompleteCriticalVictim()
        {
            if (!IsCriticalVictimRunning)
            {
                return;
            }

            bool lethal = IsCriticalVictimLethal;
            IsCriticalVictimRunning = false;
            IsCriticalVictimLethal = false;
            if (!lethal)
            {
                animator.CrossFadeInFixedTime(LOCOMOTION_STATE, ACTION_TRANSITION_SECONDS);
            }
        }

        private void OnAnimatorMove()
        {
            if (IsCriticalVictimRunning)
            {
                return;
            }

            if (IsHitReactionRunning ||
                IsActionRunning && CurrentAction.UsesRootMotion)
            {
                motor.ApplyRootMotion(animator.deltaPosition);
            }
        }

        private void BeginAction(CharacterActionDefinition action)
        {
            CurrentAction = action;
            Status = EnemyActionStatus.Running;
            Phase = EnemyActionPhase.Windup;
            ComboWindowOpen = false;
            motor.Stop();
            motor.SetRootMotion(action.UsesRootMotion);
            animator.CrossFadeInFixedTime(
                BASE_LAYER_PREFIX + action.ActionId,
                ACTION_TRANSITION_SECONDS);
        }

        private bool IsCurrent(CharacterActionId actionId) =>
            CurrentAction != null && CurrentAction.ActionId == actionId;

        private bool IsAuthoredFollowUp(CharacterActionDefinition action)
        {
            foreach (CharacterActionDefinition followUp in CurrentAction.FollowUps)
            {
                if (followUp == action)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolveCriticalVictimState(HandMode handMode, bool lethal)
        {
            return handMode switch
            {
                HandMode.OneHanded => lethal
                    ? CRITICAL_HIT_ONE_HAND_DIE_STATE
                    : CRITICAL_HIT_ONE_HAND_STATE,
                HandMode.TwoHanded => lethal
                    ? CRITICAL_HIT_TWO_HAND_DIE_STATE
                    : CRITICAL_HIT_TWO_HAND_STATE,
                _ => throw new System.ArgumentOutOfRangeException(nameof(handMode), handMode, null)
            };
        }

        private void OnMeleeHitResolved(MeleeHitResult result)
        {
            if (result.Type == MeleeHitResultType.Parried)
            {
                Interrupt();
                animator.SetTrigger(PARRIED_TRIGGER);
            }
        }

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
            if (meleeHitbox != null)
            {
                meleeHitbox.OnHitResolved -= OnMeleeHitResolved;
            }
        }
    }
}
