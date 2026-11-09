using UnityEngine;

namespace BallGame.UI
{
    /// <summary>Horizontal bar: the fill is stretched by its right anchor, so a sliced sprite keeps its rounded ends.</summary>
    public sealed class ProgressBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform fill;

        public void SetValue(float value)
        {
            var max = fill.anchorMax;
            max.x = Mathf.Clamp01(value);
            fill.anchorMax = max;
        }
    }
}
