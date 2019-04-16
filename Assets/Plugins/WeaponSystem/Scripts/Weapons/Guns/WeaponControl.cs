using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public interface IPointerDirectionService
    {
        bool TargetDirection(out Vector3 origin, out Vector3 direction, out Vector3 hitPoint, out bool hitValidTarget);
        bool PointerDirection(out Vector3 origin, out Vector3 direction, out Vector3 farPoint);
    }

    public abstract class WeaponControl : MonoBehaviour
    {
        public abstract ShipWeapon ActiveWeapon { get; }
        public abstract IPointerDirectionService PointerDirectionService { get; }
        public abstract Quaternion NeutralRotation { get; }

        public abstract bool ShouldFire(ShipWeapon weapon);
    }
}