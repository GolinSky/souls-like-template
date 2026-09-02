using SoulsLike.Entities.Combat;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    [CreateAssetMenu(fileName = "WeaponMovesetDefinition", menuName = "Enemy/Weapon Moveset")]
    public sealed class WeaponMovesetDefinition : ScriptableObject
    {
        [SerializeField] private ItemId weaponId;
        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private EnemyMove[] moves = { };
        [SerializeField] private CharacterActionDefinition deathAction;

        public ItemId WeaponId => weaponId;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public EnemyMove[] Moves => moves;
        public CharacterActionDefinition DeathAction => deathAction;
    }

}
