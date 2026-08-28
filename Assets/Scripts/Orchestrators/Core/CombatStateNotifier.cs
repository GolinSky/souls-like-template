using System.Collections.Generic;

namespace SoulsLike.Services
{
    public sealed class CombatStateNotifier : ICombatStateNotifier
    {
        private readonly HashSet<long> _aggroEnemyIds = new();
        private readonly List<ICombatStateObserver> _observers = new();

        public CombatState CurrentCombatState { get; private set; } =
            CombatState.NoCombat;

        public void RegisterObserver(ICombatStateObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void UnregisterObserver(ICombatStateObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }

        public void ReportEnemyAggroStarted(long enemyEntityId)
        {
            if (!_aggroEnemyIds.Add(enemyEntityId))
            {
                return;
            }

            SetCombatState(CombatState.Combat);
        }

        public void ReportEnemyAggroEnded(long enemyEntityId)
        {
            if (!_aggroEnemyIds.Remove(enemyEntityId))
            {
                return;
            }

            if (_aggroEnemyIds.Count == 0)
            {
                SetCombatState(CombatState.NoCombat);
            }
        }

        private void SetCombatState(CombatState newState)
        {
            if (CurrentCombatState == newState)
            {
                return;
            }

            CurrentCombatState = newState;
            foreach (ICombatStateObserver observer in _observers.ToArray())
            {
                observer.OnCombatStateChanged(CurrentCombatState);
            }
        }
    }
}
