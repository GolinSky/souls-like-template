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
            return singleLayers.Dictionary.TryGetValue(name, out mask);
        }

        public bool TryGetMask(LayerMaskName name, out LayerMask mask)
        {
            return sharedMasks.Dictionary.TryGetValue(name, out mask);
        }

        public IReadOnlyDictionary<LayerName, LayerMask> SingleLayers => singleLayers.Dictionary;
        public IReadOnlyDictionary<LayerMaskName, LayerMask> SharedMasks => sharedMasks.Dictionary;

#if UNITY_EDITOR
        public void SetLayerMaskForTest(LayerName name, LayerMask mask)
        {
            singleLayers.Dictionary[name] = mask;
        }

        public void SetSharedMaskForTest(LayerMaskName name, LayerMask mask)
        {
            sharedMasks.Dictionary[name] = mask;
        }
#endif
    }
}
