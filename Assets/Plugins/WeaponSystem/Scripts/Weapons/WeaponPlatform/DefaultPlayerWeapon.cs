using NaughtyAttributes;
using RabbitStewdio.Unity.WeaponSystem.Weapons.Guns;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.WeaponPlatform
{
    public class DefaultPlayerWeapon : PlayerWeapon
    {
        [Required]
        [SerializeField] 
        GunBase gun;
        
        [Required]
        [SerializeField] 
        WeaponControl weaponControl;
        bool gunNotNull;

        void Awake()
        {
            gunNotNull = gun != null;
        }

        void Update()
        {
            gun.Armed = weaponControl.ShouldFire(this);
            
        }

        /// <summary>
        ///   Returns the weapon range.
        /// </summary>
        public override float Range
        {
            get
            {
                if (TryGetWeaponDefinition(out var w))
                {
                    return w.Range;
                }

                return 0;
            }
        }

        /// <summary>
        ///   Attempts to query a weapon definition for the player weapon.
        /// </summary>
        public override bool TryGetWeaponDefinition(out WeaponDefinition w)
        {
            if (gunNotNull)
            {
                w = gun.WeaponDefinition;
                return true;
            }

            w = default;
            return false;
        }
    }
}