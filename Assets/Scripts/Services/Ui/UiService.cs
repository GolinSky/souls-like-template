using System.Collections.Generic;
using MultiPlayerTemplate.Ui.Base;
using UnityEngine;
using VContainer;

namespace MultiPlayerTemplate.Services
{
    public interface IUiService
    {
        TUI CreateUi<TUI>(Transform uiParent = null)
            where TUI : IBaseUi;
    }

    public class UiService: MonoBehaviour, IUiService
    {
        [SerializeField] private Transform parent;
        [SerializeField] private Canvas overlayCanvas;
        
        private readonly List<BaseUi> _overlayUis = new();

        private UiFactory UIFactory { get; set; }

        [Inject]
        private void Construct(UiFactory uiFactory)
        {
            UIFactory = uiFactory;
        }
        

        public TUI CreateUi<TUI>(Transform uiParent = null)
            where TUI : IBaseUi
        {
            TUI uiInstance = UIFactory.CreateUi<TUI>(uiParent ?? this.parent);
            return uiInstance;
        }
        

        public void MarkUiAsOverlay(BaseUi baseUi)
        {
            baseUi.Transform.SetParent(overlayCanvas.transform, false);

            if (!_overlayUis.Contains(baseUi))
            {
                _overlayUis.Add(baseUi);
            }
        }
    }
}