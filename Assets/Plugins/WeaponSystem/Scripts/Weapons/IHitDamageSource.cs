using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons
{
    public interface IHitDamageSource
    {
        float DamagePerHit { get; }
        GameObject GameObject { get; }
    }
}