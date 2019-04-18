using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    /// <summary>
    ///  A weapon projectile that uses a rigid-body of the physics system for collision detection and
    ///  locomotion.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicalWeaponProjectile : WeaponProjectile
    {
        [Tooltip("Controls whether the projectile will always face the current movement. Use this for bullets and arrows.")]
        [SerializeField] bool adjustRotationToForwardDirection;

        Rigidbody rigidBody;
        bool rigidBodyFetched;
        bool isBallistic;

        /// <summary>
        /// Controls whether the projectile will always face the current movement. Use this for bullets and arrows.
        /// </summary>
        protected bool AdjustRotationToForwardDirection => adjustRotationToForwardDirection;

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void Fire(GameObject source, Vector3 origin, Vector3 directionAndVelocity, float timeToLive, LayerMask mask)
        {
            base.Fire(source, origin, directionAndVelocity, timeToLive, mask);
            rigidBody.position = transform.position;
            rigidBody.rotation = transform.rotation;
            rigidBody.velocity = directionAndVelocity;
            rigidBody.drag = 0;
        }

        /// <inheritdoc />
        protected override void FixedUpdateOverride()
        {
            base.FixedUpdateOverride();
            if (adjustRotationToForwardDirection)
            {
                transform.LookAt(transform.position + rigidBody.velocity);
            }
        }

        /// <summary>
        ///   Unity Event Callback. 
        /// </summary>
        /// <param name="other">The other collider that collided with this bullet.</param>
        protected virtual void OnCollisionEnter(Collision other)
        {
            PerformHit(new HitInformation(other));
        }
    }
}