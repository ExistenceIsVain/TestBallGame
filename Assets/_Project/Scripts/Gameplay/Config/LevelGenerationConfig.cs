using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>
    /// Field generation. Vector2 values are (easiest, hardest): they are interpolated by level difficulty 0..1.
    /// </summary>
    [CreateAssetMenu(menuName = "BallGame/Level Generation Config", fileName = "LevelGenerationConfig")]
    public sealed class LevelGenerationConfig : ScriptableObject
    {
        [Header("Path")]
        public Vector2 pathLength = new(15f, 26f);
        [Tooltip("Start is shifted left and the door right by this much: the path runs diagonally across the screen.")]
        [Min(0f)] public float lateralOffset = 2.6f;

        [Header("Field")]
        [Min(1f)] public float fieldHalfWidth = 8f;
        [Tooltip("Obstacles also fill this far behind the start and beyond the door.")]
        [Min(0f)] public float fieldPadding = 8f;
        [Min(0.05f)] public float obstacleRadius = 0.4f;
        [Tooltip("Grid step between obstacles: smaller is denser. Early levels are dense so infection chains spread " +
                 "(forgiving); later ones are sparser than GameplayConfig.linkDistance, so shots must be aimed and sized.")]
        public Vector2 spacing = new(1.05f, 1.3f);
        [Range(0f, 0.5f)] public float jitter = 0.25f;
        [Tooltip("Share of the field left empty: higher means more gaps and lone obstacles (harder).")]
        public Vector2 emptiness = new(0.25f, 0.45f);

        [Header("Patterns")]
        [Min(0.01f)] public float noiseScale = 0.22f;
        [Tooltip("Stripes: distance between two bands across the path.")]
        [Min(1f)] public float stripePeriod = 4.5f;
        [Tooltip("Islands: average distance between island centres.")]
        [Min(1f)] public float islandSpacing = 4.6f;

        [Header("Clear zones")]
        [Min(0f)] public float startClearRadius = 2.4f;
        [Min(0f)] public float doorClearRadius = 3.4f;

        [Header("Retries")]
        [Tooltip("When a level would need a ball larger than allowed, this share of obstacles near the path is removed per retry.")]
        [Range(0f, 1f)] public float thinningStep = 0.15f;
        [Min(1)] public int maxAttempts = 6;
    }
}
