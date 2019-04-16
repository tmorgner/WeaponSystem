using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public abstract class GunBase : MonoBehaviour, IWeaponBehaviour
    {
        [SerializeField] WeaponTargetTracker targetTracker;
        [SerializeField] WeaponDefinition weaponDefinition;
        [SerializeField] bool armed;
        readonly WeaponStateTracker weaponStateTracker;

        protected abstract Transform WeaponTargetTrackerOrigin { get; }

        public WeaponDefinition WeaponDefinition => weaponDefinition;

        protected WeaponTargetTracker TargetTracker => targetTracker;

        public WeaponStateTracker.WeaponState State => weaponStateTracker.State;

        public bool Armed
        {
            get => armed;
            protected set => armed = value;
        }

        protected GunBase()
        {
            weaponStateTracker = new WeaponStateTracker();
            weaponStateTracker.WeaponBehaviour = this;
        }

        protected void Update()
        {
            UpdateOverride();

            if (!armed || !targetTracker.HasTarget)
            {
                weaponStateTracker.MarkIdle();
                return;
            }

            var distance = targetTracker.PredictPosition(out var predictedPosition);
            if (distance > weaponDefinition.MaximumTrackingRange)
            {
                targetTracker.ResetTarget();
                return;
            }

            weaponStateTracker.Update(predictedPosition);
        }

        protected virtual void UpdateOverride()
        {
        }

        void IWeaponBehaviour.BeginIdle()
        {
            BeginIdleOverride();
        }

        protected virtual void BeginIdleOverride()
        {

        }

        float IWeaponBehaviour.BeginCharging()
        {
            return BeginChargingOverride();
        }

        protected virtual float BeginChargingOverride()
        {
            return weaponDefinition.FireDelay;
        }

        void IWeaponBehaviour.WhileCharging(float percentComplete)
        {
            WhileChargingOverride(percentComplete);
        }

        protected virtual void WhileChargingOverride(float percentComplete)
        {

        }

        float IWeaponBehaviour.BeginFire(ref Vector3 target)
        {
            var fireTarget = weaponDefinition.RandomizeTarget(WeaponTargetTrackerOrigin, target);
            BeginFireOverride(fireTarget);
            return weaponDefinition.FireCoolDown;
        }

        protected virtual void BeginFireOverride(Vector3 target)
        {

        }

        protected void DoFire(Vector3 muzzlePosition, Vector3 fireTarget)
        {
            var weaponDefinition = WeaponDefinition;
            var projectileSpeed = weaponDefinition.ProjectileSpeed;
            var rayPool = weaponDefinition.RayPool;

            if (weaponDefinition.CalculateFiringSolution(muzzlePosition, fireTarget, out var rayATarget) &&
                rayPool.TryGet(out var rayA))
            {
                rayA.Fire(gameObject, muzzlePosition,
                          rayATarget * projectileSpeed,
                          weaponDefinition.ProjectileTTL,
                          weaponDefinition.InteractWith);
            }

        }

        void IWeaponBehaviour.WhileCoolDown(float chargeState)
        {
            WhileCoolDownOverride(chargeState);
        }

        protected virtual void WhileCoolDownOverride(float chargeState)
        {

        }
    }
}