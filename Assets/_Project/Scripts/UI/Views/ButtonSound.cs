using BallGame.Audio;
using BallGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    /// <summary>Plays a UI sound when the button on the same object is clicked.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonSound : MonoBehaviour
    {
        [SerializeField] private SoundId sound = SoundId.Click;

        private void Awake() => GetComponent<Button>().onClick.AddListener(Play);

        private void Play()
        {
            var app = App.Instance;
            if (app == null)
                return;

            app.Audio.Play(sound);
            app.Haptics.Light();
        }
    }
}
