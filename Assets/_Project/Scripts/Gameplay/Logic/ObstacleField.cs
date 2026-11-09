using System;
using System.Collections.Generic;
using UnityEngine;

namespace BallGame.Gameplay
{
    public readonly struct InfectionNode
    {
        public readonly int Index;
        public readonly int Depth;

        public InfectionNode(int index, int depth)
        {
            Index = index;
            Depth = depth;
        }
    }

    /// <summary>
    /// Obstacles as plain data: centres on the ground plane, one shared radius, alive flags.
    /// All gameplay queries (what blocks the ball, what an impact infects) are answered here
    /// without physics, so the runtime and the level generator's bot use exactly the same rules.
    /// </summary>
    public sealed class ObstacleField
    {
        private readonly Queue<InfectionNode> queue = new();

        private Vector2[] positions = Array.Empty<Vector2>();
        private bool[] alive = Array.Empty<bool>();
        private bool[] visited = Array.Empty<bool>();

        // Uniform grid for neighbour search: per-cell linked lists in two int arrays.
        private int[] cellHead = Array.Empty<int>();
        private int[] cellNext = Array.Empty<int>();
        private Vector2 gridOrigin;
        private float cellSize = 1f;
        private int columns;
        private int rows;

        public int Count => positions.Length;
        public int AliveCount { get; private set; }
        public float ObstacleRadius { get; private set; }

        public Vector2 GetPosition(int index) => positions[index];
        public bool IsAlive(int index) => alive[index];

        public void Build(IReadOnlyList<Vector2> points, float obstacleRadius, float gridCellSize)
        {
            var count = points.Count;
            if (positions.Length != count)
            {
                positions = new Vector2[count];
                alive = new bool[count];
                visited = new bool[count];
                cellNext = new int[count];
            }

            ObstacleRadius = obstacleRadius;
            cellSize = Mathf.Max(0.1f, gridCellSize);

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < count; i++)
            {
                positions[i] = points[i];
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            if (count == 0)
                min = max = Vector2.zero;

            gridOrigin = min;
            columns = Mathf.FloorToInt((max.x - min.x) / cellSize) + 1;
            rows = Mathf.FloorToInt((max.y - min.y) / cellSize) + 1;

            if (cellHead.Length < columns * rows)
                cellHead = new int[columns * rows];

            Array.Fill(cellHead, -1, 0, columns * rows);
            for (int i = 0; i < count; i++)
            {
                var cell = CellIndex(positions[i]);
                cellNext[i] = cellHead[cell];
                cellHead[cell] = i;
            }

            ReviveAll();
        }

        public void ReviveAll()
        {
            Array.Fill(alive, true);
            AliveCount = positions.Length;
        }

        public void Kill(int index)
        {
            if (!alive[index])
                return;

            alive[index] = false;
            AliveCount--;
        }

        public void Revive(int index)
        {
            if (alive[index])
                return;

            alive[index] = true;
            AliveCount++;
        }

        /// <summary>
        /// Finds the nearest alive obstacle that stops a ball of <paramref name="ballRadius"/> rolling from
        /// <paramref name="from"/> along <paramref name="direction"/> within <paramref name="maxDistance"/>.
        /// Obstacles overlapping the corridor by less than <paramref name="grazeTolerance"/> do not stop the ball
        /// (see <see cref="CollectSwept"/>).
        /// </summary>
        /// <param name="contactDistance">How far the ball can roll before touching it (maxDistance if nothing blocks).</param>
        /// <returns>Obstacle index, or -1 if the way is clear.</returns>
        public int FindFirstBlocker(Vector2 from, Vector2 direction, float maxDistance, float ballRadius, float grazeTolerance,
            out float contactDistance)
        {
            var reach = ballRadius + ObstacleRadius;
            var reachSq = reach * reach;
            var blockReach = Mathf.Max(0f, reach - grazeTolerance);
            var blockReachSq = blockReach * blockReach;
            var best = -1;
            var bestContact = maxDistance;

            for (int i = 0; i < positions.Length; i++)
            {
                if (!alive[i])
                    continue;

                var offset = positions[i] - from;
                var along = Vector2.Dot(offset, direction);
                if (along <= 0f)
                    continue;

                var lateral = offset.x * direction.y - offset.y * direction.x;
                var lateralSq = lateral * lateral;
                if (lateralSq >= blockReachSq)
                    continue;

                var contact = along - Mathf.Sqrt(reachSq - lateralSq);
                if (contact < bestContact)
                {
                    bestContact = contact;
                    best = i;
                }
            }

            contactDistance = Mathf.Max(0f, bestContact);
            return best;
        }

