using System;
using System.Collections.Generic;

namespace BallGame.Save
{
    /// <summary>Everything the game persists. Serialized with JsonUtility, so only public fields of simple types.</summary>
    [Serializable]
    public sealed class SaveData
    {
        // Missing in files written before versioning existed: such files load with version 0.
        public int version;

        public int level = 1;
        public int coins;

        public string selectedSkin = string.Empty;
        public List<string> ownedSkins = new();

        public bool musicOn = true;
        public bool soundOn = true;
        public bool vibrationOn = true;

        public static SaveData CreateDefault() => new() { version = SaveMigrator.CurrentVersion };
    }
}
