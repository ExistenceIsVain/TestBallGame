using System;
using UnityEngine;

namespace BallGame.Audio
{
    [CreateAssetMenu(menuName = "BallGame/Audio Config", fileName = "AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [Serializable]
        public struct SoundEntry
        {
            public SoundId id;
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume;
            [Range(0f, 0.5f)] public float pitchJitter;
            [Tooltip("Minimum seconds between two plays of this sound (protects from spam).")]
            [Min(0f)] public float cooldown;
        }

        [Header("Music")]
        public AudioClip[] musicTracks;
        [Range(0f, 1f)] public float musicVolume = 0.4f;

        [Header("Sounds")]
        [Min(1)] public int voices = 8;
        public SoundEntry[] sounds;
    }
}
