using System.Collections.Generic;
using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>
    /// A bot that plays a level with the real rules, several times with different strategies, and reports the
    /// cheapest run: tapping every blocker, fixed infection radii, and a greedy mode that compares shot sizes
    /// before every shot. The generator sizes the ball from that cheapest run, so a level is only as generous
    /// as <see cref="GameplayConfig.sizeReserve"/> allows, whatever way the player chooses to play it.
    /// </summary>
    public sealed class PassageSimulator
    {
        private const float Greedy = -1f;

        // Infection radius in metres each strategy aims for (0 = tap only).
        private static readonly float[] Strategies = { Greedy, 0f, 1f, 1.5f, 2f, 2.5f, 3f };

        public struct Result
        {
            public bool Cleared;
            public float SpentVolume;
            public int Shots;
        }

        private readonly List<InfectionNode> infection = new();
        private readonly List<int> swept = new();
        private readonly List<float> distances = new();

        /// <summary>Cheapest successful run over all strategies.</summary>
        public Result RunBest(LevelLayout layout, ObstacleField field, GameplayConfig config, float startRadius)
        {
            var best = new Result();
            foreach (var strategy in Strategies)
            {
                var result = Run(layout, field, config, startRadius, strategy);
                if (result.Cleared && (!best.Cleared || result.SpentVolume < best.SpentVolume))
                    best = result;
            }

            return best;
        }

        private Result Run(LevelLayout layout, ObstacleField field, GameplayConfig config, float startRadius, float strategy)
        {
            field.ReviveAll();

            var result = new Result();
            var volume = BallMath.Volume(startRadius);
            var position = layout.Start;
            var direction = layout.Direction;
            var remaining = layout.PathLength;

            for (int shot = 0; shot < config.botMaxShots; shot++)
            {
                // The corridor never gets narrower than the critical size: the generator checks volume separately.
                var ballRadius = Mathf.Max(BallMath.Radius(volume), config.minBallRadius);
                var blocker = field.FindFirstBlocker(position, direction, remaining, ballRadius, config.grazeTolerance, out var contact);
                if (blocker < 0)
                {
                    result.Cleared = true;
                    break;
                }

                var advance = contact - config.stopGap;
                if (advance >= config.minAdvance)
                {
                    Roll(field, ref position, direction, advance, ballRadius);
                    remaining -= advance;
                }

                var shotVolume = strategy == Greedy
                    ? ChooseShot(field, config, position, direction, remaining, ballRadius, volume)
                    : config.VolumeForRadius(strategy);
                CollectShot(field, config, position, direction, remaining, ballRadius, volume, shotVolume);
                foreach (var node in infection)
                    field.Kill(node.Index);

                volume -= shotVolume;
                result.SpentVolume += shotVolume;
                result.Shots++;
            }

            field.ReviveAll();
            return result;
        }

        /// <summary>Same rule as the game: the shot flies straight along the path and infects from where it bursts.</summary>
        private Vector2 CollectShot(ObstacleField field, GameplayConfig config, Vector2 position, Vector2 direction, float remaining,
            float ballRadius, float volume, float shotVolume)
        {
            var shotRadius = BallMath.Radius(shotVolume);
            var target = field.FindShotTarget(position, direction, remaining, ballRadius, config.grazeTolerance, shotRadius, out var distance);
            var radiusAfterShot = Mathf.Max(BallMath.Radius(volume - shotVolume), config.minBallRadius);
            var explosion = BallMath.ExplosionPoint(position, direction, distance, radiusAfterShot, shotRadius);
            field.CollectInfection(target, explosion, config.InfectionRadius(shotVolume), config.linkDistance, infection);
            return explosion;
        }

        private float ChooseShot(ObstacleField field, GameplayConfig config, Vector2 position, Vector2 direction, float remaining,
            float ballRadius, float volume)
        {
            // Where the cheapest shot would burst: candidate sizes are measured from there.
            var explosion = CollectShot(field, config, position, direction, remaining, ballRadius, volume, config.minShotVolume);
            field.CollectBlockerDistances(position, direction, config.botLookAhead + ballRadius * 2f, ballRadius,
                config.grazeTolerance, explosion, distances);
            distances.Sort();

            var bestVolume = config.minShotVolume;
            var bestScore = float.MinValue;
            var candidates = Mathf.Min(config.botCandidates, distances.Count + 1);

            for (int c = 0; c < candidates; c++)
            {
                // Candidate 0 is the cheapest tap; the others cover more and more of the blockers ahead.
                var radius = c == 0 ? 0f : distances[Mathf.Min(distances.Count - 1, c * distances.Count / (candidates - 1))] + 0.01f;
                var shotVolume = config.VolumeForRadius(radius);

                CollectShot(field, config, position, direction, remaining, ballRadius, volume, shotVolume);
                foreach (var node in infection)
                    field.Kill(node.Index);

                var nextRadius = Mathf.Max(BallMath.Radius(volume - shotVolume), config.minBallRadius);
                field.FindFirstBlocker(position, direction, remaining, nextRadius, config.grazeTolerance, out var freed);

                foreach (var node in infection)
                    field.Revive(node.Index);

                var score = freed / shotVolume;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestVolume = shotVolume;
                }
            }

            return bestVolume;
        }

        private void Roll(ObstacleField field, ref Vector2 position, Vector2 direction, float distance, float ballRadius)
        {
            var target = position + direction * distance;
            field.CollectSwept(position, target, ballRadius, swept);
            foreach (var index in swept)
                field.Kill(index);
            position = target;
        }
    }
}
