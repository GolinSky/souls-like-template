using System.Collections.Generic;
using SoulsLike.Entities.Enemy;
using SoulsLike.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.Cheats
{
    public sealed class CheatsUi : BaseUi
    {
        private const int WINDOW_ID = 175613;
        private const int PLAYER_TAB_INDEX = 0;
        private const float WINDOW_WIDTH = 300f;
        private const float WINDOW_HEIGHT = 280f;
        private const float CONTENT_MARGIN = 10f;
        private const float TAB_Y = 25f;
        private const float TAB_HEIGHT = 28f;
        private const float FIRST_BUTTON_Y = 65f;
        private const float BUTTON_HEIGHT = 32f;
        private const float BUTTON_SPACING = 8f;
        private const float ARROW_BUTTON_WIDTH = 32f;
        private const float SELECTOR_SPACING = 4f;

        private static readonly string[] _tabLabels = { "Player", "Enemies" };

        private ICheatsPresenter _presenter;
        private Rect _windowRect = new Rect(20f, 20f, WINDOW_WIDTH, WINDOW_HEIGHT);
        private int _selectedTabIndex;
        private int _selectedEnemyIdIndex;

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

            if (GUI.Button(CreateButtonRect(contentWidth, 2), "Reset Open Graces"))
            {
                _presenter.ResetOpenGraces();
            }

            string invincibilityLabel = _presenter.IsPlayerInvincible
                ? "Invincible: On"
                : "Invincible: Off";
            if (GUI.Button(CreateButtonRect(contentWidth, 3), invincibilityLabel))
            {
                _presenter.TogglePlayerInvincibility();
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

            IReadOnlyList<EnemyId> availableIds = _presenter.AvailableEnemyIds;
            if (availableIds == null || availableIds.Count == 0)
            {
                return;
            }

            if (_selectedEnemyIdIndex < 0 || _selectedEnemyIdIndex >= availableIds.Count)
            {
                _selectedEnemyIdIndex = 0;
            }

            Rect selectorRect = CreateButtonRect(contentWidth, 3);
            float centerWidth = contentWidth - (ARROW_BUTTON_WIDTH * 2f + SELECTOR_SPACING * 2f);
            Rect leftArrowRect = new Rect(CONTENT_MARGIN, selectorRect.y, ARROW_BUTTON_WIDTH, BUTTON_HEIGHT);
            Rect centerRect = new Rect(CONTENT_MARGIN + ARROW_BUTTON_WIDTH + SELECTOR_SPACING, selectorRect.y, centerWidth, BUTTON_HEIGHT);
            Rect rightArrowRect = new Rect(CONTENT_MARGIN + ARROW_BUTTON_WIDTH + SELECTOR_SPACING + centerWidth + SELECTOR_SPACING, selectorRect.y, ARROW_BUTTON_WIDTH, BUTTON_HEIGHT);

            if (GUI.Button(leftArrowRect, "<"))
            {
                _selectedEnemyIdIndex = (_selectedEnemyIdIndex - 1 + availableIds.Count) % availableIds.Count;
            }

            string currentEnemyLabel = availableIds[_selectedEnemyIdIndex].ToString();
            if (GUI.Button(centerRect, currentEnemyLabel))
            {
                _selectedEnemyIdIndex = (_selectedEnemyIdIndex + 1) % availableIds.Count;
            }

            if (GUI.Button(rightArrowRect, ">"))
            {
                _selectedEnemyIdIndex = (_selectedEnemyIdIndex + 1) % availableIds.Count;
            }

            if (GUI.Button(CreateButtonRect(contentWidth, 4), "Spawn Enemy In Front"))
            {
                _presenter.SpawnEnemy(availableIds[_selectedEnemyIdIndex]);
            }
        }

        private static Rect CreateButtonRect(float contentWidth, int index)
        {
            float y = FIRST_BUTTON_Y + index * (BUTTON_HEIGHT + BUTTON_SPACING);
            return new Rect(CONTENT_MARGIN, y, contentWidth, BUTTON_HEIGHT);
        }
    }
}
