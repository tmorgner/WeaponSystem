using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   Represents target selector to assist aiming decisions. These objects are meant to be
    ///   used in conjunction with a <see cref="TargetSelectionStrategy"/>.
    /// </summary>
    public interface IAimingMeasure
    {
        /// <summary>
        ///   Resets the tracker. Must be called before the target selection strategy is
        ///   asked to filter the targets.
        /// </summary>
        void Reset();

        /// <summary>
        ///   Updates the selection with the new target, a preference weight and a predicted location.
        /// </summary>
        /// <param name="body">The target that is added to the selection</param>
        /// <param name="targetWeight">The preference weight of the target. This should be a value between 0 and 1.</param>
        /// <param name="target">The predicted target position to fire at.</param>
        void Track(Rigidbody body, float targetWeight, ref Vector3 target);

        /// <summary>
        ///   Return the rigid-body that has been selected as the best target.
        /// </summary>
        Rigidbody Result { get; }
    }
}