using System;
using NaughtyAttributes;
using RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles;
using UnityEngine;
using UnityTools.SmartVars.RuntimeSets;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons
{
    [CreateAssetMenu(menuName = "GunShip/Weapons/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [SerializeField] WeaponProjectilePool rayPool;
        [SerializeField] WeaponProjectile projectile;
        [SerializeField] SmartRigidBodySet targetSet;
        [SerializeField] float projectileSpeed;
        [SerializeField] float fireDelay;
        [SerializeField] float fireCoolDown;
        [SerializeField] float trackingError;
        [SerializeField] float maximumTrackingRange;
        [SerializeField] float targetCone;
        [SerializeField] float projectileTTL;
        [SerializeField] LayerMask interactWith;

        [ShowNativeProperty]
        internal WeaponProjectile ProjectileUsed
        {
            get
            {
                if (rayPool != null)
                {
                    var used = rayPool.LocalProjectilePrefab;
                    if (used != null)
                    {
                        return used;
                    }
                }

                return projectile;
            }
        }

        WeaponProjectile effectiveProjectile;

        [ShowNativeProperty]
        public bool IsBallistic
        {
            get
            {
                if (ProjectileUsed == null)
                {
                    return false;
                }
                return ProjectileUsed.IsBallistic;
            }
        }

        [ShowNativeProperty]
        public float DamagePerSecond
        {
            get
            {
                if (effectiveProjectile == null)
                {
                    return 0;
                }

                var cooldowns = FireDelay + FireCoolDown;
                return effectiveProjectile.DamagePerHit / cooldowns;
            }
        }

        public WeaponProjectilePool RayPool => rayPool;

        public SmartRigidBodySet TargetSet => targetSet;

        public float ProjectileSpeed => Math.Max(0.1f, projectileSpeed);

        public float FireDelay => Math.Max(0.05f, fireDelay);

        public float FireCoolDown => Math.Max(0.05f, fireCoolDown);

        public float TrackingError => trackingError;

        public float MaximumTrackingRange => maximumTrackingRange;

        public float Range => ProjectileTTL * ProjectileSpeed;

        public float TargetCone => targetCone;

        public float ProjectileTTL => projectileTTL <= 0 ? 10 : projectileTTL;

        public LayerMask InteractWith => interactWith;

        void OnEnable()
        {
            if (rayPool != null)
            {
                effectiveProjectile = rayPool.Initialize(this);
            }
        }

        void OnDisable()
        {
            effectiveProjectile = null;
        }

        public Vector3 RandomizeTarget(Transform source, Vector3 target)
        {
            var trackingError = TrackingError;
            var maximumTrackingRange = MaximumTrackingRange;

            var wobbleFactor = trackingError * (Vector3.Distance(source.position, target) / maximumTrackingRange);
            var offset = UnityEngine.Random.insideUnitSphere * wobbleFactor;
            var fireTarget = offset + target;

            return fireTarget;
        }

        public bool CalculateFiringSolution(Vector3 start, Vector3 target, out Vector3 result)
        {
            if (!RayPool.IsBallistic)
            {
                result = Vector3.Normalize(target - start);
                return true;
            }

            var s = ProjectileSpeed;
            var delta = target - start;
            float a = Vector3.Dot(Physics.gravity, Physics.gravity);
            float b = -4 * (Vector3.Dot(Physics.gravity, delta) + s * s);
            float c = 4 * Vector3.Dot(delta, delta);

            if (4 * a * c > b * b)
            {
                result = default;
                return false;
            }

            var term1 = Mathf.Sqrt(b * b - 4 * a * c);
            var time1 = Mathf.Sqrt((-b + term1) / (2 * a));
            var time2 = Mathf.Sqrt((-b - term1) / (2 * a));
            if (float.IsNaN(time1))
            {
                Debug.Log("Time 1 invalid");
                time1 = float.MaxValue;
            }

            if (float.IsNaN(time2))
            {
                Debug.Log("Time 2 invalid");
                time2 = float.MaxValue;
            }

            float time;
            if (time1 < 0)
            {
                if (time2 < 0)
                {
                    result = default;
                    return false;
                }
                else
                {
                    time = time2;
                }
            }
            else
            {
                if (time2 < 0)
                {
                    time = time1;
                }
                else
                {
                    time = Mathf.Min(time1, time2);
                }
            }

            result = (2 * delta - Physics.gravity * time * time) / (2 * s * time);
            return true;
        }
    }
}
