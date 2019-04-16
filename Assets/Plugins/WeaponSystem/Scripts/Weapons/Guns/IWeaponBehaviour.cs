using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public interface IWeaponBehaviour
    {
        void BeginIdle();
        float BeginCharging();
        void WhileCharging(float percentComplete);
        float BeginFire(ref Vector3 target);
        void WhileCoolDown(float percentComplete);
    }
}