using System.Collections.Generic;
using UnityEngine;

namespace BallGame.Skins
{
    [CreateAssetMenu(menuName = "BallGame/Skin Catalog", fileName = "SkinCatalog")]
    public sealed class SkinCatalog : ScriptableObject
    {
        [SerializeField] private string defaultSkinId = "orange";
        [SerializeField] private List<SkinDefinition> skins = new();

        public IReadOnlyList<SkinDefinition> Skins => skins;
        public string DefaultSkinId => defaultSkinId;

        public SkinDefinition Find(string id)
        {
            foreach (var skin in skins)
            {
                if (skin.id == id)
                    return skin;
            }

            return null;
        }
    }
}
