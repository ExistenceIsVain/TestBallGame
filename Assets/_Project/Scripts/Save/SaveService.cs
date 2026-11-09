using System;
using System.IO;
using UnityEngine;

namespace BallGame.Save
{
    /// <summary>
    /// Reads and writes SaveData as JSON in persistentDataPath.
    /// Writes go to a temp file first and the previous file is kept as a backup,
    /// so a crash during writing never leaves the player without a save.
    /// </summary>
    public sealed class SaveService
    {
        private const string FileName = "save.json";

        private readonly string path;
        private readonly string tempPath;
        private readonly string backupPath;

        public SaveData Data { get; private set; }

        public SaveService(string directory = null)
        {
            directory ??= Application.persistentDataPath;
            path = Path.Combine(directory, FileName);
            tempPath = path + ".tmp";
            backupPath = path + ".bak";
        }

        public void Load()
        {
            Data = TryRead(path) ?? TryRead(backupPath);

            if (Data == null)
            {
                Data = SaveData.CreateDefault();
                Write();
                return;
            }

            if (SaveMigrator.Migrate(Data))
                Write();
        }

        /// <summary>Deletes the save, its backup and any half-written temp file. The next Load starts from defaults.</summary>
        /// <returns>Number of files removed.</returns>
        public int DeleteFiles()
        {
            var removed = 0;
            foreach (var file in new[] { path, backupPath, tempPath })
            {
                if (!File.Exists(file))
                    continue;

                File.Delete(file);
                removed++;
            }

            return removed;
        }

        public void Write()
        {
            try
            {
                File.WriteAllText(tempPath, JsonUtility.ToJson(Data));

                if (File.Exists(path))
                {
                    File.Copy(path, backupPath, true);
                    File.Delete(path);
                }

                File.Move(tempPath, path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save write failed: {e.Message}");
            }
        }

        private static SaveData TryRead(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                // Overwrite a blank object: fields missing in old files keep their defaults, version stays 0.
                var data = new SaveData();
                JsonUtility.FromJsonOverwrite(json, data);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Save read failed ({Path.GetFileName(filePath)}): {e.Message}");
                return null;
            }
        }
    }
}
