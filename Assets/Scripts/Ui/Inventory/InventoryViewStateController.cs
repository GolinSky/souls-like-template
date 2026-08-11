using SoulsLike.Extensions;
using UnityEngine;

namespace SoulsLike.Ui.Inventory
{
    public enum InventoryViewState
    {
        DualPanel = 0,   // State 1: Grid + Item Specs + Character Stats
        LoreView = 1,    // State 2: Grid + Full Lore Text Card (Stats hidden)
        SimpleView = 2   // State 3: Grid only (Details & Stats hidden for model inspection)
    }

    public class InventoryViewStateController : MonoBehaviour
    {
        [Header("Column References")]
        [SerializeField] private CanvasGroup gridColumnGroup;      // Column 1 (~30%)
        [SerializeField] private CanvasGroup detailsColumnGroup;   // Column 2 (~40%) - Standard Details Card
        [SerializeField] private CanvasGroup loreCardGroup;        // Column 2 - Full Lore Text Card
        [SerializeField] private CanvasGroup statsColumnGroup;     // Column 3 (~30%) - Character Stats Sheet

        public InventoryViewState CurrentState { get; private set; } = InventoryViewState.DualPanel;

        public void SetState(InventoryViewState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case InventoryViewState.DualPanel:
                    gridColumnGroup?.SetActive(true);
                    detailsColumnGroup?.SetActive(true);
                    loreCardGroup?.SetActive(false);
                    statsColumnGroup?.SetActive(true);
                    break;

                case InventoryViewState.LoreView:
                    gridColumnGroup?.SetActive(true);
                    detailsColumnGroup?.SetActive(false);
                    loreCardGroup?.SetActive(true);
                    statsColumnGroup?.SetActive(false);
                    break;

                case InventoryViewState.SimpleView:
                    gridColumnGroup?.SetActive(true);
                    detailsColumnGroup?.SetActive(false);
                    loreCardGroup?.SetActive(false);
                    statsColumnGroup?.SetActive(false);
                    break;
            }
        }

        public void CycleState()
        {
            int next = ((int)CurrentState + 1) % 3;
            SetState((InventoryViewState)next);
        }

        public void ToggleLoreView()
        {
            if (CurrentState == InventoryViewState.LoreView)
                SetState(InventoryViewState.DualPanel);
            else
                SetState(InventoryViewState.LoreView);
        }

        public void ToggleSimpleView()
        {
            if (CurrentState == InventoryViewState.SimpleView)
                SetState(InventoryViewState.DualPanel);
            else
                SetState(InventoryViewState.SimpleView);
        }
    }
}
