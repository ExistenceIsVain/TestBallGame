using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    /// <summary>Level complete: shows the reward. Argument = coins granted.</summary>
    public sealed class WinScreen : GameScreen, IAcceptArg<int>
    {
        [SerializeField] private TMP_Text rewardLabel;
        [SerializeField] private Button nextButton;

        public event Action NextClicked;

        public void AcceptArg(int reward) => rewardLabel.SetText("+{0}", reward);

        protected override void OnInitialize() => nextButton.onClick.AddListener(() => NextClicked?.Invoke());
    }
}
