using System;
using NaughtyAttributes;
using RabbitStewdio.Unity.UnityTools.SmartVars.RuntimeSets;
using RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons
{
    /// <summary>
    ///     A weapon definition collects all parameter for a given gun type in a
    ///     central location. 
    /// </summary>
    [CreateAssetMenu(menuName = "Weapons/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [BoxGroup("Overrides")]
        [Tooltip("A projectile pool. If empty, a new pool with default settings will be created on demand.")]
        [SerializeField]
        WeaponProjectilePool rayPool;
        
        [BoxGroup("Overrides")]
        [Tooltip("The projectile used in the pool. If the pool already defined a projectile, this setting will be ignored.")]
        [SerializeField]
        WeaponProjectile projectile;

        [BoxGroup("Targeting")]
        [Required]
        [Tooltip("The set of target elements this gun will shoot at.")]
        [SerializeField]
        SmartRigidBodySet targetSet;

        [BoxGroup("Targeting")]
        [Tooltip("The effective tracking error at maximum range. The projectile will hit an random point within a radius around the actual target point. Set to zero for perfect accuracy.")]
        [SerializeField] float trackingError;
        
        [BoxGroup("Targeting")]
        [Tooltip("The maximum distance within the gun will acquire targets.")]
        [SerializeField] float maximumTrackingRange;

        [BoxGroup("Targeting")]
        [Tooltip("The gun's field of view for tracking targets.")]
        [SerializeField] float targetCone;

        [BoxGroup("Targeting")]
        [Tooltip("The time-to-live for projectiles in seconds. Projectiles that haven't hit anything after that time will be reclaimed for the pool.")]
        [SerializeField] float projectileTTL;

        [BoxGroup("Targeting")]
        [Tooltip("A layer mask indicating all layers a projectile can collide with. If set to None, this value will be automatically computed.")]
        [SerializeField] LayerMask interactWith;

        [BoxGroup("Targeting")]
        [Tooltip("[Debug]: This field cannot be edited.")]
        [SerializeField] LayerMask effectiveInteractWith;

        [BoxGroup("Launch Properties")]
        [Tooltip("The projectile speed of this gun. This is used in targeting calculations and to launch projectiles.")]
        [SerializeField] float projectileSpeed;

        [BoxGroup("Launch Properties")]
        [Tooltip("A delay in seconds between acquiring a target and launching a projectile. Use this to produce weapon charging effects.")]
        [SerializeField] float fireDelay;

        [BoxGroup("Launch Properties")]
        [Tooltip("A delay in seconds after a projectile has launched and an attempt to acquire a new target. Use  this to produce a weapon cool-down effect.")]
        [SerializeField] float fireCoolDown;

        [BoxGroup("Launch Properties")]
        [Tooltip("If there are two firing path, should the system choose the lower or higher one?")]
        [SerializeField] bool useHighPath;

        WeaponProjectilePool effectiveRayPool;
        bool autoCreatedPool;
        WeaponProjectile effectiveProjectile;

        /// <summary>
        ///    Computes the effective projectile prefab by taking both the override on this class and
        ///    the pool's defined prefab into account. This method is internal so that NaughtyAttributes
        ///    can display it in the UI. Normal user code should always acquire projectile instances
        ///    from the pool.
        /// </summary>
        [ShowNativeProperty]
        internal WeaponProjectile ProjectileUsed
        {
            get
            {
                if (effectiveRayPool != null)
                {
                    var used = effectiveRayPool.LocalProjectilePrefab;
                    if (used != null)
                    {
                        return used;
                    }
                }

                return projectile;
            }
        }

        /// <summary>
        ///   Checks whether the projectile is ballistic. Ballistic projectiles are
        ///   affected by gravity and need more complex targeting and firing calculations.
        /// </summary>
        [ShowNativeProperty]
        public bool IsBallistic
        {
            get
            {
                var projectileUsed = ProjectileUsed;
                if (projectileUsed == null)
                {
                    return false;
                }

                return projectileUsed.IsBallistic;
            }
        }

        /// <summary>
        ///   Computes the effective damage per second under the assumption that every
        ///   projectile hits its target.
        /// </summary>
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

        /// <summary>
        ///   The projectile pool used for this weapon. All weapon instances using the same
        ///   weapon definition in a scene share the same pool. If you want to reuse the
        ///   same projectile and pool across multiple weapon definitions, create a weapon
        ///   pool object manually and assign it to the RayPool field in the inspector.
        /// </summary>
        public WeaponProjectilePool RayPool => effectiveRayPool;

        /// <summary>
        ///   A dynamic set of targets used for the weapon.
        /// </summary>
        public SmartRigidBodySet TargetSet => targetSet;

        /// <summary>
        ///   The default projectile speed after launch.
        /// </summary>
        public float ProjectileSpeed => Math.Max(0.1f, projectileSpeed);

        /// <summary>
        ///   The default fire delay in seconds between acquiring a target and actually firing the gun.
        /// </summary>
        public float FireDelay => Math.Max(0.05f, fireDelay);

        /// <summary>
        ///    The default cooldown in seconds after firing the gun.
        /// </summary>
        public float FireCoolDown => Math.Max(0.05f, fireCoolDown);

        /// <summary>
        ///    The default tracking error used by the projectile launch code. This error is expressed
        ///    as meters at maximum tracking range.
        /// </summary>
        public float TrackingError => trackingError;

        /// <summary>
        ///    The maximum tracking range for target acquisition.
        /// </summary>
        public float MaximumTrackingRange => maximumTrackingRange;

        /// <summary>
        ///   The maximum range for this gun. This should be larger than the weapon's tracking range so
        ///   that you can shoot fleeing targets.
        /// </summary>
        [ShowNativeProperty]
        public float Range => ProjectileTTL * ProjectileSpeed;

        /// <summary>
        ///  The turrets field of view for acquiring targets.
        /// </summary>
        public float TargetCone => targetCone;

        /// <summary>
        ///   The projectiles time to live before they get reclaimed to the pool.
        /// </summary>
        public float ProjectileTTL => projectileTTL <= 0 ? 10 : projectileTTL;

        /// <summary>
        ///   The gun's effective layer-mask for testing whether a target can be hit.
        ///   Usually that corresponds to the projectile's collision mask. If the
        ///   serialized property <see cref="interactWith"/> is set to Nothing this
        ///   value is automatically computed from the physics system.
        /// </summary>
        public LayerMask InteractWith => effectiveInteractWith;

        void OnEnable()
        {
            if (rayPool != null)
            {
                effectiveRayPool = rayPool;
            }
            else
            {
                effectiveRayPool = null;
            }

            if (effectiveRayPool == null)
            {
                effectiveRayPool = CreateInstance<WeaponProjectilePool>();
                autoCreatedPool = true;
            }
            else
            {
                autoCreatedPool = false;
            }

            effectiveProjectile = effectiveRayPool.Initialize(this, projectile);
             
            if (interactWith == 0)
            {
                if (effectiveProjectile == null)
                {
                    effectiveInteractWith = 0;
                } 
                else
                {
                    effectiveInteractWith = 0;
                    var projectileLayer = effectiveProjectile.gameObject.layer;
                    for (var layer = 0; layer < 32; layer += 1)
                    {
                        if (!Physics.GetIgnoreLayerCollision(projectileLayer, layer))
                        {
                            effectiveInteractWith |= 1 << layer;
                        }
                    }
                }
            }
            else
            {
                effectiveInteractWith = interactWith;
            }
        }

        void OnDisable()
        {
            if (autoCreatedPool)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(effectiveRayPool);
                }
                else
                {
                    Destroy(effectiveRayPool);
                }
            }

            effectiveProjectile = null;
        }

        /// <summary>
        ///   Tries to retrieve a new projectile instance from the associated projectile pool.
        ///   This is a convenience method for working with the pool.
        /// </summary>
        /// <param name="projectile">the acquired projectile or null</param>
        /// <returns>true if the pool provided an valid projectile, false otherwise.</returns>
        public bool TryGetProjectile(out WeaponProjectile projectile)
        {
            if (effectiveRayPool == null)
            {
                projectile = default;
                return false;
            }

            return effectiveRayPool.TryGet(out projectile);
        }

        /// <summary>
        ///   Randomizes the target position based on the accuracy settings defined in this class.
        ///   The resulting vector will be somewhere near the target point.
        /// </summary>
        /// <param name="source">The gun that shoots the projectile.</param>
        /// <param name="target">The target the gun aims at.</param>
        /// <returns>the modified target point.</returns>
        public Vector3 RandomizeTarget(Transform source, Vector3 target)
        {
            var trackingError = TrackingError;
            var maximumTrackingRange = MaximumTrackingRange;

            var wobbleFactor = trackingError * (Vector3.Distance(source.position, target) / maximumTrackingRange);
            var offset = UnityEngine.Random.insideUnitSphere * wobbleFactor;
            var fireTarget = offset + target;

            return fireTarget;
        }

        /// <summary>
        ///   A helper method that calculates a firing vector to hit the given target position
        ///   with a projectile launched from start. This calculation can fail for ballistic guns
        ///   if the target is too far away for the projectile to reach the target before being
        ///   caught by gravity effects.
        /// </summary>
        /// <remarks>
        ///   The mathematics in this method are pretty much based on the calculations found in
        ///   "Artificial Intelligence for Games" by Ian Millington and John Funge (pg 125).
        /// </remarks>
        /// <param name="start">The projectile's launch position.</param>
        /// <param name="target">The projectile's target position.</param>
        /// <param name="result">The potential launch vector.</param>
        /// <returns>true if there was a solution, false otherwise.</returns>
        public bool CalculateFiringSolution(Vector3 start, Vector3 target, out Vector3 result)
        {
            if (!RayPool.IsBallistic)
            {
                result = Vector3.Normalize(target - start);
                return true;
            }

            var s = ProjectileSpeed;
            var delta = target - start;
            var a = Vector3.Dot(Physics.gravity, Physics.gravity);
            var b = -4 * (Vector3.Dot(Physics.gravity, delta) + s * s);
            var c = 4 * Vector3.Dot(delta, delta);

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
                time1 = float.MaxValue;
            }

            if (float.IsNaN(time2))
            {
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
                else if (useHighPath)
                {
                    time = Mathf.Max(time1, time2);
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
