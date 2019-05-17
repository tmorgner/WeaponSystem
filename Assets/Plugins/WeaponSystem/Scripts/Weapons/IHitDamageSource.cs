using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons
{
    /// <summary>
    ///    A general interface marking a game object behavior that can cause
    ///    damage to other objects during a collision.
    /// </summary>
    public interface IHitDamageSource
    {
        /// <summary>
        ///  The muzzle velocity of the projectile. If you need the actual velocity
        ///  at the time of the impact, query the rigid-body if one is available.
        /// </summary>
        float Velocity { get; }

        /// <summary>
        ///  The direction the projectile travelled.
        /// </summary>
        Vector3 Direction { get; }

        /// <summary>
        ///   A damage scale indicator.
        /// </summary>
        float DamagePerHit { get; }

        /// <summary>
        ///   The game object to which this damage source was attached. Use this to
        ///   gather more context information if necessary.
        /// </summary>
        GameObject GameObject { get; }
    }
}