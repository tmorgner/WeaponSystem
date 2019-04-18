using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    /// <summary>
    ///  A weapon projectile that uses simulated kinematic movement without relying on the
    ///  physics system. This is slightly cheaper than using physical projectiles.
    /// </summary>
    public class ArtificialWeaponProjectile : WeaponProjectile
    {
        [SerializeField] bool applyGravity;
        [Tooltip("Controls whether the projectile will always face the current movement. Use this for bullets and arrows.")]
        [SerializeField] bool adjustRotationToForwardDirection = true;

        /// <summary>
        /// Controls whether the projectile will always face the current movement. Use this for bullets and arrows.
        /// </summary>
        protected bool AdjustRotationToForwardDirection => adjustRotationToForwardDirection;

        /// <inheritdoc />
        public override bool IsBallistic => applyGravity;

        /// <summary>
        ///   Returns the current position of the projectile based on the original position,
        ///   velocity, gravity and the time passed since launch.
        /// </summary>
        protected Vector3 CurrentOrigin
        {
            get
            {
                var timePassed = Time.inFixedTimeStep ? FixedUpdateTimer.TimePassed : Timer.TimePassed;
                var point = Origin + Direction * (timePassed * Velocity);
                if (applyGravity)
                {
                    point += (Physics.gravity * timePassed * timePassed) / 2;
                }

                return point;
            }
        }

        /// <summary>
        ///   Returns the next frame's position of the projectile based on the original position,
        ///   velocity, gravity and the time passed since launch. Note that if your framerate is
        ///   fluctuating wildly, this method may not return the correct result as it relies on
        ///   an estimated delta-time for its calculation.
        /// </summary>
        protected Vector3 FutureOrigin
        {
            get
            {
                float timeSoon;
                if (Time.inFixedTimeStep)
                {
                    timeSoon = FixedUpdateTimer.TimePassed + Time.fixedDeltaTime;
                }
                else
                {
                    timeSoon = Timer.TimePassed + Time.deltaTime;
                }

                var point2 = Origin + Direction * Velocity * (timeSoon);
                if (applyGravity)
                {
                    point2 += (Physics.gravity * timeSoon * timeSoon) / 2;
                }

                if (float.IsNaN(point2.x) || float.IsNaN(point2.y) || float.IsNaN(point2.z))
                {
                    Debug.Break();
                }
                return point2;
            }
        }

        /// <inheritdoc />
        protected override void OnUpdateFlyingOverride()
        {
            transform.position = CurrentOrigin;
            if (adjustRotationToForwardDirection)
            {
                transform.LookAt(FutureOrigin);
            }
        }

        /// <inheritdoc />
        protected override void FixedUpdateOverride()
        {
            var point1 = CurrentOrigin;
            var point2 = point1 + Direction * Velocity * Time.fixedDeltaTime;
            if (applyGravity)
            {
                var time = FixedUpdateTimer.TimePassed;
                var gravNow = Physics.gravity * time * time * 0.5f;
                var gravSoon = Physics.gravity * (time + Time.fixedDeltaTime) * (time + Time.fixedDeltaTime) * 0.5f;
                point2 += gravSoon - gravNow;
            }

            if (CheckCollision(point1, point2, out var hit))
            {
                PerformHit(hit);
            }
            else
            {
                FixedUpdateOverride(point2, point1);
            }
        }

        /// <summary>
        ///   An override point that provides the calculated next frame position as head and the
        ///   current position as tail.
        /// </summary>
        /// <param name="head">The predicted position during the next frame</param>
        /// <param name="tail">The current position.</param>
        protected virtual void FixedUpdateOverride(Vector3 head, Vector3 tail)
        {

        }
    }
}