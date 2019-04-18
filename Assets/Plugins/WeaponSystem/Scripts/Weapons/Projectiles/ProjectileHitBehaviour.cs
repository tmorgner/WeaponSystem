using RabbitStewdio.Unity.UnityTools;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    /// <summary>
    ///  A projectile hit behaviour that attaches the projectile to 
    /// </summary>
    [RequireComponent(typeof(FollowObjectConstraint))]
    public class ProjectileHitBehaviour : MonoBehaviour
    {
        [SerializeField] GameObject scorchMark;
        FollowObjectConstraint constraint;

        void Awake()
        {
            constraint = GetComponent<FollowObjectConstraint>();
        }

        /// <summary>
        ///   Initializer that attaches this gameObject to the given
        ///   target. The game object will follow any positional or
        ///   directional changes the target object makes.
        /// </summary>
        /// <param name="other"></param>
        public void OnHit(Transform other)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Debug.Log("On Hit for " + other);

            constraint.SetTarget(other);
            constraint.enabled = true;
        }

        void OnDisable()
        {
            constraint.enabled = false;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}