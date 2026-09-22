using System.Numerics;

namespace SoulsLike.Services.PlayerSession
{
    /// <summary>Captures local physical input after gesture recognition and before action priority.</summary>
    public readonly struct PlayerSessionInput
    {
        public Vector2 MoveInput { get; }
        public float CameraYaw { get; }
        public bool SprintHeld { get; }
        public bool RollRequested { get; }
        public bool CrouchHeld { get; }
        public bool GuardPressed { get; }
        public bool GuardHeld { get; }
        public bool AttackPressed { get; }
        public bool AttackHeld { get; }
        public bool StrongAttackPressed { get; }
        public bool StrongAttackHeld { get; }
        public bool SpecialAttackPressed { get; }
        public bool SwitchWeaponPressed { get; }
        public bool SwitchShieldPressed { get; }
        public bool SwitchFlaskPressed { get; }
        public bool UseItemPressed { get; }
        public bool TwoHandedPressed { get; }
        public bool JumpPressed { get; }

        public PlayerSessionInput(
            Vector2 moveInput, float cameraYaw, bool sprintHeld, bool rollRequested,
            bool crouchHeld, bool guardPressed, bool guardHeld, bool attackPressed,
            bool attackHeld, bool strongAttackPressed, bool strongAttackHeld,
            bool specialAttackPressed, bool switchWeaponPressed, bool switchShieldPressed,
            bool switchFlaskPressed, bool useItemPressed, bool twoHandedPressed, bool jumpPressed)
        {
            MoveInput = moveInput;
            CameraYaw = cameraYaw;
            SprintHeld = sprintHeld;
            RollRequested = rollRequested;
            CrouchHeld = crouchHeld;
            GuardPressed = guardPressed;
            GuardHeld = guardHeld;
            AttackPressed = attackPressed;
            AttackHeld = attackHeld;
            StrongAttackPressed = strongAttackPressed;
            StrongAttackHeld = strongAttackHeld;
            SpecialAttackPressed = specialAttackPressed;
            SwitchWeaponPressed = switchWeaponPressed;
            SwitchShieldPressed = switchShieldPressed;
            SwitchFlaskPressed = switchFlaskPressed;
            UseItemPressed = useItemPressed;
            TwoHandedPressed = twoHandedPressed;
            JumpPressed = jumpPressed;
        }
    }
}
