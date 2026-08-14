using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "AnimationProfile", menuName = "Data/Items/Animation Profile")]
    public sealed class AnimationProfile : ScriptableObject
    {
        [field: SerializeField] public RuntimeAnimatorController Controller { get; private set; }
        [field: SerializeField] public RuntimeAnimatorController LeftHandController { get; private set; }
        [field: SerializeField] public RuntimeAnimatorController DualWieldController { get; private set; }

        public RuntimeAnimatorController GetController(bool hasRightWeapon, bool hasLeftWeapon)
        {
            RuntimeAnimatorController controller = (hasRightWeapon, hasLeftWeapon) switch
            {
                (true, true) => DualWieldController,
                (true, false) => Controller,
                (false, true) => LeftHandController,
                _ => throw new System.InvalidOperationException(
                    $"{nameof(AnimationProfile)} requires at least one equipped weapon.")
            };

            if (controller == null)
            {
                throw new System.InvalidOperationException(
                    $"Animation profile '{name}' is missing the controller required for the current weapon loadout.");
            }

            return controller;
        }
    }
}
