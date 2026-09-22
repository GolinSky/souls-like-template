using SoulsLike.Services.Layer.Data;
using UnityEngine;

namespace SoulsLike.Services.Layer
{
    public class LayerService : ILayerService
    {
        private readonly LayerData _layerData;

        public LayerService(LayerData layerData)
        {
            _layerData = layerData;
        }

        public LayerMask GetLayerMask(LayerName name)
        {
            return _layerData.SingleLayers[name];
        }

        public int GetLayer(LayerName name)
        {
            LayerMask mask = GetLayerMask(name);
            uint bits = unchecked((uint)mask.value);

            int layer = 0;
            while ((bits >>= 1) != 0)
            {
                layer++;
            }

            return layer;
        }

        public LayerMask GetMask(LayerMaskName name)
        {
            return _layerData.SharedMasks[name];
        }

        public void SetLayer(GameObject gameObject, LayerName name, bool recursive = true)
        {
            int layer = GetLayer(name);
            if (recursive)
            {
                SetLayerRecursive(gameObject, layer);
            }
            else
            {
                gameObject.layer = layer;
            }
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            Transform transform = go.transform;
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SetLayerRecursive(transform.GetChild(i).gameObject, layer);
            }
        }
    }
}
