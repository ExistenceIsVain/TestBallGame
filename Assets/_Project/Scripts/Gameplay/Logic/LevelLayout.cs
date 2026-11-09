using System.Collections.Generic;
using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>How obstacles are grouped on the field. Levels cycle through these.</summary>
    public enum FieldPattern
    {
        Clusters, // noise blobs with lone obstacles between them
        Stripes,  // dense bands across the path with gaps between them
        Islands,  // round groups scattered over the field
    }

    /// <summary>Generated level: where the ball starts, where the door is, obstacle centres on the ground plane (x, z).</summary>
    public sealed class LevelLayout
    {
        public readonly List<Vector2> Obstacles = new();

        public int Level;
        public FieldPattern Pattern;
        public Vector2 Start;
        public Vector2 Door;
        public float ObstacleRadius;
        public float BallRadius;

        // What the generator's bot needed: kept for balancing and debugging.
        public int BotShots;
        public float BotSpentVolume;

        public Vector2 Direction => (Door - Start).normalized;
        public float PathLength => Vector2.Distance(Start, Door);
    }
}
