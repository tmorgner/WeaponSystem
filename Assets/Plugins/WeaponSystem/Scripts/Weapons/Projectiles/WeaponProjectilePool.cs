using RabbitStewdio.Unity.Plugins.UnityTools.Scripts;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    [CreateAssetMenu(menuName = "GunShip/Weapons/Weapon Projectile Pool")]
    public class WeaponProjectilePool : ScriptableServiceObject<WeaponProjectilePoolBehaviour>
    {
        [SerializeField] WeaponProjectile prefab;
        [SerializeField] int poolSizeLimit;
        [SerializeField] bool strictLimit;
        [SerializeField] int preallocatedElements;
        WeaponProjectile effectivePrefab;

        internal WeaponProjectile LocalProjectilePrefab => prefab;

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
            if (prefab != null)
            {
                IsBallistic = prefab.IsBallistic;
            }
            else
            {
                IsBallistic = false;
            }
        }

        public bool IsBallistic { get; set; }

        public WeaponProjectile Initialize(WeaponDefinition weaponDefinition)
        {
            if (effectivePrefab == null)
            {
                effectivePrefab = prefab;
            }

            RecalculateBallisticFlag();
            return effectivePrefab;
        }

        protected override void OnQuittingOverride(WeaponProjectilePoolBehaviour behaviour)
        {
            ServiceBehaviour.ClearPool();
        }

        protected override void OnConfigureService(WeaponProjectilePoolBehaviour b)
        {
            b.PoolSizeLimit = poolSizeLimit;
            b.Prefab = effectivePrefab;
            b.StrictLimit = strictLimit;
            b.Activate();
            b.PreAllocate(preallocatedElements);
        }

        public bool TryGet(out WeaponProjectile ray) => ServiceBehaviour.TryGet(out ray);
        public void Release(WeaponProjectile ray) => ServiceBehaviour.Release(ray);

    }
}