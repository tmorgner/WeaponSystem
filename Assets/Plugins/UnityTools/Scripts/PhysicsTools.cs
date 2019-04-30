using System;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public static class PhysicsTools
    {
        /// <summary>
        /// </summary>
        /// <param name="sphereOrigin"></param>
        /// <param name="radius"></param>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDirection"></param>
        /// <returns></returns>
        /// <remarks>
        ///  Based on
        ///  https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
        /// </remarks>
        public static float FindSphereIntersection(Vector3 sphereOrigin,
                                                   float radius,
                                                   Vector3 rayOrigin,
                                                   Vector3 rayDirection)
        {
            var originToCenter = rayOrigin - sphereOrigin;
            var a = Vector3.Dot(rayDirection, rayDirection);
            var b = 2 * Vector3.Dot(originToCenter, rayDirection);
            var c = Vector3.Dot(originToCenter, originToCenter) - radius * radius;
            var discr = b * b - 4 * a * c;
            if (discr < 0)
            {
                return -1;
            }
            var sqrt = Mathf.Sqrt(discr);
            var q = (b > 0) ? (b + sqrt) : (b - sqrt);
            q *= -0.5f;

            var x0 = q / a;
            var x1 = c / q;

            var t0 = Mathf.Min(x0, x1);
            var t1 = Mathf.Max(x0, x1);
            if (t0 < 0)
            {
                t0 = t1;
                if (t0 < 0)
                {
                    return -1;
                }
            }

            return t0;
        }


        public static void AccelerateTo(this Rigidbody2D body,
                                    float targetSpeedMeterPerSeconds,
                                    Vector2 direction, float deltaTime = 1)
        {
            if (deltaTime <= 0)
            {
                return;
            }

            direction.Normalize();
            var force = targetSpeedMeterPerSeconds * body.mass / deltaTime;
            body.AddForce(direction * force);
        }

        // https://answers.unity.com/questions/652010/how-drag-is-calculated-by-unity-engine.html
        public static float MaximumVelocity(this Rigidbody2D body, float accelerationForce, float timeStep)
        {
            return accelerationForce / body.drag - accelerationForce * timeStep;
        }
    }
}
