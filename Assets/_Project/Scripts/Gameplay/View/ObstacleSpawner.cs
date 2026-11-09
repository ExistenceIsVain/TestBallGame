using System;
using System.Collections.Generic;
using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>Keeps a pool of obstacle views; index in the pool = index in the level layout.</summary>
    public sealed class ObstacleSpawner : MonoBehaviour
    {
        private const float KnockExplodeDelay = 0.06f;

        [SerializeField] private ObstacleView prefab;
        [Tooltip("Obstacle radius the theme mesh scales are authored for.")]
        [SerializeField] private float prefabRadius = 0.4f;
        [SerializeField] private BurstFx explosionFx;

        private readonly List<ObstacleView> views = new();
        private Action<ObstacleView> onExploded;
        private LevelTheme theme;

        public event Action Exploded;

        private void Awake() => onExploded = HandleExploded;

        public void SetTheme(LevelTheme levelTheme)
        {
            theme = levelTheme;
            foreach (var view in views)
                view.SetLook(theme.obstacleMesh, theme.obstacleMeshScale, theme.obstacleMaterial);
        }

        public void Spawn(LevelLayout layout)
        {
            var count = layout.Obstacles.Count;
            while (views.Count < count)
            {
                var view = Instantiate(prefab, transform);
                view.SetLook(theme.obstacleMesh, theme.obstacleMeshScale, theme.obstacleMaterial);
                views.Add(view);
            }

            var random = new System.Random(layout.Level);
            var scale = layout.ObstacleRadius / prefabRadius;

            for (int i = 0; i < views.Count; i++)
            {
                if (i >= count)
                {
                    views[i].gameObject.SetActive(false);
                    continue;
                }

                var yaw = (float)random.NextDouble() * 360f;
                var variation = 0.92f + (float)random.NextDouble() * 0.16f;
                views[i].Place(BallMath.World(layout.Obstacles[i]), yaw, scale * variation);
            }
        }

        public void Infect(int index, float delay, float explodeDelay)
        {
            views[index].Infect(delay, explodeDelay, onExploded);
        }

        /// <summary>The ball brushed this obstacle: it pops almost at once.</summary>
        public void Knock(int index, float delay)
        {
            views[index].Infect(delay, KnockExplodeDelay, onExploded);
        }

        private void HandleExploded(ObstacleView view)
        {
            explosionFx.Burst(view.transform.position + Vector3.up * 0.4f, theme.burstColor);
            Exploded?.Invoke();
        }
    }
}
