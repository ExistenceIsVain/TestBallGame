using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace BallGame.Gameplay
{
    /// <summary>
    /// Builds a level from its number: the same level always produces the same layout (retry = same field).
    /// Then sizes the ball so the bot passes it with exactly <see cref="GameplayConfig.sizeReserve"/> to spare.
    /// </summary>
    public sealed class LevelGenerator
    {
        private const int FitIterations = 12;

        private readonly LevelGenerationConfig generation;
        private readonly GameplayConfig gameplay;
        private readonly ObstacleField field = new();
        private readonly PassageSimulator simulator = new();
        private readonly List<Vector2> islands = new();

        public LevelGenerator(LevelGenerationConfig generation, GameplayConfig gameplay)
        {
            this.generation = generation;
            this.gameplay = gameplay;
        }

        public static FieldPattern PatternFor(int level)
        {
            var patterns = Enum.GetValues(typeof(FieldPattern));
            return (FieldPattern)patterns.GetValue((level - 1) % patterns.Length);
        }

        public LevelLayout Generate(int level, float difficulty)
        {
            var layout = new LevelLayout
            {
                Level = level,
                Pattern = PatternFor(level),
                ObstacleRadius = generation.obstacleRadius,
            };
            var thinning = 0f;

            for (int attempt = 0; attempt < generation.maxAttempts; attempt++)
            {
                // Seed depends on the attempt too, so a thinned retry is still deterministic.
                var random = new Random(unchecked(level * 92821 + attempt * 7919 + 17));
                PlaceObstacles(layout, random, difficulty, thinning);

                if (FitBallRadius(layout))
                    return layout;

                thinning += generation.thinningStep;
            }

            Debug.LogWarning($"Level {level}: could not fit the ball size, using the maximum.");
            layout.BallRadius = gameplay.maxStartBallRadius;
            return layout;
        }

        private void PlaceObstacles(LevelLayout layout, Random random, float difficulty, float thinning)
        {
            var length = Mathf.Lerp(generation.pathLength.x, generation.pathLength.y, difficulty);
            var spacing = Mathf.Lerp(generation.spacing.x, generation.spacing.y, difficulty);
            var emptiness = Mathf.Lerp(generation.emptiness.x, generation.emptiness.y, difficulty);

            layout.Start = new Vector2(-generation.lateralOffset, 0f);
            layout.Door = new Vector2(generation.lateralOffset, length);
            layout.Obstacles.Clear();

            var direction = layout.Direction;
            var finish = layout.Door + direction * gameplay.finishBeyondDoor;
            var noiseOffset = new Vector2(Range(random, 0f, 1000f), Range(random, 0f, 1000f));
            var halfWidth = generation.fieldHalfWidth;
            var zMin = -generation.fieldPadding;
            var zMax = length + generation.fieldPadding;

            if (layout.Pattern == FieldPattern.Islands)
                PlaceIslandCentres(random, halfWidth, zMin, zMax);

            var rowStep = spacing * 0.866f; // hex packing
            var row = 0;
            for (var z = zMin; z <= zMax; z += rowStep, row++)
            {
                var rowShift = (row & 1) == 0 ? 0f : spacing * 0.5f;
                for (var x = -halfWidth + rowShift; x <= halfWidth; x += spacing)
                {
                    var jitter = generation.jitter * spacing;
                    var p = new Vector2(x + Range(random, -jitter, jitter), z + Range(random, -jitter, jitter));

                    if (Vector2.Distance(p, layout.Start) < generation.startClearRadius)
                        continue;
                    if (DistanceToSegment(p, layout.Door, finish) < generation.doorClearRadius)
                        continue;

                    var noise = Mathf.PerlinNoise((p.x + noiseOffset.x) * generation.noiseScale, (p.y + noiseOffset.y) * generation.noiseScale);
                    if (!Keep(layout.Pattern, p, noise, emptiness))
                        continue;

                    if (thinning > 0f && DistanceToSegment(p, layout.Start, layout.Door) < gameplay.maxStartBallRadius + 1f
                        && random.NextDouble() < thinning)
                        continue;

                    layout.Obstacles.Add(p);
                }
            }
        }

        private bool Keep(FieldPattern pattern, Vector2 p, float noise, float emptiness)
        {
            switch (pattern)
            {
                case FieldPattern.Stripes:
                {
                    // Bands across the path, slightly wavy so they do not look ruled.
                    var band = Mathf.Repeat((p.y + (noise - 0.5f) * 2f) / generation.stripePeriod, 1f);
                    return band >= emptiness;
                }
                case FieldPattern.Islands:
                {
                    // Island size is chosen so roughly (1 - emptiness) of the field is covered.
                    var radius = generation.islandSpacing * Mathf.Sqrt((1f - emptiness) / Mathf.PI) * (0.85f + noise * 0.3f);
                    foreach (var centre in islands)
                    {
                        if ((p - centre).sqrMagnitude < radius * radius)
                            return true;
                    }

                    return false;
                }
                default:
                    return noise >= emptiness;
            }
        }

        private void PlaceIslandCentres(Random random, float halfWidth, float zMin, float zMax)
        {
            islands.Clear();
            var step = generation.islandSpacing;
            for (var z = zMin; z <= zMax; z += step)
            for (var x = -halfWidth; x <= halfWidth; x += step)
                islands.Add(new Vector2(x + Range(random, -0.4f, 0.4f) * step, z + Range(random, -0.4f, 0.4f) * step));
        }

        // Fixed-point search: a bigger ball has a wider corridor and needs more shots, so iterate until stable.
        private bool FitBallRadius(LevelLayout layout)
        {
            field.Build(layout.Obstacles, layout.ObstacleRadius, gameplay.linkDistance);

            var minVolume = BallMath.Volume(gameplay.minBallRadius);
            var radius = gameplay.minStartBallRadius;

            for (int i = 0; i < FitIterations; i++)
            {
                var result = simulator.RunBest(layout, field, gameplay, radius);
                if (!result.Cleared)
                    return false;

                var needed = BallMath.Radius(minVolume + result.SpentVolume * (1f + gameplay.sizeReserve));
                if (needed > gameplay.maxStartBallRadius)
                    return false;

                var atFloor = radius <= gameplay.minStartBallRadius && needed <= radius;
                if (atFloor || Mathf.Abs(needed - radius) < 0.005f || i == FitIterations - 1)
                {
                    layout.BallRadius = Mathf.Max(radius, needed);
                    layout.BotShots = result.Shots;
                    layout.BotSpentVolume = result.SpentVolume;
                    return true;
                }

                radius = Mathf.Max(gameplay.minStartBallRadius, needed);
            }

            return false;
        }

        private static float Range(Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
