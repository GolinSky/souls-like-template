using System;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Combat;
using SoulsLike.Services;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyBrain :
        IInitializable,
        ITickable,
        IGameStateObserver,
        IDisposable
    {
        private const float FACING_ANGLE = 20f;

        private readonly EnemyActor _actor;
        private readonly EnemyNavigationMotor _motor;
        private readonly EnemyAnimationController _animation;
        private readonly EnemyPerception _perception;
        private readonly EnemyActionSelector _actionSelector;
        private readonly HealthModel _healthModel;
        private readonly IGameStateNotifier _gameStateNotifier;

        private GameState _gameState;
        private float _nextDecisionTime;
        private float _reactionReadyTime;
        private float _waitUntil;
        private float _searchUntil;
        private long? _reactionTargetEntityId;
        private int _patrolIndex;
        private bool _waitingAtPatrolPoint;
        private bool _deathAnimationStarted;
        private bool _despawned;

        public EnemyBrain(
            EnemyActor actor,
            EnemyNavigationMotor motor,
            EnemyAnimationController animation,
            EnemyPerception perception,
            EnemyActionSelector actionSelector,
            HealthModel healthModel,
            IGameStateNotifier gameStateNotifier)
        {
            _actor = actor;
            _motor = motor;
            _animation = animation;
            _perception = perception;
            _actionSelector = actionSelector;
            _healthModel = healthModel;
            _gameStateNotifier = gameStateNotifier;
        }

        public EnemyGoal Goal { get; private set; }
        public EnemyIntent CurrentIntent { get; private set; }
        public long? TargetEntityId => _perception.Memory?.EntityId;

        public void Initialize()
        {
            _gameStateNotifier.RegisterObserver(this);
            _gameState = _gameStateNotifier.CurrentGameState;
            _healthModel.OnDamageApplied += OnDamageApplied;
            _healthModel.OnDied += OnDied;
            Goal = _actor.BehaviourProfile.StartsDormant
                ? EnemyGoal.Dormant
                : _actor.PatrolPoints.Count > 0
                    ? EnemyGoal.Patrol
                    : EnemyGoal.Idle;
            CurrentIntent = new EnemyIntent(EnemyIntentKind.Wait, _actor.transform.position);
        }

        public void Dispose()
        {
            _gameStateNotifier.UnregisterObserver(this);
            _healthModel.OnDamageApplied -= OnDamageApplied;
            _healthModel.OnDied -= OnDied;
        }

        public void OnGameStateChanged(GameState newState)
        {
            _gameState = newState;
            if (newState != GameState.Idle)
            {
                _motor.Stop();
            }
        }

        public void Activate()
        {
            if (Goal == EnemyGoal.Dormant)
            {
                EnterGoal(_actor.PatrolPoints.Count > 0
                    ? EnemyGoal.Patrol
                    : EnemyGoal.Idle);
            }
        }

        public void Tick()
        {
            if (_gameState != GameState.Idle)
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
                _motor.Stop();
                return;
            }

            float deltaTime = Time.deltaTime;
            float now = Time.time;
            _motor.Tick(deltaTime);
            _animation.SetLocomotion(_motor.LocalVelocity);

            if (_animation.IsActionRunning)
            {
                TickCommittedAction(now, deltaTime);
                return;
            }

            TickContinuousGoal(now, deltaTime);
            if (now < _nextDecisionTime)
            {
                return;
            }

            _nextDecisionTime = now + _actor.BehaviourProfile.DecisionInterval;
            Decide(now);
        }

        private void Decide(float now)
        {
            bool observed = _perception.TryObserve(
                _actor.transform.position,
                _actor.transform.forward,
                _actor.BehaviourProfile,
                now,
                out TargetingSnapshot target);

            if (observed)
            {
                if (IsOutsideLeash(target.Position))
                {
                    EnterGoal(EnemyGoal.ReturnHome);
                    return;
                }

                BeginReaction(target.EntityId, now);
                if (now < _reactionReadyTime)
                {
                    EnterGoal(EnemyGoal.Investigate);
                    _motor.Stop();
                    _motor.Face(target.Position, 360f, Time.deltaTime);
                    return;
                }

                EnterGoal(EnemyGoal.Combat);
                DecideCombat(target, now);
                return;
            }

            if (_perception.HasRecentMemory(now, _actor.BehaviourProfile))
            {
                EnterGoal(EnemyGoal.Investigate);
                MoveTo(_perception.Memory.Value.LastKnownPosition);
                return;
            }

            if (Goal is EnemyGoal.Combat or EnemyGoal.Investigate)
            {
                EnterGoal(EnemyGoal.Search);
                return;
            }

            if (IsOutsideLeash(_actor.transform.position)
                || Goal == EnemyGoal.ReturnHome)
            {
                EnterGoal(EnemyGoal.ReturnHome);
                MoveTo(_actor.HomePosition);
                return;
            }

            if (Goal is EnemyGoal.Idle or EnemyGoal.Patrol)
            {
                DecidePatrol(now);
            }
        }

        private void DecideCombat(in TargetingSnapshot target, float now)
        {
            Vector3 toTarget = target.Position - _actor.transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            float angle = Vector3.Angle(_actor.transform.forward, toTarget);
            bool hasLineOfSight = _perception.HasLineOfSight(
                _actor.transform.position,
                target,
                _actor.BehaviourProfile);

            if (!hasLineOfSight || distance > _actor.BehaviourProfile.PreferredRangeMax)
            {
                MoveTo(target.Position);
                return;
            }

            if (distance < _actor.BehaviourProfile.PreferredRangeMin)
            {
                Vector3 retreatDirection = toTarget.sqrMagnitude > 0f
                    ? -toTarget.normalized
                    : -_actor.transform.forward;
                MoveTo(
                    _actor.transform.position
                    + retreatDirection * _actor.BehaviourProfile.StrafeDistance);
                return;
            }

            _motor.Stop();
            if (angle > FACING_ANGLE)
            {
                Face(target.Position, 360f);
                return;
            }

            CharacterActionDefinition action = _actionSelector.Select(
                _actor.BehaviourProfile.ActionRules,
                distance,
                angle,
                hasLineOfSight,
                false,
                now);
            if (action != null)
            {
                CurrentIntent = new EnemyIntent(
                    EnemyIntentKind.ExecuteAction,
                    target.Position,
                    action);
                _animation.PlayAction(action);
                return;
            }

            Vector3 side = Vector3.Cross(Vector3.up, toTarget.normalized);
            if (!_actionSelector.NextBool())
            {
                side = -side;
            }

            MoveTo(
                _actor.transform.position
                + side * _actor.BehaviourProfile.StrafeDistance);
            _waitUntil = now + _actor.BehaviourProfile.WaitSeconds;
        }

        private void TickCommittedAction(float now, float deltaTime)
        {
            if (!_perception.TryResolveRememberedTarget(out TargetingSnapshot target))
            {
                _perception.Clear();
                _animation.Interrupt();
                EnterGoal(EnemyGoal.Search);
                return;
            }

            _motor.Face(target.Position, _animation.CurrentTurnSpeed, deltaTime);
            if (!_animation.ComboWindowOpen)
            {
                return;
            }

            Vector3 toTarget = target.Position - _actor.transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            float angle = Vector3.Angle(_actor.transform.forward, toTarget);
            bool lineOfSight = _perception.HasLineOfSight(
                _actor.transform.position,
                target,
                _actor.BehaviourProfile);
            CharacterActionDefinition followUp = _actionSelector.Select(
                _actor.BehaviourProfile.ActionRules,
                distance,
                angle,
                lineOfSight,
                true,
                now);
            if (followUp != null)
            {
                _animation.QueueFollowUp(followUp);
            }
        }

        private void TickContinuousGoal(float now, float deltaTime)
        {
            switch (Goal)
            {
                case EnemyGoal.Investigate:
                    if (_perception.Memory.HasValue
                        && _motor.IsWithin(
                            _perception.Memory.Value.LastKnownPosition,
                            _actor.BehaviourProfile.ArrivalDistance))
                    {
                        EnterGoal(EnemyGoal.Search);
                    }
                    break;
                case EnemyGoal.Search:
                    _motor.Stop();
                    _motor.Rotate(
                        _actor.BehaviourProfile.SearchTurnSpeed,
                        deltaTime);
                    if (now >= _searchUntil)
                    {
                        EnterGoal(EnemyGoal.ReturnHome);
                        MoveTo(_actor.HomePosition);
                    }
                    break;
                case EnemyGoal.ReturnHome:
                    if (_motor.IsWithin(
                        _actor.HomePosition,
                        _actor.BehaviourProfile.ArrivalDistance))
                    {
                        _motor.Stop();
                        _perception.Clear();
                        _reactionTargetEntityId = null;
                        EnterGoal(_actor.PatrolPoints.Count > 0
                            ? EnemyGoal.Patrol
                            : EnemyGoal.Idle);
                    }
                    break;
            }
        }

        private void DecidePatrol(float now)
        {
            if (_actor.PatrolPoints.Count == 0)
            {
                EnterGoal(EnemyGoal.Idle);
                WaitAt(_actor.transform.position);
                return;
            }

            EnterGoal(EnemyGoal.Patrol);
            if (!_waitingAtPatrolPoint
                && _motor.IsWithin(
                    _actor.PatrolPoints[_patrolIndex],
                    _actor.BehaviourProfile.ArrivalDistance))
            {
                _waitingAtPatrolPoint = true;
                _waitUntil = now + _actor.BehaviourProfile.PatrolWaitSeconds;
            }

            if (_waitingAtPatrolPoint && now < _waitUntil)
            {
                WaitAt(_actor.PatrolPoints[_patrolIndex]);
                return;
            }

            if (_waitingAtPatrolPoint)
            {
                _waitingAtPatrolPoint = false;
                _patrolIndex = (_patrolIndex + 1) % _actor.PatrolPoints.Count;
            }

            MoveTo(_actor.PatrolPoints[_patrolIndex]);
        }

        private void BeginReaction(long targetEntityId, float now)
        {
            if (_reactionTargetEntityId == targetEntityId)
            {
                return;
            }

            _reactionTargetEntityId = targetEntityId;
            _reactionReadyTime = now + _actionSelector.Range(
                _actor.BehaviourProfile.ReactionDelayMin,
                _actor.BehaviourProfile.ReactionDelayMax);
        }

        private void EnterGoal(EnemyGoal goal)
        {
            if (Goal == goal)
            {
                return;
            }

            Goal = goal;
            if (goal == EnemyGoal.Search)
            {
                _searchUntil = Time.time + _actor.BehaviourProfile.SearchSeconds;
            }
        }

        private bool IsOutsideLeash(Vector3 position) =>
            (position - _actor.HomePosition).sqrMagnitude
            > _actor.BehaviourProfile.LeashDistance
            * _actor.BehaviourProfile.LeashDistance;

        private void MoveTo(Vector3 position)
        {
            CurrentIntent = new EnemyIntent(EnemyIntentKind.Move, position);
            _motor.SetDestination(position);
        }

        private void Face(Vector3 position, float turnSpeed)
        {
            CurrentIntent = new EnemyIntent(EnemyIntentKind.Face, position);
            _motor.Face(position, turnSpeed, Time.deltaTime);
        }

        private void WaitAt(Vector3 position)
        {
            CurrentIntent = new EnemyIntent(EnemyIntentKind.Wait, position);
            _motor.Stop();
        }

        private void OnDamageApplied(DamageResult damage)
        {
            _perception.RegisterDamageStimulus(damage.SourceEntityId, Time.time);
            BeginReaction(damage.SourceEntityId, Time.time);
            if (Goal == EnemyGoal.Dormant)
            {
                EnterGoal(EnemyGoal.Investigate);
            }
        }

        private void OnDied(long sourceEntityId)
        {
            EnterGoal(EnemyGoal.Dead);
            _motor.Stop();
            _animation.Interrupt();
        }

        private void TickDeath()
        {
            if (!_deathAnimationStarted)
            {
                _deathAnimationStarted = true;
                _animation.PlayDeath();
                return;
            }

            if (!_despawned && !_animation.IsActionRunning)
            {
                _despawned = true;
                _actor.Despawn();
            }
        }
    }
}
