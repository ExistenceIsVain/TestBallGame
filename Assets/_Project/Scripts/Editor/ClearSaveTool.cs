using BallGame.Save;
using UnityEditor;
using UnityEngine;

namespace BallGame.Editor
{
    /// <summary>Editor menu: wipes the player's save (level, coins, skins, settings) so the game starts fresh.</summary>
    public static class ClearSaveTool
    {
        private const string MenuPath = "BallGame/Clear Save";

        [MenuItem(MenuPath)]
        private static void ClearSave()
        {
            var confirmed = EditorUtility.DisplayDialog("Clear Save",
                $"Delete the save in\n{Application.persistentDataPath}?\n\nLevel, coins, skins and settings will be reset.",
                "Delete", "Cancel");
            if (!confirmed)
                return;

            var removed = new SaveService().DeleteFiles();
            Debug.Log(removed > 0 ? $"Save cleared ({removed} file(s) removed)." : "No save to clear.");
        }

        // In Play Mode the running game keeps its data in memory and would write it straight back.
        [MenuItem(MenuPath, true)]
        private static bool CanClearSave() => !EditorApplication.isPlayingOrWillChangePlaymode;
    }
}
