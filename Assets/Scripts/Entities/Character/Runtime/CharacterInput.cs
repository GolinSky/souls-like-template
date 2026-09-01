using UnityEngine;

namespace SoulsLike.Entities.Character.Runtime
{
    public readonly struct CharacterInput
    {
        public Vector2 MoveInput { get; }
        public float CameraYaw { get; }
        public bool SprintHeld { get; }
        public bool CrouchHeld { get; }
        public bool GuardHeld { get; }
        public bool StrongAttackHeld { get; }
        public CharacterAction? FirstAction { get; }
        public CharacterAction? SecondAction { get; }

        public CharacterInput(Vector2 moveInput, float cameraYaw, bool sprintHeld, bool crouchHeld, bool guardHeld, bool strongAttackHeld, CharacterAction? firstAction = null, CharacterAction? secondAction = null)
        {
            MoveInput = moveInput;
            CameraYaw = cameraYaw;
            SprintHeld = sprintHeld;
            CrouchHeld = crouchHeld;
            GuardHeld = guardHeld;
            StrongAttackHeld = strongAttackHeld;
            FirstAction = firstAction;
            SecondAction = secondAction;
        }
    }
}
