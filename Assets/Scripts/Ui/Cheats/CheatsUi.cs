using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.Cheats
{
    public sealed class CheatsUi : BaseUi
    {
        private const int WINDOW_ID = 175613;
        private const int PLAYER_TAB_INDEX = 0;
        private const float WINDOW_WIDTH = 300f;
        private const float WINDOW_HEIGHT = 200f;
        private const float CONTENT_MARGIN = 10f;
        private const float TAB_Y = 25f;
        private const float TAB_HEIGHT = 28f;
        private const float FIRST_BUTTON_Y = 65f;
        private const float BUTTON_HEIGHT = 32f;
        private const float BUTTON_SPACING = 8f;

        private static readonly string[] _tabLabels = { "Player", "Enemies" };

        private ICheatsPresenter _presenter;
        private Rect _windowRect = new Rect(20f, 20f, WINDOW_WIDTH, WINDOW_HEIGHT);
        private int _selectedTabIndex;

        public void AssignPresenter(ICheatsPresenter presenter)
        {
            _presenter = presenter;
        }

        public override void Show()
        {
            base.Show();
            _selectedTabIndex = PLAYER_TAB_INDEX;
            _windowRect.position = new Vector2(
                (Screen.width - WINDOW_WIDTH) * 0.5f,
                (Screen.height - WINDOW_HEIGHT) * 0.5f);
        }

        private void OnGUI()
        {
            if (IsHidden)
            {
                return;
            }

            _windowRect = GUI.Window(WINDOW_ID, _windowRect, DrawWindow, "Cheats");
        }

        private void DrawWindow(int windowId)
        {
            float contentWidth = WINDOW_WIDTH - CONTENT_MARGIN * 2f;
            _selectedTabIndex = GUI.Toolbar(
                new Rect(CONTENT_MARGIN, TAB_Y, contentWidth, TAB_HEIGHT),
                _selectedTabIndex,
                _tabLabels);

            if (_selectedTabIndex == PLAYER_TAB_INDEX)
            {
                DrawPlayerActions(contentWidth);
            }
            else
            {
                DrawEnemyActions(contentWidth);
            }

            GUI.DragWindow(new Rect(0f, 0f, WINDOW_WIDTH, TAB_Y));
        }

        private void DrawPlayerActions(float contentWidth)
        {
            if (GUI.Button(CreateButtonRect(contentWidth, 0), "Hit Player"))
            {
                _presenter.HitPlayer();
            }

            if (GUI.Button(CreateButtonRect(contentWidth, 1), "Kill Player"))
            {
                _presenter.KillPlayer();
            }
        }

        private void DrawEnemyActions(float contentWidth)
        {
            if (GUI.Button(CreateButtonRect(contentWidth, 0), "Hit All Enemies"))
            {
                _presenter.HitAllEnemies();
            }

            if (GUI.Button(CreateButtonRect(contentWidth, 1), "Kill All Enemies"))
            {
                _presenter.KillAllEnemies();
            }

            if (GUI.Button(CreateButtonRect(contentWidth, 2), "Respawn Enemies"))
            {
                _presenter.RespawnEnemies();
            }
        }

        private static Rect CreateButtonRect(float contentWidth, int index)
        {
            float y = FIRST_BUTTON_Y + index * (BUTTON_HEIGHT + BUTTON_SPACING);
            return new Rect(CONTENT_MARGIN, y, contentWidth, BUTTON_HEIGHT);
        }
    }
}
