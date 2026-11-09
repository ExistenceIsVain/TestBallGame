using System;
using BallGame.Skins;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    /// <summary>One skin in the shop grid.</summary>
    public sealed class SkinCellView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Image preview;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private GameObject priceGroup;
        [SerializeField] private TMP_Text priceLabel;
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color expensiveColor = new(1f, 0.45f, 0.45f);
        [SerializeField] private TMP_Text stateLabel;
        [SerializeField] private string selectText = "SELECT";
        [SerializeField] private string selectedText = "EQUIPPED";

        private SkinDefinition skin;

        public event Action<SkinDefinition> Clicked;

        private void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(skin));

        public void Bind(SkinDefinition definition)
        {
            skin = definition;
            preview.sprite = definition.icon;
            nameLabel.text = definition.displayName;
            priceLabel.SetText("{0}", definition.price);
        }

        public void Refresh(bool owned, bool selected, bool affordable)
        {
            background.sprite = selected ? selectedSprite : normalSprite;
            priceGroup.SetActive(!owned);
            priceLabel.color = affordable ? affordableColor : expensiveColor;
            stateLabel.gameObject.SetActive(owned);
            stateLabel.text = selected ? selectedText : selectText;
        }
    }
}
