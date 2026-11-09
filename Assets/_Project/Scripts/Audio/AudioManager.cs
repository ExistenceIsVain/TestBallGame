using System;
using UnityEngine;

namespace BallGame.Audio
{
    /// <summary>Music playlist + a small round-robin pool of 2D sound voices.</summary>
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;

        private AudioConfig config;
        private AudioSource[] voices = Array.Empty<AudioSource>();
        private AudioConfig.SoundEntry[] entries;
        private bool[] hasEntry;
        private float[] lastPlayTime;
        private int nextVoice;
        private int trackIndex = -1;

        private bool soundOn = true;
        private bool musicOn = true;

        public void Initialize(AudioConfig audioConfig)
        {
            config = audioConfig;

            var count = Enum.GetValues(typeof(SoundId)).Length;
            entries = new AudioConfig.SoundEntry[count];
            hasEntry = new bool[count];
            lastPlayTime = new float[count];

            foreach (var entry in config.sounds)
            {
                var index = (int)entry.id;
                entries[index] = entry;
                hasEntry[index] = entry.clips != null && entry.clips.Length > 0;
                lastPlayTime[index] = float.NegativeInfinity;
            }

            voices = new AudioSource[config.voices];
            for (int i = 0; i < voices.Length; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                voices[i] = source;
            }

            musicSource.loop = false;
            musicSource.playOnAwake = false;
            musicSource.volume = config.musicVolume;
        }

        public void Play(SoundId id, float pitch = 1f)
        {
            if (!soundOn || config == null)
                return;

            var index = (int)id;
            if (!hasEntry[index])
                return;

            var entry = entries[index];
            var now = Time.unscaledTime;
            if (now - lastPlayTime[index] < entry.cooldown)
                return;
            lastPlayTime[index] = now;

            var voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;

            voice.clip = entry.clips[UnityEngine.Random.Range(0, entry.clips.Length)];
            voice.volume = entry.volume;
            voice.pitch = pitch * (1f + UnityEngine.Random.Range(-entry.pitchJitter, entry.pitchJitter));
            voice.Play();
        }

        public void SetSoundEnabled(bool enabled)
        {
            soundOn = enabled;
            if (enabled)
                return;

            foreach (var voice in voices)
                voice.Stop();
        }

        public void SetMusicEnabled(bool enabled)
        {
            musicOn = enabled;

            if (!enabled)
                musicSource.Stop();
            else if (!musicSource.isPlaying)
                PlayNextTrack();
        }

        private void Update()
        {
            // Playlist: start the next track when the current one ends (not while the app is in background).
            if (musicOn && config != null && !musicSource.isPlaying && Application.isFocused)
                PlayNextTrack();
        }

        private void PlayNextTrack()
        {
            var tracks = config.musicTracks;
            if (tracks == null || tracks.Length == 0)
                return;

            trackIndex = (trackIndex + 1) % tracks.Length;
            musicSource.clip = tracks[trackIndex];
            musicSource.Play();
        }
    }
}
