using BallGame.Core;
using BallGame.UI;
using ProjectUI;
using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>Game scene root: generates the current level, applies its theme, starts the session and shows results.</summary>
    public sealed class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private LevelSession session;
        [SerializeField] private LevelEnvironment environment;
        [SerializeField] private ScreenManager screens;
        [SerializeField] private LevelGenerationConfig generationConfig;
        [SerializeField] private GameplayConfig gameplayConfig;
        [SerializeField] private LevelThemeCatalog themes;

        private App app;
        private LevelGenerator generator;
        private WinScreen winScreen;
        private LoseScreen loseScreen;

        private void Start()
        {
            app = App.Instance;
            generator = new LevelGenerator(generationConfig, gameplayConfig);

            session.Won += OnWon;
            session.Lost += OnLost;
            app.Skins.Changed += ApplySkin;

            winScreen = screens.GetScreen<WinScreen>();
            loseScreen = screens.GetScreen<LoseScreen>();
            winScreen.NextClicked += StartLevel;
            loseScreen.RetryClicked += StartLevel;

            StartLevel();
            screens.OpenScreen<HudScreen, LevelSession>(session);
        }

        private void OnDestroy()
        {
            if (app != null)
                app.Skins.Changed -= ApplySkin;

            session.Won -= OnWon;
            session.Lost -= OnLost;

            if (winScreen != null)
                winScreen.NextClicked -= StartLevel;
            if (loseScreen != null)
                loseScreen.RetryClicked -= StartLevel;
        }

        private void StartLevel()
        {
            screens.CloseScreen<WinScreen>();
            screens.CloseScreen<LoseScreen>();

            var level = app.Progress.Level;
            environment.Apply(themes.ForLevel(level));
            session.Begin(generator.Generate(level, app.Progress.Difficulty));
            ApplySkin();
        }

        private void ApplySkin()
        {
            var skin = app.Skins.Selected;
            session.SetSkin(skin.material, skin.mainColor);
        }

        private void OnWon()
        {
            var reward = app.Progress.CompleteLevel();
            screens.OpenScreen<WinScreen, int>(reward);
        }

        private void OnLost(LoseReason reason) => screens.OpenScreen<LoseScreen, LoseReason>(reason);
    }
}
