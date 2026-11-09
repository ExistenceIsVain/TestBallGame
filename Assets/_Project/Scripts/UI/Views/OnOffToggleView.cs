using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    /// <summary>A settings row button that shows ON / OFF with its own sprite.</summary>
    public sealed class OnOffToggleView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string onText = "ON";
        [SerializeField] private string offText = "OFF";

        public event Action Clicked;

        private void Awake() => button.onClick.AddListener(() => Clicked?.Invoke());

        public void SetState(bool on)
        {
            background.sprite = on ? onSprite : offSprite;
            label.text = on ? onText : offText;
        }
    }
}
