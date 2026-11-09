using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>The player's ball. Root sits on the ground; the body child is a unit sphere scaled to the radius.</summary>
    public sealed class PlayerBall : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Transform shadow;

        private float hopHeight;

        public float Radius { get; private set; }
        public Material Skin => bodyRenderer.sharedMaterial;

        public void ResetBall(Vector3 groundPosition, float radius)
        {
            transform.position = groundPosition;
            body.localRotation = Quaternion.identity;
            hopHeight = 0f;
            body.gameObject.SetActive(true);
            shadow.gameObject.SetActive(true);
            SetRadius(radius);
        }

        public void SetRadius(float radius)
        {
            Radius = radius;
            body.localScale = Vector3.one * (radius * 2f);
            ApplyHeight();
        }

        public void SetGroundPosition(Vector3 position) => transform.position = position;

        /// <summary>Spins the body as if it rolled over <paramref name="delta"/>, so the skin pattern shows the motion.</summary>
        public void Roll(Vector3 delta)
        {
            var distance = delta.magnitude;
            if (distance < 1e-5f || Radius <= 0f)
                return;

            var axis = Vector3.Cross(Vector3.up, delta / distance);
            body.rotation = Quaternion.AngleAxis(distance / Radius * Mathf.Rad2Deg, axis) * body.rotation;
        }

        public void SetHopHeight(float height)
        {
            hopHeight = height;
            ApplyHeight();
        }

        public void SetSkin(Material material) => bodyRenderer.sharedMaterial = material;

        public void Hide()
        {
            body.gameObject.SetActive(false);
            shadow.gameObject.SetActive(false);
        }

        private void ApplyHeight()
        {
            body.localPosition = new Vector3(0f, Radius + hopHeight, 0f);

            // Blob shadow: shrinks a little while the ball is in the air.
            var shadowScale = Radius * 2.2f / (1f + hopHeight);
            shadow.localScale = new Vector3(shadowScale, shadowScale, 1f);
        }
    }
}
