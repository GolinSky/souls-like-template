namespace SoulsLike.Entities.Character.Input
{
    public sealed class SprintRollGestureResolver
    {
        private const float HOLD_THRESHOLD = 0.3f;

        private float _holdTime;
        private bool _qualified;
        private bool _pressedDuringRoll;
        private bool _rollRequestedOnRelease;

        public bool IsSprinting => _qualified && !_pressedDuringRoll;

        public void Update(
            bool pressedThisFrame,
            bool isPressed,
            bool releasedThisFrame,
            bool rollActive,
            float deltaTime)
        {
            _rollRequestedOnRelease = false;
            if (pressedThisFrame)
            {
                _holdTime = 0f;
                _qualified = false;
                _pressedDuringRoll = rollActive;
            }

            if (isPressed)
            {
                _holdTime += deltaTime;
                if (_holdTime >= HOLD_THRESHOLD) _qualified = true;
            }

            if (releasedThisFrame)
            {
                _rollRequestedOnRelease = !_qualified || _pressedDuringRoll;
                _holdTime = 0f;
                _qualified = false;
                _pressedDuringRoll = false;
            }
        }

        public bool ShouldRoll(bool rollReleasedThisFrame) =>
            rollReleasedThisFrame && _rollRequestedOnRelease;
    }
}
