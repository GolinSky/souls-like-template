using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Components.Visibility
{
    public sealed class VisibilityComponent : MonoBehaviour, IInitializable, IEntityComponent, IDisposable
    {
        private readonly List<IVisibilityObserver> _observers = new();
        private readonly HashSet<RendererVisibilityReporter> _visibleRenderers = new();
        private Entity _entity;

        public bool IsVisible => _visibleRenderers.Count > 0;

        [Inject]
        public void Construct(Entity entity)
        {
            _entity = entity;
        }

        public void Initialize()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            var rendererObjects = new HashSet<GameObject>();

            foreach (Renderer renderer in renderers)
            {
                if (!rendererObjects.Add(renderer.gameObject))
                {
                    continue;
                }

                RendererVisibilityReporter reporter =
                    renderer.gameObject.GetComponent<RendererVisibilityReporter>();
                if (reporter == null)
                {
                    reporter = renderer.gameObject.AddComponent<RendererVisibilityReporter>();
                }

                reporter.Initialize(this);
            }

            _entity?.RegisterComponent(this);
        }

        public void Dispose()
        {
            _entity?.UnRegisterComponent(this);
        }

        public void RegisterObserver(IVisibilityObserver observer)
        {
            if (_observers.Contains(observer))
            {
                return;
            }

            _observers.Add(observer);
            observer.OnVisibilityChanged(IsVisible);
        }

        public void UnregisterObserver(IVisibilityObserver observer)
        {
            _observers.Remove(observer);
        }

        internal void NotifyRendererVisibilityChanged(
            RendererVisibilityReporter reporter,
            bool isVisible)
        {
            bool wasVisible = IsVisible;

            if (isVisible)
            {
                _visibleRenderers.Add(reporter);
            }
            else
            {
                _visibleRenderers.Remove(reporter);
            }

            if (wasVisible == IsVisible)
            {
                return;
            }

            foreach (IVisibilityObserver observer in _observers.ToArray())
            {
                observer.OnVisibilityChanged(IsVisible);
            }
        }
    }
}
