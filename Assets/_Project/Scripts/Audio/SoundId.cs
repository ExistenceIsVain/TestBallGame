namespace BallGame.Audio
{
    /// <summary>Every sound the game plays. Values are serialized in AudioConfig: append, never renumber.</summary>
    public enum SoundId
    {
        Click = 0,
        Toggle = 1,
        ChargeTick = 2,
        Shot = 3,
        Hit = 4,
        Explode = 5,
        Hop = 6,
        DoorOpen = 7,
        Win = 8,
        Lose = 9,
        Purchase = 10,
        Error = 11,
    }
}
