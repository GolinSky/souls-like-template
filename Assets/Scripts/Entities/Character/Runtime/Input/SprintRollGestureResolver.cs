namespace SoulsLike.Entities.Character.Input
{
    public sealed class SprintRollGestureResolver
    {
        private const float HOLD_THRESHOLD = 0.3f;

        private float _holdTime;
        private bool _qualified;
        private bool _rollRequestedOnRelease;

        public bool IsSprinting => _qualified;

        public void Update(
            bool pressedThisFrame,
            bool isPressed,
            bool releasedThisFrame,
            float deltaTime)
        {
            _rollRequestedOnRelease = false;
            if (pressedThisFrame)
            {
                _holdTime = 0f;
                _qualified = false;
            }

            if (isPressed)
            {
                _holdTime += deltaTime;
                if (_holdTime >= HOLD_THRESHOLD) _qualified = true;
            }

            if (releasedThisFrame)
            {
                _rollRequestedOnRelease = !_qualified;
                _holdTime = 0f;
                _qualified = false;
            }
        }

        public bool ShouldRoll(bool rollReleasedThisFrame) =>
            rollReleasedThisFrame && _rollRequestedOnRelease;
    }
}
