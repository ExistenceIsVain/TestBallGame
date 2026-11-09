using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>Applies a level theme to the scene: ground, sky colour and obstacle look.</summary>
    public sealed class LevelEnvironment : MonoBehaviour
    {
        [SerializeField] private Renderer ground;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private ObstacleSpawner obstacles;

        public void Apply(LevelTheme theme)
        {
            ground.sharedMaterial = theme.groundMaterial;
            sceneCamera.backgroundColor = theme.skyColor;
            obstacles.SetTheme(theme);
        }
    }
}
