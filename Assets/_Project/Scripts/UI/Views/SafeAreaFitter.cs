using UnityEngine;

namespace BallGame.UI
{
    /// <summary>Fits this RectTransform into Screen.safeArea (notches, rounded corners). Parent must be full-screen.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rect;
        private Rect applied;
        private Vector2Int screenSize;

        private void Awake() => rect = (RectTransform)transform;

        private void OnEnable() => Apply();

        private void Update()
        {
            if (applied != Screen.safeArea || screenSize.x != Screen.width || screenSize.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            applied = Screen.safeArea;
            screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize.x <= 0 || screenSize.y <= 0)
                return;

            rect.anchorMin = new Vector2(applied.xMin / screenSize.x, applied.yMin / screenSize.y);
            rect.anchorMax = new Vector2(applied.xMax / screenSize.x, applied.yMax / screenSize.y);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
