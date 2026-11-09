using BallGame.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    /// <summary>In-game HUD: level, coins, remaining ball size, hold hint, settings and shop buttons.</summary>
    public sealed class HudScreen : GameScreen, IAcceptArg<LevelSession>
    {
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private string levelFormat = "LEVEL {0}";
        [SerializeField] private ProgressBarView sizeBar;
        [SerializeField] private GameObject holdHint;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button shopButton;

        private LevelSession session;

        public void AcceptArg(LevelSession arg)
        {
            if (session == arg)
                return;

            Unsubscribe();
            session = arg;

            if (IsOpen)
                Subscribe();
        }

        protected override void OnInitialize()
        {
            settingsButton.onClick.AddListener(() => Manager.OpenScreen<SettingsScreen>());
            shopButton.onClick.AddListener(() => Manager.OpenScreen<ShopScreen>());
        }

        protected override void OnBeforeOpen() => Subscribe();
        protected override void OnAfterClose() => Unsubscribe();
        protected override void OnScreenDestroyed() => Unsubscribe();

        private void Update()
        {
            if (session != null)
                sizeBar.SetValue(session.SizeLeft);
        }

        private void Subscribe()
        {
            if (session == null)
                return;

            session.StateChanged += OnStateChanged;
            OnStateChanged(session.State);
        }

        private void Unsubscribe()
        {
            if (session != null)
                session.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(SessionState state)
        {
            levelLabel.text = string.Format(levelFormat, session.Level);
            holdHint.SetActive(state == SessionState.Idle && !session.HasShot);

            var idle = state == SessionState.Idle;
            settingsButton.interactable = idle;
            shopButton.interactable = idle;
        }
    }
}
