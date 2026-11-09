using System.Collections.Generic;
using BallGame.Audio;
using BallGame.Core;
using BallGame.Skins;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    /// <summary>Ball skins: buy with coins or equip an owned one.</summary>
    public sealed class ShopScreen : GameScreen
    {
        [SerializeField] private SkinCellView cellPrefab;
        [SerializeField] private Transform grid;
        [SerializeField] private Button closeButton;

        private readonly List<SkinCellView> cells = new();
        private App app;

        protected override void OnInitialize()
        {
            app = App.Instance;
            closeButton.onClick.AddListener(() => Manager.CloseScreen<ShopScreen>());

            // The catalog size is data-driven, so cells are created once from the prefab.
            foreach (var skin in app.Skins.Catalog.Skins)
            {
                var cell = Instantiate(cellPrefab, grid);
                cell.Bind(skin);
                cell.Clicked += OnCellClicked;
                cells.Add(cell);
            }
        }

        protected override void OnBeforeOpen()
        {
            app.Skins.Changed += Refresh;
            app.Progress.CoinsChanged += Refresh;
            Refresh();
        }

        protected override void OnAfterClose() => Unsubscribe();
        protected override void OnScreenDestroyed() => Unsubscribe();

        private void Unsubscribe()
        {
            if (app == null)
                return;

            app.Skins.Changed -= Refresh;
            app.Progress.CoinsChanged -= Refresh;
        }

        private void Refresh()
        {
            var skins = app.Skins;
            var catalog = skins.Catalog.Skins;

            for (int i = 0; i < cells.Count; i++)
            {
                var skin = catalog[i];
                cells[i].Refresh(skins.IsOwned(skin.id), skins.IsSelected(skin.id), app.Progress.CanAfford(skin.price));
            }
        }

        private void OnCellClicked(SkinDefinition skin)
        {
            if (app.Skins.IsOwned(skin.id))
            {
                app.Skins.Select(skin.id);
                app.Audio.Play(SoundId.Toggle);
                return;
            }

            if (app.Skins.Buy(skin.id) == PurchaseResult.Purchased)
            {
                app.Audio.Play(SoundId.Purchase);
                app.Haptics.Medium();
            }
            else
            {
                app.Audio.Play(SoundId.Error);
            }
        }
    }
}
