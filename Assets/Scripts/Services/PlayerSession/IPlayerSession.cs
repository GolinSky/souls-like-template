namespace SoulsLike.Services.PlayerSession
{
    /// <summary>Owns the local binding that submits intent to one actor runtime.</summary>
    public interface IPlayerSession
    {
        PlayerSessionSnapshot Snapshot { get; }
        void Submit(in PlayerSessionInput input);
        void ClearInput();
    }
}
