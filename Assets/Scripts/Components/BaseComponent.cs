using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Character.Components
{
    public interface IComponent
    {
    }

    public class BaseComponent : MonoBehaviour, IComponent
    {
    }

    public class BaseComponent<TModel> : BaseComponent
    {
        [Inject]
        public TModel Model { get; set; }
    }
}
