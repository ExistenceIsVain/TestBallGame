using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>
    /// Rules of the ball, the shot and the movement. Sizes are radii in metres, volumes are radius^3.
    /// Economy: the minimum shot pays for the hit itself and infects only the obstacle it touches; volume above that
    /// buys infected AREA linearly. So in a dense field a bigger shot is cheaper per obstacle than tapping each one,
    /// while lone obstacles are best cleared with taps.
    /// </summary>
    [CreateAssetMenu(menuName = "BallGame/Gameplay Config", fileName = "GameplayConfig")]
    public sealed class GameplayConfig : ScriptableObject
    {
        [Header("Ball size")]
        [Tooltip("Critical size: if charging makes the ball smaller than this, the level is lost.")]
        [Min(0.05f)] public float minBallRadius = 0.3f;
        [Tooltip("Smallest start size the generator may give a level.")]
        [Min(0.1f)] public float minStartBallRadius = 0.6f;
        [Tooltip("Largest start size. A level that would need more gets thinned out.")]
        [Min(0.1f)] public float maxStartBallRadius = 1.5f;
        [Tooltip("Spare volume on top of what the level needs: start = critical + spent * (1 + reserve).")]
        [Range(0f, 1f)] public float sizeReserve = 0.2f;

        [Header("Charge")]
        [Tooltip("Volume taken from the ball the moment the finger touches the screen (the cheapest possible shot).")]
        [Min(0.001f)] public float minShotVolume = 0.05f;
        [Tooltip("Volume moved into the shot per second, as a share of the start volume.")]
        [Min(0f)] public float chargeRate = 0.12f;
        [Tooltip("How fast the transfer rate grows the longer the finger is held (share of start volume / s^2).")]
        [Min(0f)] public float chargeAcceleration = 0.3f;
        [Min(0.02f)] public float chargeTickInterval = 0.12f;

        [Header("Shot")]
        [Min(0.1f)] public float shotSpeed = 16f;
        [Tooltip("Square metres infected per unit of shot volume.")]
        [Min(0.1f)] public float infectionAreaPerVolume = 550f;
        [Tooltip("Infection jumps between obstacles whose centres are closer than this: dense groups are easy to clear.")]
        [Min(0.1f)] public float linkDistance = 1.3f;
        [Min(0f)] public float infectionStepDelay = 0.07f;
        [Min(0f)] public float explodeDelay = 0.3f;

        [Header("Path")]
        [Tooltip("An obstacle that overlaps the ball's corridor by less than this does not stop the ball: it gets knocked away.")]
        [Min(0f)] public float grazeTolerance = 0.3f;

        [Header("Movement")]
        [Min(0.1f)] public float hopLength = 1.1f;
        [Min(0f)] public float hopHeight = 0.5f;
        [Min(0.05f)] public float hopDuration = 0.2f;
        [Tooltip("The ball does not move for gains shorter than this.")]
        [Min(0f)] public float minAdvance = 0.3f;
        [Tooltip("The ball stops this far (metres, surface to surface) before the obstacle that blocks it, so the shot has room to fly.")]
        [Min(0f)] public float stopGap = 2f;
        [Tooltip("How far past the door the ball rolls to finish the level.")]
        [Min(0f)] public float finishBeyondDoor = 1.2f;

        [Header("Door")]
        [Min(0f)] public float doorOpenDistance = 5f;

        [Header("Level generation bot")]
        [Tooltip("How far past the first blocker the bot looks when choosing a shot size.")]
        [Min(0f)] public float botLookAhead = 6f;
        [Tooltip("How many shot sizes the bot compares per shot.")]
        [Min(1)] public int botCandidates = 10;
        [Min(1)] public int botMaxShots = 120;

        /// <summary>
        /// Radius around the impact point that a shot of this volume infects. The minimum shot only infects the
        /// obstacle it hits; every bit of volume above it adds infected area linearly.
        /// </summary>
        public float InfectionRadius(float shotVolume) =>
            Mathf.Sqrt(infectionAreaPerVolume * Mathf.Max(0f, shotVolume - minShotVolume) / Mathf.PI);

        /// <summary>Shot volume needed to infect everything within this radius.</summary>
        public float VolumeForRadius(float radius) => minShotVolume + Mathf.PI * radius * radius / infectionAreaPerVolume;
    }
}
