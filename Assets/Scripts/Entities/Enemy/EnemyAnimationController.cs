using SoulsLike.Entities.BaseEntity;
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
        private static readonly int SPEED = Animator.StringToHash("Speed");
        private static readonly int MOVE_X = Animator.StringToHash("MoveX");
        private static readonly int MOVE_Y = Animator.StringToHash("MoveY");
        private static readonly int HIT_TRIGGER = Animator.StringToHash("Hit");

        [SerializeField] private Animator animator;
        [SerializeField] private EnemyNavigationMotor motor;
        [SerializeField] private EnemyActor actor;
        [SerializeField] private MeleeHitboxController meleeHitbox;

        private IEntityLocator _entityLocator;
        private Entity _entity;
        private WeaponDatabase _weaponDatabase;
        private CharacterActionDefinition _queuedFollowUp;

        public CharacterActionDefinition CurrentAction { get; private set; }
        public EnemyActionStatus Status { get; private set; } = EnemyActionStatus.Completed;
        public EnemyActionPhase Phase { get; private set; }
        public bool ComboWindowOpen { get; private set; }
        public bool IsActionRunning => Status == EnemyActionStatus.Running;
        public bool IsHitReactionRunning { get; private set; }

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
            WeaponDatabase weaponDatabase)
        {
            _entityLocator = entityLocator;
            _entity = entity;
            _weaponDatabase = weaponDatabase;
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
            ItemStatSnapshot weaponStats = _weaponDatabase
                .GetRequired(actor.Moveset.WeaponId)
                .Stats;
            meleeHitbox.Open(
                actionId,
                weaponStats.PhysicalAttack * CurrentAction.DamageMultiplier);
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

        public void PlayHit()
        {
            Interrupt();
            motor.Stop();
            IsHitReactionRunning = true;
            animator.SetTrigger(HIT_TRIGGER);
        }

        public void ReportHitEntered()
        {
            IsHitReactionRunning = true;
        }

        public void ReportHitExited()
        {
            IsHitReactionRunning = false;
        }

        public void PlayDeath()
        {
            Interrupt();
            BeginAction(actor.Moveset.GetAction(CharacterActionId.Death));
        }

        private void OnAnimatorMove()
        {
            if (IsActionRunning && CurrentAction.UsesRootMotion)
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
    }
}
