using UnityEngine;

namespace SoulsLike.Items
{
    [CreateAssetMenu(fileName = "AnimationProfile", menuName = "Data/Items/Animation Profile")]
    public sealed class AnimationProfile : ScriptableObject
    {
        [field: SerializeField] public RuntimeAnimatorController Controller { get; private set; }
    }
}
