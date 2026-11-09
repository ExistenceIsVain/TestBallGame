using System.Collections;
using BallGame.UI;
using ProjectUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BallGame.Core
{
    /// <summary>Start scene: shows the loading screen for a fixed time while the game scene loads in the background.</summary>
    public sealed class StartSceneEntry : MonoBehaviour
    {
        [SerializeField] private ScreenManager screens;
        [SerializeField] private float fakeLoadDuration = 2f;

        private IEnumerator Start()
        {
            screens.OpenScreen<LoadingScreen>();
            var loadingScreen = screens.GetScreen<LoadingScreen>();

            var operation = SceneManager.LoadSceneAsync(SceneNames.Game);
            operation.allowSceneActivation = false;

            for (var time = 0f; ; time += Time.unscaledDeltaTime)
            {
                // Unity reports 0.9 when a scene is loaded and waits for activation.
                var realProgress = operation.progress / 0.9f;
                var shown = Mathf.Min(time / fakeLoadDuration, realProgress);
                loadingScreen.SetProgress(shown);

                if (time >= fakeLoadDuration && realProgress >= 1f)
                    break;

                yield return null;
            }

            loadingScreen.SetProgress(1f);
            operation.allowSceneActivation = true;
        }
    }
}
