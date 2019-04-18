using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.WeaponPlatform
{
    /// <summary>
    ///   A input system query service that allows the weapon system to
    ///   request targeting information from the player.
    ///   This is an Unity abstract base class so that the serializer can handle scene references correctly.
    /// </summary>
    public abstract class PointerDirectionService : MonoBehaviour, IPointerDirectionService
    {
        /// <summary>
        ///   Queries the location the player targets filtered by whether the player actually
        ///   points at a valid target.
        /// </summary>
        /// <param name="origin">returns the player origin position for targeting purposes.</param>
        /// <param name="direction">returns the direction the player is targeting towards</param>
        /// <param name="farPoint">returns the hit point towards the pointed-at target or a far away point in the general direction of the player's targeting if there is no valid target.</param>
        /// <param name="hitValidTarget">indicates whether the far point is a valid target or just a general point somewhere in the direction of the targeting.</param>
        /// <returns></returns>
        public abstract bool TargetDirection(out Vector3 origin, out Vector3 direction, out Vector3 hitPoint, out bool hitValidTarget);

        /// <summary>
        ///   Queries the general direction a player is pointing at. The farPoint returned does not
        ///   necessarily point at a valid target object or anything at all and should only be used
        ///   as directional hint.
        /// </summary>
        /// <param name="origin">returns the player origin position for targeting purposes.</param>
        /// <param name="direction">returns the direction the player is targeting towards</param>
        /// <param name="farPoint">returns a far away point in the general direction of the player's targeting.</param>
        /// <returns></returns>
        public abstract bool PointerDirection(out Vector3 origin, out Vector3 direction, out Vector3 farPoint);
    }
}