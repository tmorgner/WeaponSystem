using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons
{
    /// <summary>
    ///   An event receiver interface that marks game objects that can receive hits from
    ///   projectiles.
    /// </summary>
    public interface IHitReceiver
    {
        /// <summary>
        /// Informs the game object that it has been hit by a damage effect from
        /// a projectile.
        /// </summary>
        /// <param name="source">
        /// The gun or game object that launched the projectile.
        /// </param>
        /// <param name="projectile">
        /// The projectile that has hit this target.
        /// </param>
        /// <param name="point">
        /// The point where the <paramref name="projectile"/> hit. For area
        /// damage type projectiles this point might lay outside of any collider
        /// of this target.
        /// </param>
        void OnReceivedHit(GameObject source, IHitDamageSource projectile, Vector3 point);
    }
}