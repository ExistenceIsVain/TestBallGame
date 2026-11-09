using System;
using BallGame.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    /// <summary>Level failed: explains why. Argument = reason.</summary>
    public sealed class LoseScreen : GameScreen, IAcceptArg<LoseReason>
    {
        [SerializeField] private TMP_Text reasonLabel;
        [SerializeField] private string overchargedText = "You held too long:\nthe whole ball went into the shot!";
        [SerializeField] private string outOfSizeText = "Out of size:\nthe ball is too small to clear the path.";
        [SerializeField] private Button retryButton;

        public event Action RetryClicked;

        public void AcceptArg(LoseReason reason)
        {
            reasonLabel.text = reason == LoseReason.Overcharged ? overchargedText : outOfSizeText;
        }

        protected override void OnInitialize() => retryButton.onClick.AddListener(() => RetryClicked?.Invoke());
    }
}
