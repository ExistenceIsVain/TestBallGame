using System.Collections;
using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>The shot ball: grows in front of the player while charging, then flies to the impact point.</summary>
    public sealed class ShotProjectile : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Renderer bodyRenderer;

        public float Radius { get; private set; }

        public void Show(Material material)
        {
            bodyRenderer.sharedMaterial = material;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        public void Place(Vector3 groundPosition, float radius)
        {
            Radius = radius;
            transform.position = groundPosition;
            body.localScale = Vector3.one * (radius * 2f);
            body.localPosition = new Vector3(0f, radius, 0f);
        }

        public IEnumerator FlyTo(Vector3 groundTarget, float speed)
        {
            var start = transform.position;
            var duration = Vector3.Distance(start, groundTarget) / speed;

            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(start, groundTarget, t / duration);
                yield return null;
            }

            transform.position = groundTarget;
        }
    }
}
