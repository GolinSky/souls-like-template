using Prospector.Utility.Timer;
using UnityEngine;

namespace SoulsLike.Entities.Character
{
    public enum CharacterActionType
    {
        None = 0,
        LightAttack = 1,
        HeavyAttack = 2,
        SpecialAttack = 4,
        Roll = 5
    }

    public readonly struct BufferedCharacterAction
    {
        public CharacterActionType Type { get; }
        public Vector2 MoveInput { get; }
        public float CameraYaw { get; }
        public bool IsSprinting { get; }
        public bool IsLeftHandAttack { get; }

        private BufferedCharacterAction(
            CharacterActionType type,
            Vector2 moveInput,
            float cameraYaw,
            bool isSprinting,
            bool isLeftHandAttack)
        {
            Type = type;
            MoveInput = moveInput;
            CameraYaw = cameraYaw;
            IsSprinting = isSprinting;
            IsLeftHandAttack = isLeftHandAttack;
        }

        public static BufferedCharacterAction Attack(
            CharacterActionType type,
            bool isSprinting,
            bool isLeftHandAttack = false)
        {
            return new BufferedCharacterAction(
                type,
                Vector2.zero,
                0.0f,
                isSprinting,
                isLeftHandAttack);
        }

        public static BufferedCharacterAction Roll(Vector2 moveInput, float cameraYaw)
        {
            return new BufferedCharacterAction(
                CharacterActionType.Roll,
                moveInput,
                cameraYaw,
                false,
                false);
        }
    }

    public sealed class CharacterActionBuffer
    {
        private const float BUFFER_DURATION = 1f;

        private readonly ITimer _bufferTimer;

        private BufferedCharacterAction _bufferedAction;
        private bool _hasBufferedAction;

        public CharacterActionBuffer()
        {
            _bufferTimer = TimerFactory.ConstructTimer(BUFFER_DURATION);
        }

        public void Buffer(BufferedCharacterAction action)
        {
            ExpireAction();

            _bufferedAction = action;
            _hasBufferedAction = true;
            _bufferTimer
                .ChangeDuration(BUFFER_DURATION)
                .Start();
        }

        public bool TryPeek(
            out BufferedCharacterAction action,
            bool retainWhileActionActive)
        {
            if (!retainWhileActionActive)
            {
                ExpireAction();
            }

            action = _bufferedAction;
            return _hasBufferedAction;
        }

        public void Consume()
        {
            Clear();
        }

        public void Clear()
        {
            _bufferedAction = default;
            _hasBufferedAction = false;
            _bufferTimer.Reset();
        }

        private void ExpireAction()
        {
            if (_hasBufferedAction && _bufferTimer.IsComplete)
            {
                Clear();
            }
        }
    }
}
