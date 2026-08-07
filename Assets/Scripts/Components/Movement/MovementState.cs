namespace SoulsLike.Entities.Character.Components.Movement
{
    [System.Serializable]
    public enum MovementState
    {
        Normal = 0,
        Climbing = 1,
        Ziplining = 2,
        LedgeGrabbing = 3,
        Rolling = 4,
        Sliding = 5
    }
}
