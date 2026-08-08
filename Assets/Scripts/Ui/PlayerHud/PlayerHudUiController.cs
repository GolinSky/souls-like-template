using System;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Services;
using SoulsLike.Services.Targeting;
using VContainer.Unity;

namespace SoulsLike.Ui.PlayerHud
{
    public class PlayerHudUiController : UiController, IInitializable, ITickable, IPlayerHudPresenter, IDisposable
    {
        private readonly ITargetingService _targetingService;
        private readonly HealthModel _healthModel;
        private PlayerHudUi _playerHudUi;
        private HealthStats _healthStats;

        public PlayerHudUiController(
            IUiService uiService,
            HealthModel healthModel,
            ITargetingService targetingService = null) : base(uiService)
        {
            _healthModel = healthModel;
            _targetingService = targetingService;
        }

        public void Initialize()
        {
            _playerHudUi = CreateUi<PlayerHudUi>();
            _playerHudUi.AssignPresenter(this);
            _healthStats = _healthModel.Stats;
            _healthModel.OnStatsChanged += OnStatsChanged;
            _playerHudUi.Show();
        }

        public void Tick()
        {
            if (_playerHudUi == null) return;

            UpdateStats();
        }

        private void OnStatsChanged(HealthStats stats)
        {
            _healthStats = stats;
            UpdateStats();
        }

        private void UpdateStats()
        {
            if (_playerHudUi == null) return;

            _playerHudUi.UpdateStats(_healthStats);
        }

        public void Dispose()
        {
            _healthModel.OnStatsChanged -= OnStatsChanged;

            if (_playerHudUi != null)
            {
                _playerHudUi.Hide();
            }
        }
    }
}
