using BallGame.Audio;
using BallGame.Haptics;
using BallGame.Progression;
using BallGame.Save;
using BallGame.Skins;
using UnityEngine;

namespace BallGame.Core
{
    /// <summary>
    /// Persistent composition root. Spawned from Resources before the first scene loads,
    /// so any scene (Start or Game) can be played directly in the editor.
    /// </summary>
    public sealed class App : MonoBehaviour
    {
        private const string PrefabPath = "App";

        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AudioConfig audioConfig;
        [SerializeField] private SkinCatalog skinCatalog;
        [SerializeField] private ProgressionConfig progressionConfig;
        [SerializeField] private int targetFrameRate = 60;

        public static App Instance { get; private set; }

        public SaveService Save { get; private set; }
        public SettingsService Settings { get; private set; }
        public ProgressService Progress { get; private set; }
        public SkinService Skins { get; private set; }
        public HapticService Haptics { get; private set; }
        public AudioManager Audio => audioManager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
                return;

            var prefab = Resources.Load<App>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"App prefab not found at Resources/{PrefabPath}.");
                return;
            }

            Instantiate(prefab).name = prefab.name;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ConfigureRuntime();

            Save = new SaveService();
            Save.Load();

            Haptics = new HapticService();
            audioManager.Initialize(audioConfig);
            Settings = new SettingsService(Save, audioManager, Haptics);
            Progress = new ProgressService(Save, progressionConfig);
            Skins = new SkinService(Save, skinCatalog, Progress);

            Settings.Apply();
        }

        private void ConfigureRuntime()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // The game resolves all collisions analytically, so physics never needs to step.
            Physics.simulationMode = SimulationMode.Script;
            Physics2D.simulationMode = SimulationMode2D.Script;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                Save?.Write();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
