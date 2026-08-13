using SoulsLike.Entities.Character;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.EditorTools
{
    public static class PlayerAnimatorTool
    {
        [MenuItem("Tools/SoulsLike/Focus Current Player Animator &a", false, 20)]
        private static void FocusCurrentPlayerAnimator()
        {
            Animator animator = FindAnimator();
            if (animator == null)
            {
                Debug.LogWarning("[PlayerAnimatorTool] No Animator was found on the selected object or current Character.");
                return;
            }

            Selection.activeGameObject = animator.gameObject;
            EditorGUIUtility.PingObject(animator.gameObject);
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");

            Debug.Log($"[PlayerAnimatorTool] Focused '{animator.name}'. {GetCurrentStates(animator)}");
        }

        private static Animator FindAnimator()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject != null)
            {
                Animator selectedAnimator = selectedObject.GetComponentInChildren<Animator>(true)
                    ?? selectedObject.GetComponentInParent<Animator>();
                if (selectedAnimator != null)
                {
                    return selectedAnimator;
                }
            }

            Character character = Object.FindFirstObjectByType<Character>(FindObjectsInactive.Include);
            return character == null ? null : character.GetComponentInChildren<Animator>(true);
        }

        private static string GetCurrentStates(Animator animator)
        {
            if (!Application.isPlaying)
            {
                return "Enter Play Mode to inspect live Animator states.";
            }

            string[] layers = new string[animator.layerCount];
            for (int layerIndex = 0; layerIndex < animator.layerCount; layerIndex++)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(layerIndex);
                layers[layerIndex] = $"{animator.GetLayerName(layerIndex)}: hash {state.fullPathHash}, time {state.normalizedTime:F2}";
            }

            return string.Join(" | ", layers);
        }
    }
}
