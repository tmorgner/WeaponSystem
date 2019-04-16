using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    public class ArtificialWeaponProjectile : WeaponProjectile
    {
        [SerializeField] bool applyGravity;

        public override bool IsBallistic => applyGravity;

        public Vector3 CurrentOrigin
        {
            get
            {
                var timePassed = Time.inFixedTimeStep ? fixedUpdateTimer.TimePassed : timer.TimePassed;
                var point = Origin + Direction * (timePassed * Velocity);
                if (applyGravity)
                {
                    point += (Physics.gravity * timePassed * timePassed) / 2;
                }

                return point;
            }
        }

        public Vector3 FutureOrigin
        {
            get
            {
                float timeSoon;
                if (Time.inFixedTimeStep)
                {
                    timeSoon = fixedUpdateTimer.TimePassed + Time.fixedDeltaTime;
                }
                else
                {
                    timeSoon = timer.TimePassed + Time.deltaTime;
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

        protected override void OnUpdateFlying()
        {
            transform.position = CurrentOrigin;
            transform.LookAt(FutureOrigin);
        }

        protected override void OnFixedUpdate()
        {
            var point1 = CurrentOrigin;
            var point2 = point1 + Direction * Velocity * Time.fixedDeltaTime;
            if (applyGravity)
            {
                var time = fixedUpdateTimer.TimePassed;
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
                OnFixedUpdate(point2, point1);
            }
        }

        protected virtual void OnFixedUpdate(Vector3 head, Vector3 tail)
        {

        }
    }
}