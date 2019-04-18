using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.WeaponPlatform
{
    /// <summary>
    ///   A weapon control interface. This grants access to the weapon controls
    ///   of player centric games and an implementation agnostic interface to
    ///   the UI and input system.
    /// </summary>
    public abstract class WeaponControl : MonoBehaviour
    {
        /// <summary>
        ///   Checks whether this weapon is currently active. Usually this means
        ///   the this the the weapon the player would shoot with.
        /// </summary>
        /// <param name="weapon">The weapon to be queried</param>
        /// <returns>true if the given weapon is active, false otherwise.</returns>
        public abstract bool IsActiveWeapon(PlayerWeapon weapon);

        /// <summary>
        ///   A pointer direction service allows weapons to query the direction
        ///   the player looks or points at.
        /// </summary>
        public abstract IPointerDirectionService PointerDirectionService { get; }

        /// <summary>
        ///   Queries the control system on whether the given weapon should fire.
        /// </summary>
        /// <param name="weapon">The weapon to be queried</param>
        /// <returns>true if the given weapon should fire, false otherwise.</returns>
        public abstract bool ShouldFire(PlayerWeapon weapon);
    }
}