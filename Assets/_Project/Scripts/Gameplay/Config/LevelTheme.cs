using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>Look of a level: obstacle model and colour, ground gradient, sky colour.</summary>
    [CreateAssetMenu(menuName = "BallGame/Level Theme", fileName = "LevelTheme")]
    public sealed class LevelTheme : ScriptableObject
    {
        [Header("Obstacles")]
        public Mesh obstacleMesh;
        [Tooltip("Scale applied to the mesh so it is 2 x obstacle radius wide (0.8 m for the default radius).")]
        public Vector3 obstacleMeshScale = Vector3.one;
        public Material obstacleMaterial;
        public Color burstColor = new(1f, 0.6f, 0.15f);

        [Header("Environment")]
        public Material groundMaterial;
        [Tooltip("Camera clear colour: matches the far end of the ground gradient.")]
        public Color skyColor = Color.white;
    }
}
