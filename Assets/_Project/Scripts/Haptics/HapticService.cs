using UnityEngine;

namespace BallGame.Haptics
{
    /// <summary>
    /// Thin wrapper over the Vibration plugin (github.com/BenoitFreslon/Vibration).
    /// The plugin assembly exists only for mobile platforms, so calls are compiled for device builds only.
    /// </summary>
    public sealed class HapticService
    {
        private const float MinInterval = 0.06f;

        private float lastTime = float.NegativeInfinity;

        public bool Enabled { get; set; } = true;

        public HapticService()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Vibration.Init();
#endif
        }

        /// <summary>Tiny tick: UI, small hits.</summary>
        public void Light()
        {
            if (!CanVibrate()) return;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Vibration.VibratePop();
#endif
        }

        /// <summary>Noticeable bump: shot impact, door.</summary>
        public void Medium()
        {
            if (!CanVibrate()) return;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Vibration.VibratePeek();
#endif
        }

        /// <summary>Full vibration: win / lose.</summary>
        public void Heavy()
        {
            if (!CanVibrate()) return;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Vibration.Vibrate();
#endif
        }

        private bool CanVibrate()
        {
            if (!Enabled)
                return false;

            var now = Time.unscaledTime;
            if (now - lastTime < MinInterval)
                return false;

            lastTime = now;
            return true;
        }
    }
}
