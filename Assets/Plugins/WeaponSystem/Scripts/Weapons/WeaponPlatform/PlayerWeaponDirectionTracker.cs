using NaughtyAttributes;
using RabbitStewdio.Unity.WeaponSystem.Weapons.Guns;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.WeaponPlatform
{
    /// <summary>
    ///   Aligns the weapon with the player's targeting direction. This behaviour
    ///   supports cases where the gaze of the player is independent of the body
    ///   of the player (ie for VR or mouse-look / HOTAS head controls).
    /// </summary>
    public class PlayerWeaponDirectionTracker : WeaponDirectionTracker
    {
        [Required]
        [SerializeField] 
        WeaponControl weaponControl;

        [Required]
        [SerializeField] 
        PlayerWeapon weapon;

        [SerializeField] bool alwaysActive;

        /// <summary>
        ///   Returns a quaternion that represents a rotation towards the target point
        ///   pointed at by the player.
        /// </summary>
        /// <remarks>
        ///   Right now this function is unclamped and thus does not respects the
        ///   angle limits of the rotation area. 
        /// </remarks>
        /// <returns>A rotation towards the pointed-at target.</returns>
        protected override Quaternion QueryGazeDirection()
        {
            if (!alwaysActive && !weaponControl.IsActiveWeapon(weapon))
            {
                return QueryNeutralPosition();
            }

            if (!weaponControl.PointerDirectionService.TargetDirection(out _,
                                                                       out _,
                                                                       out var hitPoint,
                                                                       out _))
            {
                return QueryNeutralPosition();
            }

            var up = NeutralPosition.up;
            return Quaternion.LookRotation((hitPoint - WeaponMount.position), up);
        }

    }
}