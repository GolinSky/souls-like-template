using SoulsLike.Entities.Character.Components.Equipment;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor
{
    [CustomEditor(typeof(WeaponRuntime))]
    [CanEditMultipleObjects]
    public sealed class SupportHandGripEditor : UnityEditor.Editor
    {
        private const float GRIP_RADIUS = 0.04f;
        private const float AXIS_LENGTH = 0.1f;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();

            foreach (WeaponRuntime weaponRuntime in targets)
            {
                DrawAuthoringValidation(weaponRuntime);
            }
        }

        private void OnSceneGUI()
        {
            var weaponRuntime = (WeaponRuntime)target;
            SupportHandGrip supportHandGrip = weaponRuntime.SupportHandGrip;
            if (!supportHandGrip.IsEnabled
                || !HasFinitePose(supportHandGrip)
                || !HasPositiveUniformScaleChain(weaponRuntime.transform))
            {
                return;
            }

            Pose localPose = supportHandGrip.LocalPose;
            Vector3 worldPosition = weaponRuntime.transform.TransformPoint(localPose.position);
            Quaternion worldRotation = weaponRuntime.transform.rotation * localPose.rotation;

            Color previousColor = Handles.color;
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(worldPosition, worldRotation * Vector3.up, GRIP_RADIUS);
            DrawAxis(worldPosition, worldRotation * Vector3.right, Color.red);
            DrawAxis(worldPosition, worldRotation * Vector3.up, Color.green);
            DrawAxis(worldPosition, worldRotation * Vector3.forward, Color.blue);
            Handles.color = previousColor;
        }

        private static void DrawAuthoringValidation(WeaponRuntime weaponRuntime)
        {
            SupportHandGrip supportHandGrip = weaponRuntime.SupportHandGrip;
            if (!supportHandGrip.IsEnabled)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                "Support-hand grip metadata is authored in weapon-local coordinates. It does not enable runtime rigging.",
                MessageType.Info);

            if (!HasFinitePose(supportHandGrip))
            {
                EditorGUILayout.HelpBox(
                    "Support-hand grip requires finite local position and rotation values before it can be previewed.",
                    MessageType.Error);
            }

            if (!HasPositiveUniformScaleChain(weaponRuntime.transform))
            {
                EditorGUILayout.HelpBox(
                    "Support-hand grip preview requires every transform from this weapon to the root to use a positive, uniform local scale.",
                    MessageType.Warning);
            }
        }

        private static void DrawAxis(Vector3 position, Vector3 direction, Color color)
        {
            Handles.color = color;
            Handles.DrawLine(position, position + direction * AXIS_LENGTH);
        }

        private static bool HasFinitePose(SupportHandGrip supportHandGrip)
        {
            Vector3 localPosition = supportHandGrip.LocalPosition;
            Vector3 localEulerAngles = supportHandGrip.LocalEulerAngles;
            return IsFinite(localPosition.x)
                && IsFinite(localPosition.y)
                && IsFinite(localPosition.z)
                && IsFinite(localEulerAngles.x)
                && IsFinite(localEulerAngles.y)
                && IsFinite(localEulerAngles.z);
        }

        private static bool HasPositiveUniformScaleChain(Transform transform)
        {
            for (Transform current = transform; current != null; current = current.parent)
            {
                Vector3 scale = current.localScale;
                if (scale.x <= 0f
                    || scale.y <= 0f
                    || scale.z <= 0f
                    || !Mathf.Approximately(scale.x, scale.y)
                    || !Mathf.Approximately(scale.y, scale.z))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
