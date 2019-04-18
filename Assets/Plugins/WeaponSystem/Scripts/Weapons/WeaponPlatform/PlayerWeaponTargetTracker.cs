using RabbitStewdio.Unity.WeaponSystem.Weapons.Guns;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.WeaponPlatform
{
    /// <summary>
    ///  Player weapons never signal that there is no target. If the player wants to
    ///  shoot into the air, we should let them do that.
    /// </summary>
    public class PlayerWeaponTargetTracker: DefaultWeaponTargetTracker
    {
        public override bool PredictCurrentTargetPosition(out float distance, out Vector3 position)
        {
            if (this.Mode != AimingMode.Manual &&
                base.PredictCurrentTargetPosition(out distance, out position))
            {
                return true;
            }

            var weaponDefinition = WeaponDefinition;
            position = transform.position + transform.forward * weaponDefinition.MaximumTrackingRange;
            distance = weaponDefinition.MaximumTrackingRange;
            return true;
        }

        public override bool HasTarget
        {
            get
            {
                RevalidateTrackedTarget();
                return true;
            }
        }
    }
}
