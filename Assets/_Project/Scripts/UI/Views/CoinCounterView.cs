using BallGame.Core;
using TMPro;
using UnityEngine;

namespace BallGame.UI
{
    /// <summary>Shows the player's coins and follows changes while enabled.</summary>
    public sealed class CoinCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private void OnEnable()
        {
            var app = App.Instance;
            if (app == null)
                return;

            app.Progress.CoinsChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            var app = App.Instance;
            if (app != null)
                app.Progress.CoinsChanged -= Refresh;
        }

        private void Refresh() => label.SetText("{0}", App.Instance.Progress.Coins);
    }
}
