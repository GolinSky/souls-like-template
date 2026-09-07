namespace SoulsLike.Entities.Ladder
{
    public interface ILadderAnimator
    {
        void SetOnLadder(bool isOnLadder);
        void SetLadderClimbSpeed(float speed);
        void SetLadderSlide(bool isSliding);
        void TriggerLadderIdle();
        void TriggerLadderEnterBottom();
        void TriggerLadderEnterTop();
        void TriggerLadderExitBottom();
        void TriggerLadderExitTop();
        void TriggerLadderPunch();
        void TriggerLadderKick();
        void TriggerLadderDrink();
        void TriggerLadderUnlock();
    }
}
