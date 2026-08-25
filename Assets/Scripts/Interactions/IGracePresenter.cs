using System.Threading;
using Cysharp.Threading.Tasks;

namespace SoulsLike.Interactions
{
    public interface IGracePresenter
    {
        bool CanInteract();
        InteractionPrompt GetPrompt(GraceView graceView);
        InteractionPrompt GetFailurePrompt();
        UniTask InteractAsync(GraceView graceView, CancellationToken token);
    }
}
