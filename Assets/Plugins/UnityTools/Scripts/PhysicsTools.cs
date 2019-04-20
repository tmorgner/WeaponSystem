using System;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public static class PhysicsTools
    {

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
            var s1 = (-b + sqrt) / (2 * a);
            var s2 = (-b - sqrt) / (2 * a);
            return Math.Min(s1, s2);
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
