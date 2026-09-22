using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Character.Components
{
    public interface IComponent
    {
    }

    /// <summary>Marks a Unity component that belongs to the character aggregate.</summary>
    public class BaseComponent : MonoBehaviour, IComponent
    {
    }

    /// <summary>Receives the aggregate-owned model through VContainer injection.</summary>
    public class BaseComponent<TModel> : BaseComponent
    {
        /// <summary>VContainer assigns the model; the setter remains public for existing isolated component fixtures.</summary>
        [Inject]
        public TModel Model { get; set; }
    }
}
