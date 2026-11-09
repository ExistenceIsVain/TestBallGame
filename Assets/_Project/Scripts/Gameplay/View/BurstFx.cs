using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>
    /// One shared particle system per effect type (explosions, dust): Emit at a position instead of spawning effect objects.
    /// </summary>
    public sealed class BurstFx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private int particlesPerBurst = 10;

        public void Burst(Vector3 position, Color color, float size = 1f)
        {
            // A stopped system does not update emitted particles; its emission module is off, so Play adds nothing itself.
            if (!particles.isPlaying)
                particles.Play();

            var emit = new ParticleSystem.EmitParams
            {
                position = position,
                applyShapeToPosition = true,
                startColor = color,
                startSize = particles.main.startSize.constant * size,
            };
            particles.Emit(emit, particlesPerBurst);
        }
    }
}
