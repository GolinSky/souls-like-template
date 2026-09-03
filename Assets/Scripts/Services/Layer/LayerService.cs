using System;
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
            if (!_layerData.TryGetLayerMask(name, out LayerMask mask))
            {
                throw new InvalidOperationException($"[LayerService] LayerMask for '{name}' is not configured in LayerData.");
            }

            uint bits = unchecked((uint)mask.value);
            if (bits == 0)
            {
                throw new InvalidOperationException($"[LayerService] LayerMask for '{name}' is zero (empty). Expected a single-layer mask.");
            }

            if ((bits & (bits - 1)) != 0)
            {
                throw new InvalidOperationException($"[LayerService] LayerMask for '{name}' has multiple bits set (0x{bits:X8}). Expected exactly one bit.");
            }

            return mask;
        }

        public int GetLayer(LayerName name)
        {
            LayerMask mask = GetLayerMask(name);
            uint bits = unchecked((uint)mask.value);

            for (int i = 0; i < 32; i++)
            {
                if ((bits & (1u << i)) != 0)
                {
                    return i;
                }
            }

            throw new InvalidOperationException($"[LayerService] Failed to determine layer index for '{name}'.");
        }

        public LayerMask GetMask(LayerMaskName name)
        {
            if (!_layerData.TryGetMask(name, out LayerMask mask))
            {
                throw new InvalidOperationException($"[LayerService] Shared mask for '{name}' is not configured in LayerData.");
            }

            if (mask.value == 0)
            {
                throw new InvalidOperationException($"[LayerService] Shared mask for '{name}' is zero (empty). Expected a non-zero mask.");
            }

            return mask;
        }

        public void SetLayer(GameObject gameObject, LayerName name, bool recursive = true)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

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
