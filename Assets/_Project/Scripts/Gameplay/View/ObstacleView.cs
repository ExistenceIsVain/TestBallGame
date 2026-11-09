using System;
using System.Collections;
using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>Visual of one obstacle. Pooled by <see cref="ObstacleSpawner"/>; the mesh comes from the level theme.</summary>
    public sealed class ObstacleView : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private Material infectedMaterial;
        [SerializeField] private float swellScale = 1.3f;

        private Material normalMaterial;
        private Vector3 baseScale;

        public void SetLook(Mesh mesh, Vector3 meshScale, Material material)
        {
            meshFilter.sharedMesh = mesh;
            meshFilter.transform.localScale = meshScale;
            normalMaterial = material;
            meshRenderer.sharedMaterial = material;
        }

        public void Place(Vector3 position, float yaw, float scale)
        {
            StopAllCoroutines();
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            baseScale = Vector3.one * scale;
            transform.localScale = baseScale;
            meshRenderer.sharedMaterial = normalMaterial;
            gameObject.SetActive(true);
        }

        public void Infect(float delay, float explodeDelay, Action<ObstacleView> exploded)
        {
            StartCoroutine(InfectRoutine(delay, explodeDelay, exploded));
        }

        private IEnumerator InfectRoutine(float delay, float explodeDelay, Action<ObstacleView> exploded)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            meshRenderer.sharedMaterial = infectedMaterial;

            // Swell up before popping.
            for (var t = 0f; t < explodeDelay; t += Time.deltaTime)
            {
                var k = t / explodeDelay;
                transform.localScale = baseScale * (1f + (swellScale - 1f) * k * k);
                yield return null;
            }

            exploded?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
