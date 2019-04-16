using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public abstract class ShipWeapon : MonoBehaviour
    {
        public abstract float Range { get; }
        public abstract bool TryGetWeaponDefinition(out WeaponDefinition w);
    }
}