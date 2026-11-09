using UnityEngine;

namespace BallGame.Gameplay
{
    [CreateAssetMenu(menuName = "BallGame/Level Theme Catalog", fileName = "LevelThemeCatalog")]
    public sealed class LevelThemeCatalog : ScriptableObject
    {
        [SerializeField] private LevelTheme[] themes;

        /// <summary>Themes cycle level by level.</summary>
        public LevelTheme ForLevel(int level) => themes[(Mathf.Max(1, level) - 1) % themes.Length];
    }
}
