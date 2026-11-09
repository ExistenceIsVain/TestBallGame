using UnityEngine;

namespace BallGame.Progression
{
    [CreateAssetMenu(menuName = "BallGame/Progression Config", fileName = "ProgressionConfig")]
    public sealed class ProgressionConfig : ScriptableObject
    {
        [Header("Rewards")]
        [Min(0)] public int baseReward = 50;
        [Min(0)] public int rewardPerLevel = 10;

        [Header("Difficulty")]
        [Tooltip("Level at which generation reaches the hardest settings.")]
        [Min(2)] public int maxDifficultyLevel = 20;

        public int RewardFor(int level) => baseReward + rewardPerLevel * Mathf.Max(0, level - 1);

        /// <summary>0 on level 1, 1 on maxDifficultyLevel and above.</summary>
        public float DifficultyFor(int level) => Mathf.Clamp01((level - 1) / (float)(maxDifficultyLevel - 1));
    }
}
