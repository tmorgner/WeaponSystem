using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A query callback that allows TargetSelectionStrategy implementations to acquire additional
    ///   information about the target object.
    /// </summary>
    public interface ITargetSelectionInformation
    {
        /// <summary>
        ///   Attempts to predict the targets position based on the target's current velocity and the
        ///   weapon's projectile velocity.
        /// </summary>
        /// <param name="target">The target whose position should be predicted</param>
        /// <param name="distance">The predicted distance.</param>
        /// <param name="predictedPosition">The predicted position.</param>
        /// <returns>true if the target is in range and can be fired at, false otherwise.</returns>
        bool PredictTargetPosition(Rigidbody target, out float distance, out Vector3 predictedPosition);

        /// <summary>
        ///   Checks whether the target is currently visible.
        /// </summary>
        /// <param name="target">The target that should be checked.</param>
        /// <returns>true if the target is visible, false otherwise.</returns>
        bool IsVisible(Rigidbody target);

        float MaximumTargetRange { get; }
    }
}