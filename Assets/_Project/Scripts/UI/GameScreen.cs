using System;
using System.Collections;
using ProjectUI;
using UnityEngine;

namespace BallGame.UI
{
    /// <summary>Project base screen: fades the screen's CanvasGroup in and out (unscaled time).</summary>
    public abstract class GameScreen : UIScreen
    {
        [SerializeField] private float fadeDuration = 0.15f;

        private Coroutine fade;

        protected override void PlayOpenTransition(Action onComplete) => StartFade(0f, 1f, onComplete);

        protected override void PlayCloseTransition(Action onComplete) => StartFade(1f, 0f, onComplete);

        protected override void StopTransition()
        {
            if (fade != null)
            {
                StopCoroutine(fade);
                fade = null;
            }

            CanvasGroup.alpha = State == ScreenState.Closing ? 0f : 1f;
        }

        private void StartFade(float from, float to, Action onComplete)
        {
            if (fade != null)
                StopCoroutine(fade);

            if (fadeDuration <= 0f || !isActiveAndEnabled)
            {
                CanvasGroup.alpha = to;
                onComplete();
                return;
            }

            fade = StartCoroutine(Fade(from, to, onComplete));
        }

        private IEnumerator Fade(float from, float to, Action onComplete)
        {
            for (var t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                CanvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }

            CanvasGroup.alpha = to;
            fade = null;
            onComplete();
        }
    }
}
