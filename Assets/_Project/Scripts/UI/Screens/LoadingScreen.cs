using TMPro;
using UnityEngine;

namespace BallGame.UI
{
    public sealed class LoadingScreen : GameScreen
    {
        [SerializeField] private ProgressBarView progressBar;
        [SerializeField] private TMP_Text percentLabel;

        public void SetProgress(float value)
        {
            progressBar.SetValue(value);
            percentLabel.SetText("{0}%", Mathf.RoundToInt(value * 100f));
        }

        protected override void OnBeforeOpen() => SetProgress(0f);
    }
}
