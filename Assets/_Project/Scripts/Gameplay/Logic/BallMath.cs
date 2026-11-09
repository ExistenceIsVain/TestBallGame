using UnityEngine;

namespace BallGame.Gameplay
{
    /// <summary>
    /// Size math. Volume is r^3 (the 4/3*PI factor cancels out everywhere), so moving volume
    /// from the ball into the shot keeps the total amount of "ball" constant.
    /// </summary>
    public static class BallMath
    {
        public static float Volume(float radius) => radius * radius * radius;

        public static float Radius(float volume) => volume <= 0f ? 0f : Mathf.Pow(volume, 1f / 3f);

        public static Vector2 Flat(Vector3 v) => new(v.x, v.z);

        public static Vector3 World(Vector2 v, float y = 0f) => new(v.x, y, v.y);

        /// <summary>Distance from the ball centre to the shot centre when the shot sits in front of the ball.</summary>
        public static float ShotStartDistance(float ballRadius, float shotRadius) => ballRadius + shotRadius + 0.05f;

        /// <summary>Shot centre at the moment it bursts: it flies straight along the path from the ball.</summary>
        public static Vector2 ExplosionPoint(Vector2 ball, Vector2 direction, float explosionDistance, float ballRadius, float shotRadius)
        {
            return ball + direction * Mathf.Max(explosionDistance, ShotStartDistance(ballRadius, shotRadius));
        }
    }
}
