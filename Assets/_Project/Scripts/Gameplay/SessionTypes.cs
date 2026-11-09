namespace BallGame.Gameplay
{
    public enum SessionState
    {
        None,
        Idle,       // waiting for the player to press
        Charging,   // finger is down, the shot grows
        Resolving,  // shot flies, obstacles explode
        Moving,     // ball hops forward
        Won,
        Lost,
    }

    public enum LoseReason
    {
        Overcharged, // held too long: the ball shrank below the critical size
        OutOfSize,   // path still blocked and the ball is too small for another shot
    }
}
