namespace SoulsLike.Entities.Character.Components.Movement
{
    public enum LocomotionState
    {
        Grounded = 0,
        JumpStart = 1,
        Airborne = 2,
        Landing = 3,
        HardLanding = 4
    }

    public enum LandingType
    {
        None = 0,
        Normal = 1,
        Hard = 2
    }
}
