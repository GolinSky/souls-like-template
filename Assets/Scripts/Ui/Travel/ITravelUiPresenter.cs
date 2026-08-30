using SoulsLike.Services.Scenes.Data;
using SoulsLike.Services.Travel.Data;

namespace SoulsLike.Ui.Travel
{
    public interface ITravelUiPresenter
    {
        void OnLocationSelection(SceneType locationId);
        void OnGraceSelection(GraceId graceId);
    }
}
