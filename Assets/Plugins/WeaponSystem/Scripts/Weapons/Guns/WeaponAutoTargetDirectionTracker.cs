using NaughtyAttributes;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A weapon rotation controller that attempts to point the gun at the
    ///   currently tracked target from the automatic target-tracker. Use
    ///   this script on computer controlled rotating guns.
    /// </summary>
    public class WeaponAutoTargetDirectionTracker : WeaponDirectionTracker
    {
        [Required]
        [SerializeField] 
        [Tooltip("The component that selects targets to be fired at. This should be the same instance the main gun script uses.")]
        WeaponTargetTracker targetTracker;

        /// <summary>
        ///   Returns the gaze direction of the gun. This method queries the target tracker
        ///   assigned to this class to align the gun with the predicted movement path of
        ///   the tracked target.
        /// </summary>
        /// <returns>The gaze direction rotation.</returns>
        protected override Quaternion QueryGazeDirection()
        {
            if (!targetTracker.PredictCurrentTargetPosition(out _, out var hitPoint))
            {
                return QueryNeutralPosition();
            }

            var up = NeutralPosition.up;
            return Quaternion.LookRotation((hitPoint - WeaponMount.position), up);
        }
    }
}