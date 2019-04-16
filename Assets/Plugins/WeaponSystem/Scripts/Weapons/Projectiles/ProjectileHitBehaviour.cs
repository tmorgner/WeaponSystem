using RabbitStewdio.Unity.UnityTools;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    [RequireComponent(typeof(FollowObjectConstraint))]
    public class ProjectileHitBehaviour : MonoBehaviour
    {
        [SerializeField] GameObject scorchMark;
        FollowObjectConstraint constraint;

        void Awake()
        {
            constraint = GetComponent<FollowObjectConstraint>();
        }

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