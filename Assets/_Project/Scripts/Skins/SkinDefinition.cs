using System;
using UnityEngine;

namespace BallGame.Skins
{
    [Serializable]
    public sealed class SkinDefinition
    {
        [Tooltip("Stable id stored in the save. Never rename after release.")]
        public string id;
        public string displayName;
        [Min(0)] public int price;
        [Tooltip("Ball and shot material (patterned texture).")]
        public Material material;
        [Tooltip("Shop icon: the ball drawn with its pattern.")]
        public Sprite icon;
        [Tooltip("Dominant colour of the pattern: used for effects like the ball bursting.")]
        public Color mainColor = Color.white;
    }
}
