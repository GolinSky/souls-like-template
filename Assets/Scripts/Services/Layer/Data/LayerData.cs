using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Services.Layer.Data
{
    [CreateAssetMenu(fileName = "LayerData", menuName = "Data/LayerData")]
    public class LayerData : Model.Data
    {
        [SerializeField]
        private SerializedDictionary<LayerName, LayerMask> singleLayers = new();

        [SerializeField]
        private SerializedDictionary<LayerMaskName, LayerMask> sharedMasks = new();

        public bool TryGetLayerMask(LayerName name, out LayerMask mask)
        {
            if (singleLayers?.Dictionary != null && singleLayers.Dictionary.TryGetValue(name, out mask))
            {
                return true;
            }

            mask = default;
            return false;
        }

        public bool TryGetMask(LayerMaskName name, out LayerMask mask)
        {
            if (sharedMasks?.Dictionary != null && sharedMasks.Dictionary.TryGetValue(name, out mask))
            {
                return true;
            }

            mask = default;
            return false;
        }

        public IReadOnlyDictionary<LayerName, LayerMask> SingleLayers => singleLayers?.Dictionary;
        public IReadOnlyDictionary<LayerMaskName, LayerMask> SharedMasks => sharedMasks?.Dictionary;

#if UNITY_EDITOR
        public void SetLayerMaskForTest(LayerName name, LayerMask mask)
        {
            singleLayers.Dictionary[name] = mask;
        }

        public void SetSharedMaskForTest(LayerMaskName name, LayerMask mask)
        {
            sharedMasks.Dictionary[name] = mask;
        }

        private void OnValidate()
        {
            ValidateConfiguration();
        }

        public void ValidateConfiguration()
        {
            if (singleLayers?.Dictionary != null)
            {
                foreach (LayerName layerName in Enum.GetValues(typeof(LayerName)))
                {
                    if (!singleLayers.Dictionary.TryGetValue(layerName, out LayerMask mask))
                    {
                        Debug.LogError($"[LayerData] Missing single-layer entry for '{layerName}'.", this);
                        continue;
                    }

                    uint bits = unchecked((uint)mask.value);
                    if (bits == 0)
                    {
                        Debug.LogError($"[LayerData] Single-layer entry '{layerName}' has mask 0.", this);
                    }
                    else if ((bits & (bits - 1)) != 0)
                    {
                        Debug.LogError($"[LayerData] Single-layer entry '{layerName}' has multiple bits set (0x{bits:X8}). Expected exactly one bit.", this);
                    }
                }
            }

            if (sharedMasks?.Dictionary != null)
            {
                foreach (LayerMaskName maskName in Enum.GetValues(typeof(LayerMaskName)))
                {
                    if (!sharedMasks.Dictionary.TryGetValue(maskName, out LayerMask mask))
                    {
                        Debug.LogError($"[LayerData] Missing shared mask entry for '{maskName}'.", this);
                        continue;
                    }

                    if (mask.value == 0)
                    {
                        Debug.LogError($"[LayerData] Shared mask entry '{maskName}' is zero.", this);
                    }
                }
            }
        }
#endif
    }
}
