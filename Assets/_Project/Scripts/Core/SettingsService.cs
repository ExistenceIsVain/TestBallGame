using System;
using BallGame.Audio;
using BallGame.Haptics;
using BallGame.Save;

namespace BallGame.Core
{
    /// <summary>Player toggles (music, sound, vibration): stored in the save and applied to the services.</summary>
    public sealed class SettingsService
    {
        private readonly SaveService save;
        private readonly AudioManager audio;
        private readonly HapticService haptics;

        public event Action Changed;

        public bool MusicOn => save.Data.musicOn;
        public bool SoundOn => save.Data.soundOn;
        public bool VibrationOn => save.Data.vibrationOn;

        public SettingsService(SaveService save, AudioManager audio, HapticService haptics)
        {
            this.save = save;
            this.audio = audio;
            this.haptics = haptics;
        }

        public void Apply()
        {
            audio.SetMusicEnabled(MusicOn);
            audio.SetSoundEnabled(SoundOn);
            haptics.Enabled = VibrationOn;
        }

        public void SetMusic(bool on) => Set(ref save.Data.musicOn, on);
        public void SetSound(bool on) => Set(ref save.Data.soundOn, on);
        public void SetVibration(bool on) => Set(ref save.Data.vibrationOn, on);

        private void Set(ref bool field, bool value)
        {
            if (field == value)
                return;

            field = value;
            save.Write();
            Apply();
            Changed?.Invoke();
        }
    }
}
