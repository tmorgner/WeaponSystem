using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons
{
    public interface IHitReceiver
    {
        void OnReceivedHit(GameObject source, IHitDamageSource projectile, Vector3 point);
    }
}