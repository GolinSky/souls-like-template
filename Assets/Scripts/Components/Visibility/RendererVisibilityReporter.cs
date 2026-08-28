using UnityEngine;

namespace SoulsLike.Components.Visibility
{
    public sealed class RendererVisibilityReporter : MonoBehaviour
    {
        private VisibilityComponent _visibilityComponent;
        private Renderer[] _renderers;

        public void Initialize(VisibilityComponent visibilityComponent)
        {
            _visibilityComponent = visibilityComponent;
            _renderers = GetComponents<Renderer>();
            NotifyVisibilityChanged();
        }

        private void OnBecameVisible()
        {
            NotifyVisibilityChanged();
        }

        private void OnBecameInvisible()
        {
            NotifyVisibilityChanged();
        }

        private void OnDisable()
        {
            _visibilityComponent.NotifyRendererVisibilityChanged(this, false);
        }

        private void NotifyVisibilityChanged()
        {
            foreach (Renderer renderer in _renderers)
            {
                if (renderer.isVisible)
                {
                    _visibilityComponent.NotifyRendererVisibilityChanged(this, true);
                    return;
                }
            }

            _visibilityComponent.NotifyRendererVisibilityChanged(this, false);
        }
    }
}