        /// <summary>
        /// Where a shot of <paramref name="shotRadius"/> flying from <paramref name="from"/> straight along
        /// <paramref name="direction"/> (parallel to the path) explodes: at the first obstacle it touches, or abreast of
        /// an obstacle that blocks the ball's corridor from the side, whichever comes first.
        /// </summary>
        /// <param name="explosionDistance">Distance along the path from <paramref name="from"/> to the shot centre at the explosion.</param>
        /// <returns>The obstacle the shot explodes on, or -1 if nothing is in the way.</returns>
        public int FindShotTarget(Vector2 from, Vector2 direction, float maxDistance, float ballRadius, float grazeTolerance,
            float shotRadius, out float explosionDistance)
        {
            var touch = shotRadius + ObstacleRadius;
            var touchSq = touch * touch;
            var blockReach = Mathf.Max(0f, ballRadius + ObstacleRadius - grazeTolerance);
            var best = -1;
            var bestDistance = float.MaxValue;

            for (int i = 0; i < positions.Length; i++)
            {
                if (!alive[i])
                    continue;

                var offset = positions[i] - from;
                var along = Vector2.Dot(offset, direction);
                if (along <= 0f || along > maxDistance + touch)
                    continue;

                var lateral = offset.x * direction.y - offset.y * direction.x;
                var lateralSq = lateral * lateral;
                float distance;
                if (lateralSq < touchSq)
                    distance = along - Mathf.Sqrt(touchSq - lateralSq); // the shot touches it
                else if (Mathf.Abs(lateral) < blockReach)
                    distance = along; // blocks the ball from the side: the shot bursts beside it
                else
                    continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            explosionDistance = best < 0 ? maxDistance : Mathf.Max(0f, bestDistance);
            return best;
        }

        /// <summary>Alive obstacles the ball overlaps while rolling from <paramref name="from"/> to <paramref name="to"/>: grazed ones it knocks away.</summary>
        public void CollectSwept(Vector2 from, Vector2 to, float ballRadius, List<int> result)
        {
            result.Clear();
            var reach = ballRadius + ObstacleRadius;
            var reachSq = reach * reach;
            var segment = to - from;
            var lengthSq = segment.sqrMagnitude;

            for (int i = 0; i < positions.Length; i++)
            {
                if (!alive[i])
                    continue;

                var t = lengthSq > 1e-6f ? Mathf.Clamp01(Vector2.Dot(positions[i] - from, segment) / lengthSq) : 0f;
                if ((positions[i] - (from + segment * t)).sqrMagnitude < reachSq)
                    result.Add(i);
            }
        }

        /// <summary>Distances from <paramref name="point"/> to alive obstacles that stop the ball within a stretch of its path.</summary>
        public void CollectBlockerDistances(Vector2 from, Vector2 direction, float maxDistance, float ballRadius, float grazeTolerance,
            Vector2 point, List<float> result)
        {
            result.Clear();
            var blockReach = Mathf.Max(0f, ballRadius + ObstacleRadius - grazeTolerance);

            for (int i = 0; i < positions.Length; i++)
            {
                if (!alive[i])
                    continue;

                var offset = positions[i] - from;
                var along = Vector2.Dot(offset, direction);
                if (along <= 0f || along > maxDistance)
                    continue;

                var lateral = offset.x * direction.y - offset.y * direction.x;
                if (Mathf.Abs(lateral) < blockReach)
                    result.Add(Vector2.Distance(positions[i], point));
            }
        }

        /// <summary>
        /// Infection spreads from the hit obstacle to neighbours closer than <paramref name="linkDistance"/>,
        /// but never beyond <paramref name="radius"/> from the impact point. Result is in spreading order with wave depth.
        /// </summary>
        public void CollectInfection(int hitIndex, Vector2 impact, float radius, float linkDistance, List<InfectionNode> result)
        {
            result.Clear();
            if (hitIndex < 0 || !alive[hitIndex])
                return;

            Array.Clear(visited, 0, visited.Length);
            queue.Clear();

            var radiusSq = radius * radius;
            var linkSq = linkDistance * linkDistance;
            var cellRange = Mathf.CeilToInt(linkDistance / cellSize);

            visited[hitIndex] = true;
            queue.Enqueue(new InfectionNode(hitIndex, 0));

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                result.Add(node);

                var p = positions[node.Index];
                var cx = CellX(p.x);
                var cy = CellY(p.y);

                for (int y = Mathf.Max(0, cy - cellRange); y <= Mathf.Min(rows - 1, cy + cellRange); y++)
                for (int x = Mathf.Max(0, cx - cellRange); x <= Mathf.Min(columns - 1, cx + cellRange); x++)
                {
                    for (int j = cellHead[y * columns + x]; j >= 0; j = cellNext[j])
                    {
                        if (visited[j] || !alive[j])
                            continue;

                        var q = positions[j];
                        if ((q - p).sqrMagnitude > linkSq || (q - impact).sqrMagnitude > radiusSq)
                            continue;

                        visited[j] = true;
                        queue.Enqueue(new InfectionNode(j, node.Depth + 1));
                    }
                }
            }
        }

        private int CellX(float x) => Mathf.Clamp((int)((x - gridOrigin.x) / cellSize), 0, columns - 1);
        private int CellY(float y) => Mathf.Clamp((int)((y - gridOrigin.y) / cellSize), 0, rows - 1);
        private int CellIndex(Vector2 p) => CellY(p.y) * columns + CellX(p.x);
    }
}
