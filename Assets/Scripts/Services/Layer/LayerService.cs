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
            if (_layerData.Layers.TryGetValue(name, out var mask))
            {
                return mask;
            }
            
            Debug.LogWarning($"[LayerService] LayerMask for {name} not found in LayerData.");
            return 0;
        }

        public int GetLayer(LayerName name)
        {
            LayerMask mask = GetLayerMask(name);
            int maskValue = mask.value;
            
            if (maskValue == 0) return 0;

            // Find the first set bit (layer index)
            for (int i = 0; i < 32; i++)
            {
                if ((maskValue & (1 << i)) != 0)
                {
                    return i;
                }
            }
            
            return 0;
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

        private void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}
