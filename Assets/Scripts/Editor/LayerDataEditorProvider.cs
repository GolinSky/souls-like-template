#if UNITY_EDITOR
using System;
using SoulsLike.Services.Layer;
using SoulsLike.Services.Layer.Data;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor
{
    public static class LayerDataEditorProvider
    {
        public const string LAYER_DATA_PATH = "Assets/Settings/Data/LayerData.asset";

        public static LayerData LoadLayerData()
        {
            var data = AssetDatabase.LoadAssetAtPath<LayerData>(LAYER_DATA_PATH);
            if (data == null)
            {
                throw new InvalidOperationException(
                    $"[LayerDataEditorProvider] Canonical LayerData asset not found at '{LAYER_DATA_PATH}'.");
            }

            return data;
        }

        public static LayerMask GetMask(LayerMaskName maskName)
        {
            var data = LoadLayerData();
            if (!data.TryGetMask(maskName, out LayerMask mask))
            {
                throw new InvalidOperationException(
                    $"[LayerDataEditorProvider] Shared mask '{maskName}' is not configured in '{LAYER_DATA_PATH}'.");
            }

            if (mask.value == 0)
            {
                throw new InvalidOperationException(
                    $"[LayerDataEditorProvider] Shared mask '{maskName}' in '{LAYER_DATA_PATH}' is zero (empty).");
            }

            return mask;
        }

        public static LayerMask GetLayerMask(LayerName layerName)
        {
            var data = LoadLayerData();
            if (!data.TryGetLayerMask(layerName, out LayerMask mask))
            {
                throw new InvalidOperationException(
                    $"[LayerDataEditorProvider] Single layer '{layerName}' is not configured in '{LAYER_DATA_PATH}'.");
            }

            uint bits = unchecked((uint)mask.value);
            if (bits == 0 || (bits & (bits - 1)) != 0)
            {
                throw new InvalidOperationException(
                    $"[LayerDataEditorProvider] Single layer '{layerName}' in '{LAYER_DATA_PATH}' is not one-hot: 0x{bits:X8}.");
            }

            return mask;
        }

        public static int GetLayer(LayerName layerName)
        {
            LayerMask mask = GetLayerMask(layerName);
            uint bits = unchecked((uint)mask.value);

            for (int i = 0; i < 32; i++)
            {
                if ((bits & (1u << i)) != 0)
                {
                    return i;
                }
            }

            throw new InvalidOperationException(
                $"[LayerDataEditorProvider] Failed to extract layer index for '{layerName}'.");
        }
    }
}
#endif
