using System;
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
        [SerializeField] private CharacterActionDefinition[] actions = { };

        public ItemId WeaponId => weaponId;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public CharacterActionDefinition[] Actions => actions;

        public CharacterActionDefinition GetAction(CharacterActionId actionId)
        {
            foreach (CharacterActionDefinition action in actions)
            {
                if (action.ActionId == actionId)
                {
                    return action;
                }
            }

            throw new InvalidOperationException(
                $"Moveset '{name}' does not contain action '{actionId}'.");
        }
    }

}
