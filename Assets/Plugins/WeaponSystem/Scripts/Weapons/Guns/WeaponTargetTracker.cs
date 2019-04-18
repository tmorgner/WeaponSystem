using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A interface definition for target trackers. As this is Unity, we cannot use
    ///   proper interfaces as the serializer cannot handle those.
    /// </summary>
    /// <seealso cref="DefaultWeaponTargetTracker"/>
    public abstract class WeaponTargetTracker : MonoBehaviour
    {
        public abstract bool HasTarget { get; }
        public abstract bool PredictCurrentTargetPosition(out float distance, out Vector3 predictedPosition);
        public abstract void ResetTarget();
    }
}