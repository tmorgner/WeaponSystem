using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A weapon behaviour. This interface contains a set of callback for the
    ///   weapon state tracker that allows weapon implementations to influence
    ///   the state progression and to receive targeted notifications when the
    ///   weapon state changes.
    /// </summary>
    public interface IWeaponBehaviour
    {
        /// <summary>
        ///   A call back for when the weapon starts to become idle.
        /// </summary>
        void BeginIdle();

        /// <summary>
        ///   A call back for when the weapon starts to charge.
        /// </summary>
        /// <returns>the time the weapon should charge</returns>
        float BeginCharging();

        /// <summary>
        ///   A callback that is invoked while the weapon is charged.
        ///   This provides the current charge status as percentage
        ///   in the range of 0 to 1 so that implementors do not have
        ///   to track time themselves.
        /// </summary>
        /// <param name="percentComplete">the current charge status</param>
        void WhileCharging(float percentComplete);

        /// <summary>
        ///   A callback that is invoked when the weapon is firing.
        /// </summary>
        /// <param name="target">the target position to be fired at</param>
        /// <returns>the time the weapon should cool down after firing</returns>
        float BeginFire(ref Vector3 target);

        /// <summary>
        ///   A callback that is invoked while the weapon is cooling down.
        ///   This provides the current status as percentage
        ///   in the range of 0 to 1 so that implementors do not have
        ///   to track time themselves.
        /// </summary>
        /// <param name="percentComplete">the current cool down status</param>
        void WhileCoolDown(float percentComplete);
    }
}
