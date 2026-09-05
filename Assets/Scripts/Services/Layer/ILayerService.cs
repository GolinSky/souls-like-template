using UnityEngine;

namespace SoulsLike.Services.Layer
{
    public interface ILayerService
    {
        LayerMask GetLayerMask(LayerName name);
        int GetLayer(LayerName name);
        LayerMask GetMask(LayerMaskName name);
        void SetLayer(GameObject gameObject, LayerName name, bool recursive = true);
    }
}
