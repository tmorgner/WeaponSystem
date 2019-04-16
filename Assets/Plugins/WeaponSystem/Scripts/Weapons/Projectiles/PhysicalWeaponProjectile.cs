using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicalWeaponProjectile : WeaponProjectile
    {
        Rigidbody rigidBody;
        bool rigidBodyFetched;
        bool isBallistic;

        public override bool IsBallistic
        {
            get
            {
                if (!rigidBodyFetched)
                {
                    FetchRigidbody();
                }
                return isBallistic;
            }
        }

        protected override void AwakeOverride()
        {
            if (!rigidBodyFetched)
            {
                FetchRigidbody();
            }
        }

        void FetchRigidbody()
        {
            rigidBodyFetched = true;
            rigidBody = GetComponent<Rigidbody>();
            isBallistic = rigidBody.useGravity;
        }

        public override void Fire(GameObject source, Vector3 origin, Vector3 directionAndVelocity, float timeToLive, LayerMask mask)
        {
            base.Fire(source, origin, directionAndVelocity, timeToLive, mask);
            rigidBody.position = transform.position;
            rigidBody.rotation = transform.rotation;
            rigidBody.velocity = directionAndVelocity;
            rigidBody.drag = 0;
        }

        void OnCollisionEnter(Collision other)
        {
            PerformHit(new HitInformation(other));
        }
    }
}