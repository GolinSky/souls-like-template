using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "AnimationProfile", menuName = "Data/Items/Animation Profile")]
    public sealed class AnimationProfile : ScriptableObject
    {
        [field: SerializeField] public RuntimeAnimatorController Controller { get; private set; }

        public RuntimeAnimatorController GetController(bool hasRightWeapon)
        {
            if (!hasRightWeapon)
            {
                throw new System.InvalidOperationException(
                    $"{nameof(AnimationProfile)} requires an equipped right-hand weapon.");
            }

            if (Controller == null)
            {
                throw new System.InvalidOperationException(
                    $"Animation profile '{name}' is missing its right-hand controller.");
            }

            return Controller;
        }
    }
}
