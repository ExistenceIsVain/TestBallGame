using System;
using System.Collections;
using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>Level exit. Opens by itself when the watched ball comes within the open distance.</summary>
    public sealed class Door : MonoBehaviour
    {
        [Tooltip("Hinge transforms: the left one swings by -angle, the right one by +angle.")]
        [SerializeField] private Transform leftLeaf;
        [SerializeField] private Transform rightLeaf;
        [SerializeField] private float openAngle = 100f;
        [SerializeField] private float openDuration = 0.45f;
        [Tooltip("Burst of confetti fired when the door opens.")]
        [SerializeField] private ParticleSystem confetti;

        private Transform watched;
        private float openDistanceSqr;

        public bool IsOpen { get; private set; }

        public event Action Opened;

        public void Setup(Vector3 position, Vector2 direction, Transform target, float openDistance)
        {
            StopAllCoroutines();
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(BallMath.World(direction)));
            watched = target;
            openDistanceSqr = openDistance * openDistance;
            IsOpen = false;
            SetLeaves(0f);
            confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Update()
        {
            if (IsOpen || watched == null)
                return;

            var offset = watched.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= openDistanceSqr)
                Open();
        }

        private void Open()
        {
            IsOpen = true;
            confetti.Play();
            Opened?.Invoke();
            StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            for (var t = 0f; t < openDuration; t += Time.deltaTime)
            {
                var k = 1f - (1f - t / openDuration) * (1f - t / openDuration); // ease out
                SetLeaves(k);
                yield return null;
            }

            SetLeaves(1f);
        }

        private void SetLeaves(float openness)
        {
            leftLeaf.localRotation = Quaternion.Euler(0f, -openAngle * openness, 0f);
            rightLeaf.localRotation = Quaternion.Euler(0f, openAngle * openness, 0f);
        }
    }
}
