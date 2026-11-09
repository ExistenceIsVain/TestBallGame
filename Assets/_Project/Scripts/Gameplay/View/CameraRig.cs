using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>
    /// Follows the ball (optionally a point ahead of it along the path) with a fixed angle.
    /// Pulls back on screens narrower than the reference aspect.
    /// </summary>
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [Tooltip("Camera position relative to the focus point.")]
        [SerializeField] private Vector3 offset = new(0f, 15f, -7f);
        [Tooltip("How far ahead of the ball along the path the camera looks.")]
        [SerializeField] private float lookAhead;
        [Tooltip("Share of the focus point's sideways movement the camera follows (0 = stays centred on the field).")]
        [Range(0f, 1f)] [SerializeField] private float followX = 0.35f;
        [SerializeField] private float smoothTime = 0.3f;
        [SerializeField] private float referenceAspect = 9f / 16f;

        private Transform target;
        private Vector3 forward;
        private Vector3 velocity;

        public Camera Camera => targetCamera;

        public void Follow(Transform followTarget, Vector3 pathDirection)
        {
            target = followTarget;
            forward = pathDirection;
            velocity = Vector3.zero;
            transform.position = DesiredPosition();
        }

        private void LateUpdate()
        {
            if (target != null)
                transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition(), ref velocity, smoothTime);
        }

        private Vector3 DesiredPosition()
        {
            var focus = target.position + forward * lookAhead;
            focus.x *= followX;
            focus.y = 0f;
            var zoom = Mathf.Max(1f, referenceAspect / targetCamera.aspect);
            return focus + offset * zoom;
        }
    }
}
