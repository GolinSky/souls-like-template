using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;
using UnityEngine;

namespace SoulsLike.Interactions
{
    public interface IInteractable
    {
        bool CanInteract(IEntity actor);
        InteractionPrompt GetPrompt(IEntity actor);
        InteractionPrompt GetFailurePrompt(IEntity actor);
        UniTask InteractAsync(IEntity actor, CancellationToken token);
        Transform InteractionAnchor { get; }
        int Priority { get; }
    }
}
