using UnityEngine;

namespace BallGame.Save
{
    /// <summary>
    /// Upgrades old save files step by step: v1 -> v2 -> ... -> CurrentVersion.
    /// To change the save format: bump CurrentVersion and add one MigrateVxToVy step.
    /// </summary>
    public static class SaveMigrator
    {
        public const int CurrentVersion = 2;

        /// <returns>True if the data was changed and should be written back.</returns>
        public static bool Migrate(SaveData data)
        {
            if (data.version > CurrentVersion)
            {
                // A save from a newer build: keep it untouched instead of guessing a downgrade.
                Debug.LogWarning($"Save version {data.version} is newer than supported {CurrentVersion}.");
                return false;
            }

            var startVersion = data.version;

            // Files written before versioning are treated as v1.
            if (data.version < 1)
                data.version = 1;

            while (data.version < CurrentVersion)
            {
                switch (data.version)
                {
                    case 1: MigrateV1ToV2(data); break;
                }

                data.version++;
            }

            return data.version != startVersion;
        }

        // v1 had a single "soundOn" switch for all audio; v2 adds a separate music switch.
        private static void MigrateV1ToV2(SaveData data)
        {
            data.musicOn = data.soundOn;
        }
    }
}
