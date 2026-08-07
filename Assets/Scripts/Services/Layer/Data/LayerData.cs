using System.Collections.Generic;
using UnityEngine;

namespace MultiPlayerTemplate.Services.Layer.Data
{
    [CreateAssetMenu(fileName = "LayerData", menuName = "Data/LayerData")]
    public class LayerData : Model.Data
    {
        [SerializeField] private SerializedDictionary<LayerName, LayerMask> layers;

        public Dictionary<LayerName, LayerMask> Layers => layers.Dictionary;
    }
}
