using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A no-op target tracker that does nothing.
    /// </summary>
    public class ManualWeaponTargetTracker: WeaponTargetTracker
    {
        [SerializeField] WeaponDefinition weaponDefinition;

        public override bool HasTarget => true;

        /// <summary>
        ///  Always returns a target directly in front of the gun.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="predictedPosition"></param>
        /// <returns></returns>
        public override bool PredictCurrentTargetPosition(out float distance, out Vector3 predictedPosition)
        {
            predictedPosition = transform.position + transform.forward * weaponDefinition.MaximumTrackingRange;
            distance = weaponDefinition.MaximumTrackingRange;
            return true;
        }

        public override void ResetTarget()
        {
        }
    }
}
