using System;
using BallGame.Progression;
using BallGame.Save;

namespace BallGame.Skins
{
    public enum PurchaseResult
    {
        Purchased,
        AlreadyOwned,
        NotEnoughCoins,
        Unknown,
    }

    /// <summary>Owned and selected ball skins.</summary>
    public sealed class SkinService
    {
        private readonly SaveService save;
        private readonly SkinCatalog catalog;
        private readonly ProgressService progress;

        public event Action Changed;

        public SkinCatalog Catalog => catalog;
        public SkinDefinition Selected => catalog.Find(save.Data.selectedSkin);

        public SkinService(SaveService save, SkinCatalog catalog, ProgressService progress)
        {
            this.save = save;
            this.catalog = catalog;
            this.progress = progress;
            EnsureValidState();
        }

        public bool IsOwned(string id) => save.Data.ownedSkins.Contains(id);
        public bool IsSelected(string id) => save.Data.selectedSkin == id;

        public PurchaseResult Buy(string id)
        {
            var skin = catalog.Find(id);
            if (skin == null)
                return PurchaseResult.Unknown;
            if (IsOwned(id))
                return PurchaseResult.AlreadyOwned;
            if (!progress.TrySpend(skin.price))
                return PurchaseResult.NotEnoughCoins;

            save.Data.ownedSkins.Add(id);
            save.Data.selectedSkin = id;
            save.Write();
            Changed?.Invoke();
            return PurchaseResult.Purchased;
        }

        public bool Select(string id)
        {
            if (!IsOwned(id) || IsSelected(id))
                return false;

            save.Data.selectedSkin = id;
            save.Write();
            Changed?.Invoke();
            return true;
        }

        // The catalog may change between builds: the default skin is always owned, a removed skin falls back to it.
        private void EnsureValidState()
        {
            var data = save.Data;
            var changed = false;

            if (!data.ownedSkins.Contains(catalog.DefaultSkinId))
            {
                data.ownedSkins.Add(catalog.DefaultSkinId);
                changed = true;
            }

            if (catalog.Find(data.selectedSkin) == null || !IsOwned(data.selectedSkin))
            {
                data.selectedSkin = catalog.DefaultSkinId;
                changed = true;
            }

            if (changed)
                save.Write();
        }
    }
}
