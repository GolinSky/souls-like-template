using UnityEngine;

namespace MultiPlayerTemplate.Services.Layer
{
    public interface ILayerService
    {
        LayerMask GetLayerMask(LayerName name);
        int GetLayer(LayerName name);
        void SetLayer(GameObject gameObject, LayerName name, bool recursive = true);
    }
}
