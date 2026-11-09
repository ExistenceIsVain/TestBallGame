using System;
using BallGame.Save;

namespace BallGame.Progression
{
    /// <summary>Current level and coins.</summary>
    public sealed class ProgressService
    {
        private readonly SaveService save;
        private readonly ProgressionConfig config;

        public event Action CoinsChanged;
        public event Action LevelChanged;

        public int Level => save.Data.level;
        public int Coins => save.Data.coins;
        public float Difficulty => config.DifficultyFor(Level);
        public int CurrentReward => config.RewardFor(Level);

        public ProgressService(SaveService save, ProgressionConfig config)
        {
            this.save = save;
            this.config = config;

            if (save.Data.level < 1)
                save.Data.level = 1;
        }

        /// <summary>Grants the level reward and moves to the next level.</summary>
        /// <returns>Coins granted.</returns>
        public int CompleteLevel()
        {
            var reward = CurrentReward;
            save.Data.coins += reward;
            save.Data.level++;
            save.Write();

            CoinsChanged?.Invoke();
            LevelChanged?.Invoke();
            return reward;
        }

        public bool CanAfford(int amount) => Coins >= amount;

        public bool TrySpend(int amount)
        {
            if (amount < 0 || !CanAfford(amount))
                return false;

            save.Data.coins -= amount;
            save.Write();
            CoinsChanged?.Invoke();
            return true;
        }
    }
}
