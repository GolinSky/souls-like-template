using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Interactions;
using UnityEngine;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public interface IInteractableCommand : IEntityComponent, IInteractable
    {
        Transform GetInteractionAnchor(IEntity actor);
    }
}
