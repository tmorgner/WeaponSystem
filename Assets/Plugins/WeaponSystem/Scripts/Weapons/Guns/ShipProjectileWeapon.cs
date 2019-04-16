using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public class ShipProjectileWeapon : ShipWeapon, IWeaponBehaviour
    {
        [SerializeField] DefaultWeaponTargetTracker targetTracker;
        [SerializeField] WeaponControl weaponControl;
        [SerializeField] WeaponDefinition weaponDefinition;
        [SerializeField] Light rayOriginPointA;
        [SerializeField] float targetPointResetTime;
        bool automaticTarget;
        readonly WeaponStateTracker weaponStateTracker;
        float lightIntensity;

        public override float Range => weaponDefinition.MaximumTrackingRange;

        public override bool TryGetWeaponDefinition(out WeaponDefinition w)
        {
            w = weaponDefinition;
            return true;
        }

        public ShipProjectileWeapon()
        {
            weaponStateTracker = new WeaponStateTracker();
            weaponStateTracker.WeaponBehaviour = this;
        }

        void Awake()
        {
            automaticTarget = targetTracker != null && targetTracker.Mode != DefaultWeaponTargetTracker.AimingMode.Manual;
            lightIntensity = rayOriginPointA.intensity;
            targetPointResetTime = Mathf.Max(0.1f, targetPointResetTime);
        }

        protected virtual bool FindTarget(out Vector3 origin)
        {
            if (automaticTarget)
            {
                targetTracker.ResetTarget();
                if (targetTracker.HasTarget)
                {
                    var distance = targetTracker.PredictPosition(out origin);
                    if (!(distance > weaponDefinition.MaximumTrackingRange))
                    {
                        return true;
                    }
                }

                origin = transform.position + transform.forward * weaponDefinition.MaximumTrackingRange;
                return true;
            }

            return FindTargetWithControlDirection(out origin);
        }

        protected virtual bool FindTargetWithControlDirection(out Vector3 origin)
        {
            origin = transform.position + transform.forward * weaponDefinition.MaximumTrackingRange;
            return true;
        }

        void Update()
        {
            if (!weaponControl.ShouldFire(this))
            {
                weaponStateTracker.MarkIdle();
                return;
            }

            Debug.Log("Weapon " + this + " wants to fire");
            if (FindTarget(out var targetPoint))
            {
                weaponStateTracker.Update(targetPoint);
            }
            else
            {
                weaponStateTracker.MarkIdle();
            }
        }

        void IWeaponBehaviour.BeginIdle()
        {
            rayOriginPointA.intensity = 0;
        }

        float IWeaponBehaviour.BeginCharging()
        {
            return weaponDefinition.FireDelay;
        }

        void IWeaponBehaviour.WhileCharging(float percentComplete)
        {
            rayOriginPointA.intensity = percentComplete * lightIntensity;
        }

        float IWeaponBehaviour.BeginFire(ref Vector3 target)
        {
            var fireTarget = weaponDefinition.RandomizeTarget(rayOriginPointA.transform, target);

            DoFire(fireTarget);

            return weaponDefinition.FireCoolDown;
        }

        void DoFire(Vector3 fireTarget)
        {
            var projectileSpeed = weaponDefinition.ProjectileSpeed;
            var rayPool = weaponDefinition.RayPool;
            var rayATransform = rayOriginPointA.transform.position;
            if (weaponDefinition.CalculateFiringSolution(rayATransform, fireTarget, out var rayATarget))
            {
                if (rayPool.TryGet(out var rayA))
                {
                    rayA.Fire(weaponControl.gameObject,
                              rayATransform,
                              rayATarget * projectileSpeed,
                              weaponDefinition.ProjectileTTL,
                              weaponDefinition.InteractWith);
                }
            }

        }

        void IWeaponBehaviour.WhileCoolDown(float chargeState)
        {
            // charge is 1 - charge-squared for a very quick falloff.
            chargeState *= chargeState;
            chargeState = 1 - chargeState;

            rayOriginPointA.intensity = chargeState * lightIntensity;
        }

    }
}