using System.Threading;
using Cysharp.Threading.Tasks;
using SoulsLike.Entities.BaseEntity;

namespace SoulsLike.Entities.Elevator
{
    public interface IElevatorPresenter
    {
        void Register(ElevatorView elevator);
        void Unregister(ElevatorView elevator);
        bool CanInteract(ElevatorEndpoint endpoint, IEntity actor);
        string GetPrompt();
        string GetFailurePrompt(ElevatorEndpoint endpoint, IEntity actor);
        UniTask InteractAsync(ElevatorEndpoint endpoint, IEntity actor, CancellationToken token);
    }
}
