using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.WeaponPlatform
{
    /// <summary>
    ///   A player weapon definition provides access to gun parameters without
    ///   exposing the gun itself. This reduces the coupling between the two
    ///   subsystems so that player can reuse any gun instead of requiring
    ///   specialized implementations.
    /// </summary>
    public abstract class PlayerWeapon : MonoBehaviour
    {
        /// <summary>
        ///   Returns the weapon range.
        /// </summary>
        public abstract float Range { get; }

        /// <summary>
        ///   Attempts to query a weapon definition for the player weapon.
        /// </summary>
        public abstract bool TryGetWeaponDefinition(out WeaponDefinition w);
    }
}