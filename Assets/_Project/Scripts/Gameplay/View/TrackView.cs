using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>The path strip from the ball to the door. Its width equals the ball's diameter.</summary>
    public sealed class TrackView : MonoBehaviour
    {
        [Tooltip("Flat quad child, 1x1, lying on the ground and pointing along local Z.")]
        [SerializeField] private Transform strip;
        [SerializeField] private float height = 0.01f;

        public void SetSegment(Vector3 from, Vector3 to, float width)
        {
            var delta = to - from;
            delta.y = 0f;
            var length = delta.magnitude;

            if (length < 0.01f)
            {
                strip.gameObject.SetActive(false);
                return;
            }

            strip.gameObject.SetActive(true);
            transform.SetPositionAndRotation((from + to) * 0.5f + Vector3.up * height, Quaternion.LookRotation(delta));
            strip.localScale = new Vector3(width, length, 1f);
        }
    }
}
