using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A base class for all projectile weapons. This class manages the weapon's
    ///   internal state, handles all target tracking (via the injected WeaponTargetTracker instances)
    ///   and fires projectiles instantiated via a weapon projectile pool.
    /// </summary>
    public abstract class GunBase : MonoBehaviour, IWeaponBehaviour
    {
        [Required]
        [SerializeField] 
        WeaponTargetTracker targetTracker;

        [Required]
        [SerializeField] 
        WeaponDefinition weaponDefinition;

        [SerializeField] UnityEvent fireEvent;

        [SerializeField] 
        bool armed;

        readonly WeaponStateTracker weaponStateTracker;

        /// <summary>
        ///   The tracker origin represents a origin point for weapon calculations. If this
        ///   weapon has only one barrel, the origin point will be the barrel itself,
        ///   otherwise it should be a suitable location that is close to all possible
        ///   projectile spawn points.
        /// </summary>
        protected abstract Transform WeaponTargetTrackerOrigin { get; }

        /// <summary>
        ///   The weapon definition defines the characteristics of this weapon and contains
        ///   the weapon projectile pool needed to instantiate projectiles.
        /// </summary>
        public WeaponDefinition WeaponDefinition
        {
            get => weaponDefinition;
            protected set => weaponDefinition = value;
        }

        /// <summary>
        ///   The target tracker is responsible for selecting targets.
        /// </summary>
        protected WeaponTargetTracker TargetTracker
        {
            get => targetTracker;
            set => targetTracker = value;
        }

        /// <summary>
        ///   Returns the current weapon state.
        /// </summary>
        public WeaponState State => weaponStateTracker.State;

        /// <summary>
        ///  The armed flag controls whether the weapon will actually fire at targets.
        /// </summary>
        public bool Armed
        {
            get => armed;
            set => armed = value;
        }

        /// <summary>
        ///   The current target of the weapon. This provides the actual point of where the
        ///   weapon fired, including any deviation due to inaccuracies. For non-ballistic
        ///   weapons this is the point where the projectile will hit.
        /// </summary>
        public Vector3 FireTarget { get; protected set; }

        /// <summary>
        ///   A Unity-Event that is fired when the gun fired a bullet.
        /// </summary>
        public UnityEvent FireEvent => fireEvent;

        protected GunBase()
        {
            weaponStateTracker = new WeaponStateTracker();
            weaponStateTracker.WeaponBehaviour = this;
        }

        /// <summary>
        ///   Unity Event Callback. This method updates the weapon state and acquires
        ///   new targets when necessary.
        /// </summary>
        protected void Update()
        {
            UpdateOverride();

            if (!armed || !targetTracker.HasTarget)
            {
                weaponStateTracker.MarkIdle();
                return;
            }

            if (!targetTracker.PredictCurrentTargetPosition(out _, out var predictedPosition))
            {
                targetTracker.ResetTarget();
                return;
            }

            weaponStateTracker.Update(predictedPosition);
        }

        /// <summary>
        ///   An extension point so that subclasses can receive update events.
        /// </summary>
        protected virtual void UpdateOverride()
        {
        }

        /// <inheritdoc />
        void IWeaponBehaviour.BeginIdle()
        {
            BeginIdleOverride();
        }

        /// <summary>
        ///   An extension point so that subclasses can react to idle events.
        /// </summary>
        protected virtual void BeginIdleOverride()
        {

        }

        /// <inheritdoc />
        float IWeaponBehaviour.BeginCharging()
        {
            return BeginChargingOverride();
        }

        /// <summary>
        ///   An extension point so that subclasses can react to charge state events.
        /// </summary>
        protected virtual float BeginChargingOverride()
        {
            return weaponDefinition.FireDelay;
        }

        /// <inheritdoc />
        void IWeaponBehaviour.WhileCharging(float percentComplete)
        {
            WhileChargingOverride(percentComplete);
        }

        /// <summary>
        ///   An extension point so that subclasses can react to charge events.
        /// </summary>
        protected virtual void WhileChargingOverride(float percentComplete)
        {

        }

        /// <inheritdoc />
        float IWeaponBehaviour.BeginFire(ref Vector3 target)
        {
            var fireTarget = weaponDefinition.RandomizeTarget(WeaponTargetTrackerOrigin, target);
            BeginFireOverride(fireTarget);
            return weaponDefinition.FireCoolDown;
        }

        /// <summary>
        ///   An extension point so that subclasses can react to fire events. This also
        ///   acts as cool-down started event in the same call.
        /// </summary>
        protected virtual void BeginFireOverride(Vector3 target)
        {

        }

        /// <summary>
        ///   Handles the firing of projectiles for the given muzzle position and target.
        /// </summary>
        /// <param name="muzzlePosition">The spawn location of the new projectile</param>
        /// <param name="fireTarget">The target position</param>
        /// <returns>true if the projectile has been fired, false otherwise.</returns>
        protected bool DoFire(Vector3 muzzlePosition, Vector3 fireTarget)
        {
            var weaponDefinition = WeaponDefinition;
            var projectileSpeed = weaponDefinition.ProjectileSpeed;

            if (weaponDefinition.CalculateFiringSolution(muzzlePosition, fireTarget, out var rayATarget) &&
                weaponDefinition.TryGetProjectile(out var rayA))
            {
                rayA.Fire(gameObject, muzzlePosition,
                          rayATarget * projectileSpeed,
                          weaponDefinition.ProjectileTTL,
                          weaponDefinition.InteractWith);
                
                this.FireTarget = fireTarget;
                this.FireEvent.Invoke();

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        void IWeaponBehaviour.WhileCoolDown(float chargeState)
        {
            WhileCoolDownOverride(chargeState);
        }

        /// <summary>
        ///   An extension point so that subclasses can react to cool-down state events.
        /// </summary>
        protected virtual void WhileCoolDownOverride(float chargeState)
        {

        }
    }
}