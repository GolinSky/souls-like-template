using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Ladder;
using SoulsLike.Services;
using SoulsLike.Services.Navigation;
using UnityEngine;
using UnityEngine.AI;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyController :
        IInitializable,
        ITickable,
        IGameStateObserver,
        IDisposable
    {
        private const float FACING_ANGLE = 20f;

        private readonly EnemyActor _actor;
        private readonly EnemyNavigationMotor _motor;
        private readonly EnemyActionExecutor _executor;
        private readonly EnemyPerception _perception;
        private readonly EnemyActionSelector _actionSelector;
        private readonly EnemyRandomStreams _randomStreams;
        private readonly EnemyGroupCoordinator _groupCoordinator;
        private readonly HealthModel _healthModel;
        private readonly CombatDefenseComponent _defense;
        private readonly IGameStateNotifier _gameStateNotifier;
        private readonly ICombatStateNotifier _combatStateNotifier;
        private readonly INavMeshService _navMeshService;
        private readonly LadderClimber _ladderClimber;
        private readonly LadderSystem _ladderSystem;
        private readonly IEntityLocator _entityLocator;


        private GameState _gameState;
        private float _nextDecisionTime;
        private float _reactionReadyTime;
        private float _patrolWaitUntil;
        private float _postActionDecisionUntil;
        private float _searchUntil;
        private float _searchPauseUntil;
        private float _firstAttackReadyTime;
        private long? _reactionTargetEntityId;
        private EnemyMemory? _searchMemory;
        private Vector3[] _searchPoints = Array.Empty<Vector3>();
        private int _searchPointIndex;
        private Vector3 _committedAttackPoint;
        private int _patrolIndex;
        private int _startedAttackCount;
        private bool _waitingAtPatrolPoint;
        private bool _hasStartedAttack;
        private bool _deathAnimationStarted;
        private bool _despawned;

        public EnemyController(
            EnemyActor actor,
            EnemyNavigationMotor motor,
            EnemyActionExecutor executor,
            EnemyPerception perception,
            EnemyActionSelector actionSelector,
            EnemyRandomStreams randomStreams,
            EnemyGroupCoordinator groupCoordinator,
            HealthModel healthModel,
            CombatDefenseComponent defense,
            IGameStateNotifier gameStateNotifier,
            ICombatStateNotifier combatStateNotifier,
            INavMeshService navMeshService,
            LadderClimber ladderClimber,
            LadderSystem ladderSystem,
            IEntityLocator entityLocator)
        {
            _actor = actor;
            _motor = motor;
            _executor = executor;
            _perception = perception;
            _actionSelector = actionSelector;
            _randomStreams = randomStreams;
            _groupCoordinator = groupCoordinator;
            _healthModel = healthModel;
            _defense = defense;
            _gameStateNotifier = gameStateNotifier;
            _combatStateNotifier = combatStateNotifier;
            _navMeshService = navMeshService;
            _ladderClimber = ladderClimber;
            _ladderSystem = ladderSystem;
            _entityLocator = entityLocator;
        }

        public EnemyGoal Goal { get; private set; }

        public void Initialize()
        {
            _gameStateNotifier.RegisterObserver(this);
            _healthModel.OnDamageApplied += OnDamageApplied;
            _healthModel.OnDied += OnDied;
            _executor.ActionStarted += OnActionStarted;
            _executor.ActionCompleted += OnActionCompleted;
            _executor.Interrupted += OnActionInterrupted;
            _groupCoordinator.Register(_actor, this);
            Goal = _actor.BehaviourProfile.ActivationMode == EnemyActivationMode.Immediate
                ? _actor.HasPatrolPositions
                    ? EnemyGoal.Patrol
                    : EnemyGoal.Idle
                : EnemyGoal.Dormant;
            OnGameStateChanged(_gameStateNotifier.CurrentGameState);
        }

        public void Dispose()
        {
            if (IsAggroGoal(Goal))
            {
                _combatStateNotifier.ReportEnemyAggroEnded(_actor.Entity.Id);
            }

            _gameStateNotifier.UnregisterObserver(this);
            _healthModel.OnDamageApplied -= OnDamageApplied;
            _healthModel.OnDied -= OnDied;
            _executor.ActionStarted -= OnActionStarted;
            _executor.ActionCompleted -= OnActionCompleted;
            _executor.Interrupted -= OnActionInterrupted;
            _groupCoordinator.ReleasePressureSlot(_actor);
            _groupCoordinator.Unregister(_actor);
        }

        public void OnGameStateChanged(GameState newState)
        {
            _gameState = newState;
            if (newState == GameState.OnGraceSit)
            {
                _motor.Stop();
            }
        }

        private void ActivateDormant()
        {
            if (Goal == EnemyGoal.Dormant)
            {
                EnterGoal(_actor.HasPatrolPositions
                    ? EnemyGoal.Patrol
                    : EnemyGoal.Idle);
            }
        }

        public void ActivateFromTrigger()
        {
            if (_actor.BehaviourProfile.ActivationMode == EnemyActivationMode.Triggered)
            {
                ActivateDormant();
            }
        }

        public void Tick()
        {
            if (_gameState == GameState.OnGraceSit)
            {
                return;
            }

            if (Goal == EnemyGoal.Dead)
            {
                TickDeath();
                return;
            }

            if (Goal == EnemyGoal.Dormant)
            {
                TickDormant(Time.time);
                return;
            }

            float deltaTime = Time.deltaTime;
            if (_ladderClimber.IsBusy)
            {
                if (_perception.TryGetRecentMemory(Time.time, out EnemyMemory ladderTarget)
                    && ladderTarget.EntityId.HasValue
                    && _entityLocator.TryGetEntity(ladderTarget.EntityId.Value, out IEntity targetEntity)
                    && targetEntity.TryGetComponent(out TargetingCommand targeting))
                {
                    _ladderClimber.TickEnemy(targeting.Read().Position, deltaTime);
                }
                else if (_ladderClimber.IsAttached)
                {
                    _ladderClimber.ForceDetach(
                        _ladderClimber.DistanceOnLadder >= _ladderClimber.CurrentLadder.Length * 0.5f
                            ? LadderDetachReason.ExitTop
                            : LadderDetachReason.ExitBottom);
                }

                return;
            }
            _defense.TickRecovery(deltaTime);
            float now = Time.time;
            if (_defense.IsInCriticalState
                || _defense.HasCriticalOpportunity
                || _defense.IsParryStunned
                || _defense.IsGuardBroken)
            {
                _motor.Stop();
                _executor.SetLocomotion(Vector3.zero);
                return;
            }

            if (Goal != EnemyGoal.ReturnHome && IsBeyondHardLeash())
            {
                BeginReturnHome();
                return;
            }

            if (_executor.Mode == EnemyExecutionMode.Action)
            {
                _motor.Tick(deltaTime, Goal != EnemyGoal.Combat);
                _executor.SetLocomotion(_motor.LocalVelocity);
                _executor.Tick(now);
                if (_executor.Mode != EnemyExecutionMode.Action)
                {
                    return;
                }

                TickCommittedAction(now, deltaTime);
                return;
            }

            if (_executor.BlocksDecisions)
            {
                _motor.Stop();
                _executor.SetLocomotion(Vector3.zero);
                return;
            }

            _motor.Tick(deltaTime, Goal != EnemyGoal.Combat);
            _executor.SetLocomotion(_motor.LocalVelocity);

            TickContinuousGoal(now, deltaTime);
            if (now < _nextDecisionTime)
            {
                return;
            }

            _nextDecisionTime = GetNextDecisionTime(now);
            Decide(now);
        }

        private void TickDormant(float now)
        {
            _motor.Stop();
            _executor.SetLocomotion(Vector3.zero);
            if (_actor.BehaviourProfile.ActivationMode != EnemyActivationMode.Perception
                || now < _nextDecisionTime)
            {
                return;
            }

            _nextDecisionTime = GetNextDecisionTime(now);
            if (_perception.TryObserve(
                    _actor.transform.position,
                    _actor.transform.forward,
                    _actor.BehaviourProfile,
                    now,
                    out TargetObservation observation))
            {
                BeginReaction(observation.EntityId, now);
                ActivateDormant();
            }
        }

        private void Decide(float now)
        {
            if (Goal == EnemyGoal.ReturnHome)
            {
                MoveTo(_actor.HomePosition);
                return;
            }

            bool observed = _perception.TryObserve(
                _actor.transform.position,
                _actor.transform.forward,
                _actor.BehaviourProfile,
                now,
                out TargetObservation target);

            if (observed)
            {
                _groupCoordinator.BroadcastAllyAlert(
                    _actor,
                    target.Position,
                    now);
                BeginReaction(target.EntityId, now);
                if (now < _reactionReadyTime)
                {
                    EnterGoal(EnemyGoal.Investigate);
                    _motor.Stop();
                    Face(target.Position, 360f, Time.deltaTime);
                    return;
                }

                EnterGoal(EnemyGoal.Combat);
                DecideCombat(target.Position, target.LockPoint, true, now);
                return;
            }

            if (_perception.TryGetRecentMemory(
                    now,
                    out EnemyMemory memory))
            {
                if (!_perception.IsRememberedTargetAlive())
                {
                    _perception.ClearMemory();
                }
                else if (memory.StimulusType == EnemyStimulusType.Damage)
                {
                    if (!memory.EntityId.HasValue)
                    {
                        BeginInvestigate(memory);
                        return;
                    }

                    BeginReaction(memory.EntityId.Value, now);
                    if (now < _reactionReadyTime)
                    {
                        EnterGoal(EnemyGoal.Investigate);
                        _motor.Stop();
                        Face(memory.LastKnownPosition, 360f, Time.deltaTime);
                        return;
                    }

                    EnterGoal(EnemyGoal.Combat);
                    DecideCombat(
                        memory.LastKnownPosition,
                        memory.LastKnownLockPoint,
                        false,
                        now);
                    return;
                }
                else
                {
                    BeginInvestigate(memory);
                    return;
                }
            }

            if (Goal == EnemyGoal.Search)
            {
                return;
            }

            if (_perception.TryGetRecentMemory(
                    now,
                    out memory))
            {
                BeginInvestigate(memory);
                return;
            }

            if (Goal is EnemyGoal.Combat or EnemyGoal.Investigate)
            {
                BeginSearch(now);
                return;
            }

            if (IsBeyondSoftLeash())
            {
                BeginReturnHome();
                return;
            }

            if (Goal is EnemyGoal.Idle or EnemyGoal.Patrol)
            {
                DecidePatrol(now);
            }
        }

        private void DecideCombat(
            Vector3 targetPosition,
            Vector3 combatTarget,
            bool hasLineOfSight,
            float now)
        {
            if (now < _postActionDecisionUntil)
            {
                return;
            }

            Vector3 toTarget = combatTarget - _actor.transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            float angle = Vector3.Angle(_actor.transform.forward, toTarget);
            EnemyMove move = null;
            EnemyCombatMovement movement = ResolveCombatMovement(
                distance,
                angle,
                hasLineOfSight,
                now,
                out move);
            switch (movement)
            {
                case EnemyCombatMovement.Approach:
                    if (TryStartLadderTraversal(targetPosition))
                    {
                        return;
                    }

                    MoveTo(BiasCombatDestinationTowardHome(targetPosition));
                    return;
                case EnemyCombatMovement.Retreat:
                    _postActionDecisionUntil = 0f;
                    MoveTo(BiasCombatDestinationTowardHome(
                        _actor.transform.position
                        + (toTarget.sqrMagnitude > 0f
                            ? -toTarget.normalized
                            : -_actor.transform.forward)
                        * _actor.BehaviourProfile.StrafeDistance));
                    return;
                case EnemyCombatMovement.CircleLeft:
                case EnemyCombatMovement.CircleRight:
                    CircleCombatTarget(combatTarget, toTarget, movement, now);
                    return;
                case EnemyCombatMovement.Attack:
                    _motor.Stop();
                    if (!CanStartAttack(now))
                    {
                        return;
                    }

                    if (_actor.BehaviourProfile.UsesPressureSlot
                        && !_groupCoordinator.TryAcquirePressureSlot(_actor, now))
                    {
                        CircleCombatTarget(
                            combatTarget,
                            toTarget,
                            _randomStreams.NextMovementBool()
                                ? EnemyCombatMovement.CircleLeft
                                : EnemyCombatMovement.CircleRight,
                            now);
                        return;
                    }

                    if (_executor.TryStart(move))
                    {
                        _committedAttackPoint = combatTarget;
                        FaceImmediately(combatTarget);
                        return;
                    }

                    _groupCoordinator.ReleasePressureSlot(_actor);

                    return;
                case EnemyCombatMovement.Guard:
                case EnemyCombatMovement.Hold:
                    _motor.Stop();
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(movement), movement, null);
            }
        }

        private EnemyCombatMovement ResolveCombatMovement(
            float distance,
            float angle,
            bool hasLineOfSight,
            float now,
            out EnemyMove move)
        {
            move = null;
            if (!hasLineOfSight)
            {
                move = _actionSelector.Choose(
                    _actor.Moveset.Moves,
                    distance,
                    angle,
                    false,
                    null,
                    false,
                    now);
                return move != null
                    ? EnemyCombatMovement.Attack
                    : EnemyCombatMovement.Approach;
            }

            if (distance < _actor.BehaviourProfile.PreferredRangeMin)
            {
                return EnemyCombatMovement.Retreat;
            }

            if (!_actionSelector.IsWithinAnyMoveRange(_actor.Moveset.Moves, distance))
            {
                return EnemyCombatMovement.Approach;
            }

            move = _actionSelector.Choose(
                _actor.Moveset.Moves,
                distance,
                angle,
                hasLineOfSight,
                null,
                false,
                now);
            if (move != null)
            {
                return EnemyCombatMovement.Attack;
            }

            if (angle > FACING_ANGLE)
            {
                return _randomStreams.NextMovementBool()
                    ? EnemyCombatMovement.CircleLeft
                    : EnemyCombatMovement.CircleRight;
            }

            return _randomStreams.NextMovementBool()
                ? EnemyCombatMovement.CircleLeft
                : EnemyCombatMovement.Hold;
        }

        private bool CanStartAttack(float now)
        {
            if (_actor.BehaviourProfile.MaximumAttackCount > 0
                && _startedAttackCount >= _actor.BehaviourProfile.MaximumAttackCount)
            {
                return false;
            }

            if (_defense.HasCriticalOpportunity
                || _defense.IsParryStunned
                || _defense.IsInCriticalState
                || _defense.IsInHitReaction
                || _defense.IsGuardBroken)
            {
                return false;
            }

            if (now < _postActionDecisionUntil)
            {
                return false;
            }

            if (_hasStartedAttack)
            {
                return true;
            }

            if (_firstAttackReadyTime <= 0f)
            {
                _firstAttackReadyTime = now + _randomStreams.TimingRange(
                    _actor.BehaviourProfile.FirstAttackHesitationMin,
                    _actor.BehaviourProfile.FirstAttackHesitationMax);
            }

            return now >= _firstAttackReadyTime;
        }

        private void CircleCombatTarget(
            Vector3 combatTarget,
            Vector3 toTarget,
            EnemyCombatMovement movement,
            float now)
        {
            Face(combatTarget, 360f);
            Vector3 side = Vector3.Cross(Vector3.up, toTarget.normalized);
            if (movement == EnemyCombatMovement.CircleRight)
            {
                side = -side;
            }

            MoveTo(BiasCombatDestinationTowardHome(
                _actor.transform.position
                + side * _actor.BehaviourProfile.StrafeDistance));
        }

        private void TickCommittedAction(float now, float deltaTime)
        {
            if (_actor.BehaviourProfile.UsesPressureSlot
                && _executor.CurrentMoveStarted)
            {
                _groupCoordinator.RenewPressureSlot(_actor, now);
            }

            bool observed = _perception.TryObserve(
                _actor.transform.position,
                _actor.transform.forward,
                _actor.BehaviourProfile,
                now,
                out TargetObservation observation);
            if (observed)
            {
                _groupCoordinator.BroadcastAllyAlert(
                    _actor,
                    observation.Position,
                    now);
            }

            if (!_perception.IsRememberedTargetAlive())
            {
                _perception.ClearMemory();
                _executor.Interrupt(EnemyInterruptReason.LostTarget);
                BeginSearch(now);
                return;
            }

            if (!_perception.TryGetRecentMemory(now, out EnemyMemory memory))
            {
                return;
            }

            if (_executor.TrackingOpen && observed)
            {
                _committedAttackPoint = observation.LockPoint;
            }

            Vector3 lockPoint = _committedAttackPoint;

            if (_executor.TrackingOpen)
            {
                float turnSpeed = _executor.CurrentTurnSpeed;
                if (turnSpeed > 0f)
                {
                    Face(lockPoint, turnSpeed, deltaTime);
                }
            }
            if (!_executor.ComboWindowOpen)
            {
                return;
            }

            Vector3 toTarget = lockPoint - _actor.transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            float angle = Vector3.Angle(_actor.transform.forward, toTarget);
            EnemyMove followUp = _actionSelector.Choose(
                _actor.Moveset.Moves,
                distance,
                angle,
                observed,
                _executor.CurrentMove,
                true,
                now);
            if (followUp != null && _executor.TryQueue(followUp))
            {
                return;
            }
        }

        private void TickContinuousGoal(float now, float deltaTime)
        {
            switch (Goal)
            {
                case EnemyGoal.Combat:
                    if (_perception.TryGetRecentMemory(
                            now,
                            out EnemyMemory memory))
                    {
                        Face(memory.LastKnownLockPoint, 360f, deltaTime);
                    }
                    break;
                case EnemyGoal.Investigate:
                    if (!_perception.TryGetRecentMemory(now, out memory))
                    {
                        BeginReturnHome();
                    }
                    else if (_motor.IsWithin(
                        memory.LastKnownPosition,
                        _actor.BehaviourProfile.ArrivalDistance))
                    {
                        BeginSearch(now);
                    }
                    else
                    {
                        Face(memory.LastKnownPosition, 360f, deltaTime);
                        MoveTo(memory.LastKnownPosition);
                    }
                    break;
                case EnemyGoal.Search:
                    TickSearch(now, deltaTime);
                    break;
                case EnemyGoal.ReturnHome:
                    if (_motor.IsWithin(
                        _actor.HomePosition,
                        _actor.BehaviourProfile.ReturnHomeDistance))
                    {
                        _motor.Stop();
                        _perception.ClearMemory();
                        _reactionTargetEntityId = null;
                        EnterGoal(_actor.HasPatrolPositions
                            ? EnemyGoal.Patrol
                            : EnemyGoal.Idle);
                    }
                    break;
            }
        }

        private void DecidePatrol(float now)
        {
            if (!_actor.HasPatrolPositions)
            {
                EnterGoal(EnemyGoal.Idle);
                WaitAt();
                return;
            }

            EnterGoal(EnemyGoal.Patrol);
            if (!_waitingAtPatrolPoint
                && _motor.IsWithin(
                    _actor.PatrolPoints[_patrolIndex],
                    _actor.BehaviourProfile.ArrivalDistance))
            {
                _waitingAtPatrolPoint = true;
                _patrolWaitUntil = now + _actor.BehaviourProfile.PatrolWaitSeconds;
            }

            if (_waitingAtPatrolPoint && now < _patrolWaitUntil)
            {
                WaitAt();
                return;
            }

            if (_waitingAtPatrolPoint)
            {
                _waitingAtPatrolPoint = false;
                _patrolIndex = (_patrolIndex + 1) % _actor.PatrolPoints.Count;
            }

            MoveTo(_actor.PatrolPoints[_patrolIndex]);
        }

        private void BeginInvestigate(in EnemyMemory memory)
        {
            EnterGoal(EnemyGoal.Investigate);
            MoveTo(memory.LastKnownPosition);
        }

        private void BeginSearch(float now)
        {
            if (!_perception.TryGetRecentMemory(now, out EnemyMemory memory))
            {
                BeginReturnHome();
                return;
            }

            _searchMemory = memory;
            _searchPoints = BuildSearchPoints(memory);
            _searchPointIndex = 0;
            _searchPauseUntil = 0f;
            EnterGoal(EnemyGoal.Search);
        }

        private void BeginReturnHome()
        {
            if (_executor.Mode == EnemyExecutionMode.Action)
            {
                _executor.Interrupt(EnemyInterruptReason.LostTarget);
            }

            _motor.Stop();
            _groupCoordinator.ReleasePressureSlot(_actor);
            EnterGoal(EnemyGoal.ReturnHome);
            MoveTo(_actor.HomePosition);
        }

        private void TickSearch(float now, float deltaTime)
        {
            if (!_perception.TryGetRecentMemory(now, out EnemyMemory memory)
                || now >= _searchUntil)
            {
                BeginReturnHome();
                return;
            }

            if (_searchMemory.HasValue
                && _searchMemory.Value.LastConfirmedTime != memory.LastConfirmedTime)
            {
                BeginSearch(now);
                return;
            }

            if (_searchPointIndex >= _searchPoints.Length)
            {
                _motor.Stop();
                Rotate(_actor.BehaviourProfile.SearchTurnSpeed, deltaTime);
                return;
            }

            Vector3 point = _searchPoints[_searchPointIndex];
            if (!_motor.IsWithin(point, _actor.BehaviourProfile.ArrivalDistance))
            {
                MoveTo(point);
                return;
            }

            _motor.Stop();
            Face(memory.LastKnownPosition, _actor.BehaviourProfile.SearchTurnSpeed);
            if (_searchPauseUntil <= 0f)
            {
                _searchPauseUntil = now + _actor.BehaviourProfile.SearchPauseSeconds;
                return;
            }

            if (now >= _searchPauseUntil)
            {
                _searchPointIndex++;
                _searchPauseUntil = 0f;
            }
        }

        private Vector3[] BuildSearchPoints(in EnemyMemory memory)
        {
            int requestedCount = _actor.BehaviourProfile.SearchPointCount;
            if (requestedCount == 0 || _actor.BehaviourProfile.SearchPointRadius <= 0f)
            {
                return Array.Empty<Vector3>();
            }

            NavMeshAgent agent = _actor.NavMeshAgent;
            var filter = new NavMeshQueryFilter
            {
                agentTypeID = agent.agentTypeID,
                areaMask = agent.areaMask
            };
            Vector3 forward = memory.LastKnownPosition - _actor.transform.position;
            forward.y = 0f;
            if (forward.sqrMagnitude == 0f)
            {
                forward = _actor.transform.forward;
                forward.y = 0f;
            }

            int offset = ((int)memory.StimulusType + (memory.EntityId.HasValue
                ? (int)(memory.EntityId.Value & 3)
                : 0)) & 3;
            var points = new Vector3[requestedCount];
            int count = 0;
            for (int index = 0; index < requestedCount; index++)
            {
                Vector3 direction = Quaternion.AngleAxis(90f * ((offset + index) & 3), Vector3.up)
                    * forward.normalized;
                Vector3 requested = memory.LastKnownPosition
                    + direction * _actor.BehaviourProfile.SearchPointRadius;
                if (_navMeshService.TrySamplePosition(
                        requested,
                        _actor.BehaviourProfile.SearchPointRadius,
                        filter,
                        out NavMeshHit hit))
                {
                    points[count++] = hit.position;
                }
            }

            if (count == requestedCount)
            {
                return points;
            }

            Array.Resize(ref points, count);
            return points;
        }

        private void BeginReaction(long targetEntityId, float now)
        {
            if (_reactionTargetEntityId == targetEntityId)
            {
                return;
            }

            _reactionTargetEntityId = targetEntityId;
            _reactionReadyTime = now + _randomStreams.TimingRange(
                _actor.BehaviourProfile.ReactionDelayMin,
                _actor.BehaviourProfile.ReactionDelayMax);
        }

        private float GetNextDecisionTime(float now) =>
            now + _actor.BehaviourProfile.DecisionInterval
            + _randomStreams.TimingRange(
                0f,
                _actor.BehaviourProfile.DecisionJitterSeconds);

        private void EnterGoal(EnemyGoal goal)
        {
            if (Goal == goal)
            {
                return;
            }

            EnemyGoal previousGoal = Goal;
            Goal = goal;
            ReportCombatState(previousGoal, goal);
            if (goal == EnemyGoal.Combat)
            {
                _postActionDecisionUntil = 0f;
                if (previousGoal != EnemyGoal.Combat)
                {
                    _hasStartedAttack = false;
                    _firstAttackReadyTime = 0f;
                }
            }
            if (goal == EnemyGoal.Search)
            {
                _searchUntil = Time.time + _actor.BehaviourProfile.SearchSeconds;
            }
        }

        private void ReportCombatState(EnemyGoal previousGoal, EnemyGoal goal)
        {
            bool wasAggro = IsAggroGoal(previousGoal);
            bool isAggro = IsAggroGoal(goal);
            if (wasAggro == isAggro)
            {
                return;
            }

            if (isAggro)
            {
                _combatStateNotifier.ReportEnemyAggroStarted(_actor.Entity.Id);
                return;
            }

            _combatStateNotifier.ReportEnemyAggroEnded(_actor.Entity.Id);
        }

        private static bool IsAggroGoal(EnemyGoal goal) =>
            goal is EnemyGoal.Investigate
                or EnemyGoal.Combat;

        private bool IsBeyondHardLeash() =>
            (_actor.transform.position - _actor.HomePosition).sqrMagnitude
            > _actor.BehaviourProfile.HardLeashDistance
            * _actor.BehaviourProfile.HardLeashDistance;

        private bool IsBeyondSoftLeash() =>
            (_actor.transform.position - _actor.HomePosition).sqrMagnitude
            > _actor.BehaviourProfile.SoftLeashDistance
            * _actor.BehaviourProfile.SoftLeashDistance;

        private Vector3 BiasCombatDestinationTowardHome(Vector3 destination)
        {
            float distanceFromHome = Vector3.Distance(
                _actor.transform.position,
                _actor.HomePosition);
            float softLeash = _actor.BehaviourProfile.SoftLeashDistance;
            float hardLeash = _actor.BehaviourProfile.HardLeashDistance;
            if (distanceFromHome <= softLeash || hardLeash <= softLeash)
            {
                return destination;
            }

            float homeBias = Mathf.InverseLerp(softLeash, hardLeash, distanceFromHome);
            return Vector3.Lerp(destination, _actor.HomePosition, homeBias);
        }

        private void MoveTo(Vector3 position)
        {
            if (_actor.BehaviourProfile.RemainsStationary)
            {
                _motor.Stop();
                return;
            }

            _motor.SetDestination(position);
        }

        private bool TryStartLadderTraversal(Vector3 targetPosition)
        {
            if (!_actor.BehaviourProfile.CanUseLadders)
            {
                return false;
            }

            if (_reactionTargetEntityId.HasValue
                && _entityLocator.TryGetEntity(_reactionTargetEntityId.Value, out IEntity targetEntity)
                && targetEntity.TryGetComponent(out LadderClimber targetClimber)
                && targetClimber.IsAttached)
            {
                LadderEnd targetEntry = targetClimber.DistanceOnLadder < targetClimber.CurrentLadder.Length * 0.5f
                    ? LadderEnd.Bottom
                    : LadderEnd.Top;
                return TryReachLadderEntry(targetClimber.CurrentLadder, targetEntry);
            }

            if (!_ladderSystem.TryFindRoute(
                    _actor.transform.position,
                    targetPosition,
                    new NavMeshQueryFilter
                    {
                        agentTypeID = _actor.NavMeshAgent.agentTypeID,
                        areaMask = _actor.NavMeshAgent.areaMask
                    },
                    out LadderView ladder,
                    out LadderEnd entryEnd))
            {
                return false;
            }

            return TryReachLadderEntry(ladder, entryEnd);
        }

        private bool TryReachLadderEntry(LadderView ladder, LadderEnd entryEnd)
        {
            Transform entry = ladder.GetExit(entryEnd);
            NavMeshPath path = new();
            NavMeshQueryFilter filter = new()
            {
                agentTypeID = _actor.NavMeshAgent.agentTypeID,
                areaMask = _actor.NavMeshAgent.areaMask
            };
            if (!NavMesh.CalculatePath(_actor.transform.position, entry.position, filter, path)
                || path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            if (!_motor.IsWithin(entry.position, _actor.BehaviourProfile.ArrivalDistance))
            {
                MoveTo(entry.position);
                return true;
            }

            _ladderClimber.AttachAsync(ladder, entryEnd, CancellationToken.None).Forget();
            return true;
        }

        private void Face(Vector3 position, float turnSpeed)
        {
            Face(position, turnSpeed, Time.deltaTime);
        }

        private void Face(Vector3 position, float turnSpeed, float deltaTime)
        {
            if (_actor.BehaviourProfile.LocksFacing)
            {
                return;
            }

            _motor.Face(position, turnSpeed, deltaTime);
        }

        private void FaceImmediately(Vector3 position)
        {
            if (_actor.BehaviourProfile.LocksFacing)
            {
                return;
            }

            _motor.FaceImmediately(position);
        }

        private void Rotate(float degrees, float deltaTime)
        {
            if (_actor.BehaviourProfile.LocksFacing)
            {
                return;
            }

            _motor.Rotate(degrees, deltaTime);
        }

        private void WaitAt()
        {
            _motor.Stop();
        }

        private void OnDamageApplied(DamageResult damage)
        {
            if (damage.HealthDamageAmount <= 0f)
            {
                return;
            }

            float now = Time.time;
            if (!_perception.RegisterDamageStimulus(
                    damage.SourceEntityId,
                    _actor.BehaviourProfile,
                    now))
            {
                return;
            }

            _groupCoordinator.BroadcastAllyAlert(
                _actor,
                _perception.Memory.Value.LastKnownPosition,
                now);

            if (_perception.Memory.Value.EntityId.HasValue)
            {
                BeginReaction(_perception.Memory.Value.EntityId.Value, now);
            }
            if (Goal == EnemyGoal.Dormant)
            {
                ActivateDormant();
            }
        }

        private void OnDied(long sourceEntityId)
        {
            _ladderClimber.ForceDetach(LadderDetachReason.Death);
            EnterGoal(EnemyGoal.Dead);
            _motor.Stop();
            if (_executor.IsCriticalVictimRunning && _executor.IsCriticalVictimLethal)
            {
                _deathAnimationStarted = true;
                return;
            }

            _executor.Interrupt(EnemyInterruptReason.Death);
        }

        private void OnActionStarted(EnemyMove move)
        {
            _actionSelector.CommitStarted(move, Time.time);
            _hasStartedAttack = true;
            _startedAttackCount++;
        }

        private void OnActionCompleted(EnemyMove move)
        {
            if (_executor.Mode == EnemyExecutionMode.Locomotion)
            {
                _committedAttackPoint = default;
                _groupCoordinator.ReleasePressureSlot(_actor);
                _postActionDecisionUntil = Time.time + _actor.BehaviourProfile.PostActionDecisionDelaySeconds;
            }
        }

        private void OnActionInterrupted(EnemyInterruptReason reason)
        {
            _committedAttackPoint = default;
            _groupCoordinator.ReleasePressureSlot(_actor);
            _postActionDecisionUntil = Time.time + _actor.BehaviourProfile.PostActionDecisionDelaySeconds;
        }

        public void ReceiveAllyAlert(Vector3 position, long sourceEntityId, float now)
        {
            if (_actor.BehaviourProfile.ActivationMode == EnemyActivationMode.Triggered
                || !_perception.RegisterAllyAlertStimulus(
                    position,
                    1f,
                    sourceEntityId,
                    _actor.BehaviourProfile,
                    now))
            {
                return;
            }

            if (Goal == EnemyGoal.Dormant)
            {
                ActivateDormant();
            }

            if (Goal != EnemyGoal.Dead
                && _perception.TryGetRecentMemory(now, out EnemyMemory memory))
            {
                BeginInvestigate(memory);
            }
        }

        private void TickDeath()
        {
            if (_executor.IsCriticalVictimRunning)
            {
                return;
            }

            if (!_deathAnimationStarted)
            {
                _deathAnimationStarted = true;
                _executor.PlayDeath();
                return;
            }

            if (!_despawned && _executor.Mode != EnemyExecutionMode.Death)
            {
                _despawned = true;
                _actor.Despawn();
            }
        }
    }
}
