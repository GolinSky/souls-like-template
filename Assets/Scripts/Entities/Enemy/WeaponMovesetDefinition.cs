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
        [SerializeField] private AnimationClip combatIdle;
        [SerializeField] private AnimationClip walkForward;
        [SerializeField] private AnimationClip walkBackward;
        [SerializeField] private AnimationClip walkLeft;
        [SerializeField] private AnimationClip walkRight;
        [SerializeField] private AnimationClip runForward;
        [SerializeField] private AnimationClip runBackward;
        [SerializeField] private AnimationClip runLeft;
        [SerializeField] private AnimationClip runRight;
        [SerializeField] private CharacterActionDefinition[] actions = { };

        public ItemId WeaponId => weaponId;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public AnimationClip CombatIdle => combatIdle;
        public AnimationClip WalkForward => walkForward;
        public AnimationClip WalkBackward => walkBackward;
        public AnimationClip WalkLeft => walkLeft;
        public AnimationClip WalkRight => walkRight;
        public AnimationClip RunForward => runForward;
        public AnimationClip RunBackward => runBackward;
        public AnimationClip RunLeft => runLeft;
        public AnimationClip RunRight => runRight;
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
