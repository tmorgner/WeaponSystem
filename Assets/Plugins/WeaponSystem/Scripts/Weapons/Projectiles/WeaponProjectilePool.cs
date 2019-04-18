using RabbitStewdio.Unity.Plugins.UnityTools.Scripts;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    /// <summary>
    ///   A projectile pool service. This object pool will manage projectiles of a
    ///   given type. You will only need to create explicit instances of this service
    ///   if you need to share the projectile pool between different weapon definitions.
    /// </summary>
    /// <remarks>
    ///   Like all service objects, this service will create a service behaviour in the current scene
    ///   to manage the created projectile instances.
    /// </remarks>
    [CreateAssetMenu(menuName = "Weapons/Weapon Projectile Pool")]
    public class WeaponProjectilePool : ScriptableServiceObject<WeaponProjectilePoolBehaviour>
    {
        [Tooltip("[Optional] A prefab of the weapon projectiles used in the pool. You only have to assign this if you intend to share the pool with multiple weapon definitions. Otherwise define the projectile on the weapon definition itself.")]
        [SerializeField] WeaponProjectile prefab;
        [Tooltip("The maximum number of projectiles allocated in the pool.")]
        [SerializeField] int poolSizeLimit;
        [Tooltip("Should the pool reject requests for projectiles when all projectiles have been reserved?")]
        [SerializeField] bool strictLimit;
        [Tooltip("How many projectile instances should be pre-allocated at the start of the scene?")]
        [SerializeField] int preallocatedElements;
        WeaponProjectile effectivePrefab;

        /// <summary>
        ///   Internal debug accessor to allow the weapon definition class to compute the effective projectile used for a gun.
        /// </summary>
        internal WeaponProjectile LocalProjectilePrefab => prefab;

        /// <summary>
        ///   Defines some sensible defaults for automatically created pools.
        /// </summary>
        public WeaponProjectilePool()
        {
            poolSizeLimit = 100;
            strictLimit = false;
            preallocatedElements = 10;
        }

        /// <inheritdoc />
        protected override void OnEnableOverride()
        {
            base.OnEnableOverride();
            if (effectivePrefab == null)
            {
                effectivePrefab = prefab;
            }

            RecalculateBallisticFlag();
        }

        void RecalculateBallisticFlag()
        {
            if (effectivePrefab != null)
            {
                IsBallistic = effectivePrefab.IsBallistic;
            }
            else
            {
                IsBallistic = false;
            }
        }

        /// <summary>
        ///   Returns whether the projectile will be affected by gravity.
        /// </summary>
        public bool IsBallistic { get; private set; }

        /// <summary>
        ///   Initializes the projectile pool from the given weapon definition.
        /// </summary>
        /// <param name="weaponDefinition">The weapon definition that called this method.</param>
        /// <param name="projectileOverride">The wepon definitions preferred projectile prefab. This will only be applied if this pool has no other projectile prefab set.</param>
        /// <returns>The effective projectile prefab that is going to be used in this pool.</returns>
        internal WeaponProjectile Initialize(WeaponDefinition weaponDefinition, WeaponProjectile projectileOverride)
        {
            if (effectivePrefab == null)
            {
                effectivePrefab = prefab;
            }

            if (effectivePrefab == null)
            {
                effectivePrefab = projectileOverride;
            }

            RecalculateBallisticFlag();
            return effectivePrefab;
        }

        /// <summary>
        ///   Releases all instances when the application finishes or the service object gets unloaded by the framework.
        /// </summary>
        /// <param name="behaviour"></param>
        protected override void OnQuittingOverride(WeaponProjectilePoolBehaviour behaviour)
        {
            ServiceBehaviour.ClearPool();
        }

        /// <summary>
        ///   Called during the initialization of the service behaviour.
        /// </summary>
        /// <param name="b"></param>
        protected override void OnConfigureService(WeaponProjectilePoolBehaviour b)
        {
            b.PoolSizeLimit = poolSizeLimit;
            b.Prefab = effectivePrefab;
            b.StrictLimit = strictLimit;
            b.Activate();
            b.PreAllocate(preallocatedElements);

            b.gameObject.name += " (" + b.Prefab.name + ")";

        }

        /// <summary>
        ///   Attempts to retrieve a projectile from the pool.
        /// </summary>
        /// <param name="ray">the projectile or null</param>
        /// <returns>true if the pool returned a projectile, false otherwise.</returns>
        /// <seealso cref="WeaponProjectilePoolBehaviour.TryGet"/>
        public bool TryGet(out WeaponProjectile ray) => ServiceBehaviour.TryGet(out ray);

        /// <summary>
        ///   Reclaims the given weapon projectile for the pool.
        /// </summary>
        /// <param name="ray">The projectile that should be reclaimed.</param>
        /// <seealso cref="WeaponProjectilePoolBehaviour.Release"/>
        public void Release(WeaponProjectile ray) => ServiceBehaviour.Release(ray);

    }
}