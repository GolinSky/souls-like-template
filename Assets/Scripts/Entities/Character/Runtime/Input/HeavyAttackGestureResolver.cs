namespace SoulsLike.Entities.Character.Input
{
    public sealed class HeavyAttackGestureResolver
    {
        private bool _strongInputActive;
        private bool _suppressLightUntilRelease;

        public bool StrongAttackHeld => _strongInputActive;

        public bool TryResolve(
            bool strongPressedThisFrame,
            bool strongReleasedThisFrame,
            bool lightIsPressed,
            bool canBufferAttack)
        {
            if (!lightIsPressed) _suppressLightUntilRelease = false;
            if (_strongInputActive && strongReleasedThisFrame) _strongInputActive = false;
            if (!strongPressedThisFrame) return false;

            _strongInputActive = canBufferAttack;
            _suppressLightUntilRelease = true;
            return canBufferAttack;
        }

        public bool ShouldSuppressLightAttack(bool lightPressedThisFrame) =>
            _suppressLightUntilRelease || !lightPressedThisFrame;
    }
}
