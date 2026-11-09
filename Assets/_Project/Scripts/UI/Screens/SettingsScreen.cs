using BallGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    public sealed class SettingsScreen : GameScreen
    {
        [SerializeField] private OnOffToggleView music;
        [SerializeField] private OnOffToggleView sound;
        [SerializeField] private OnOffToggleView vibration;
        [SerializeField] private Button closeButton;

        private SettingsService settings;

        protected override void OnInitialize()
        {
            settings = App.Instance.Settings;
            music.Clicked += () => settings.SetMusic(!settings.MusicOn);
            sound.Clicked += () => settings.SetSound(!settings.SoundOn);
            vibration.Clicked += () => settings.SetVibration(!settings.VibrationOn);
            closeButton.onClick.AddListener(() => Manager.CloseScreen<SettingsScreen>());
        }

        protected override void OnBeforeOpen()
        {
            settings.Changed += Refresh;
            Refresh();
        }

        protected override void OnAfterClose() => Unsubscribe();
        protected override void OnScreenDestroyed() => Unsubscribe();

        private void Unsubscribe()
        {
            if (settings != null)
                settings.Changed -= Refresh;
        }

        private void Refresh()
        {
            music.SetState(settings.MusicOn);
            sound.SetState(settings.SoundOn);
            vibration.SetState(settings.VibrationOn);
        }
    }
}
