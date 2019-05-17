using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public class HitAreaMarker: MonoBehaviour
    {
        // a simple marker object that assists in aiming. It makes it 
        // faster to find a valid target spot to shoot at. Without it
        // we either shoot at the ground (where the rigidbody position
        // is or shoot at the center of the bounding box (and thus may
        // actually miss the target.
    }
}
