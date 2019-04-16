using System;
using System.Collections.Generic;
using RabbitStewdio.Unity.GunShip.Game;
using RabbitStewdio.Unity.UnityTools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityTools;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    public abstract class WeaponProjectile : MonoBehaviour, IHitDamageSource
    {
        public enum WeaponProjectileState
        {
            Expired,
            Flying,
            Hit
        }

        public enum WeaponHitBehaviour
        {
            Single,
            Area
        }

        static readonly RaycastHit[] RaycastResults;
        static readonly Collider[] CollisionResults;
        static readonly List<Collider> CollectedColliders;
        static readonly List<Collider> CollectedProjectileColliders;

        [FormerlySerializedAs("projectilePrefab")] [SerializeField]
        GameObject projectileBody;
        [FormerlySerializedAs("muzzlePrefab")] [SerializeField]
        GameObject muzzleFlashEffect;
        [FormerlySerializedAs("hitPrefab")] [SerializeField]
        GameObject hitEffect;
        [SerializeField] AudioClip shotSFX;
        [SerializeField] AudioClip hitSFX;
        [SerializeField] float collisionDetectionDistance;
        [SerializeField] float damagePerHit;
        [SerializeField] WeaponHitBehaviour hitBehaviour;
        [FormerlySerializedAs("damageArea")]
        [SerializeField]
        float damageRadius;

        LayerMask layerMask;
        protected readonly ActivationTimer timer;
        protected readonly ActivationTimer fixedUpdateTimer;
        int sourceId;
        bool isMuzzlePrefabNotNull;
        bool isHitPrefabNotNull;
        bool isProjectilePrefabNotNull;
        GameObject source;

        public abstract bool IsBallistic { get; }

        public WeaponProjectileState State { get; protected set; }

        public WeaponProjectilePoolBehaviour Pool { get; set; }

        public Vector3 Direction { get; private set; }

        public float Velocity { get; private set; }

        public Vector3 Origin { get; private set; }

        public float RayLength { get; private set; }

        public float TimeToLive { get; set; }

        public float DamagePerHit => damagePerHit;

        public GameObject GameObject => gameObject;

        public bool HitPrefabActive
        {
            get { return isHitPrefabNotNull && hitEffect.activeSelf; }
            set
            {
                if (isHitPrefabNotNull)
                {
                    hitEffect.SetActive(value);
                }
            }
        }

        public bool MuzzlePrefabActive
        {
            get { return isMuzzlePrefabNotNull && muzzleFlashEffect.activeSelf; }
            set
            {
                if (isMuzzlePrefabNotNull)
                {
                    muzzleFlashEffect.SetActive(value);
                }
            }
        }

        public bool ProjectilePrefabActive
        {
            get { return isProjectilePrefabNotNull && projectileBody.activeSelf; }
            set
            {
                if (isProjectilePrefabNotNull)
                {
                    projectileBody.SetActive(value);
                }
            }
        }

        public WeaponProjectile()
        {
            timer = new ActivationTimer();
            fixedUpdateTimer = new ActivationTimer();
        }

        static WeaponProjectile()
        {
            RaycastResults = new RaycastHit[256];
            CollisionResults = new Collider[256];
            CollectedColliders = new List<Collider>();
            CollectedProjectileColliders = new List<Collider>();
        }

        void Awake()
        {
            if (projectileBody != null)
            {
                isProjectilePrefabNotNull = true;
            }

            if (hitEffect != null)
            {
                isHitPrefabNotNull = true;
                hitEffect.AddComponent<DisableWhenParticlesFinished>();
            }

            if (muzzleFlashEffect != null)
            {
                isMuzzlePrefabNotNull = true;
                muzzleFlashEffect.AddComponent<DisableWhenParticlesFinished>();
            }

            AwakeOverride();
        }

        protected virtual void AwakeOverride() { }

        void OnEnable()
        {
            State = WeaponProjectileState.Expired;
            if (projectileBody != null)
            {
                projectileBody.SetActive(false);
            }

            if (hitEffect != null)
            {
                hitEffect.SetActive(false);
            }

            if (muzzleFlashEffect != null)
            {
                muzzleFlashEffect.SetActive(false);
            }
        }

        public virtual void Fire(GameObject source, Vector3 origin, Vector3 directionAndVelocity, float timeToLive, LayerMask mask)
        {
            if (!origin.IsValid())
            {
                Debug.LogError("Invalid origin", source);
            }

            if (!directionAndVelocity.IsValid())
            {
                Debug.LogError("Invalid directionAndVelocity", source);
            }

            this.Origin = origin;
            this.Velocity = directionAndVelocity.magnitude;
            this.Direction = Vector3.Normalize(directionAndVelocity);
            this.TimeToLive = timeToLive;
            this.source = source;
            this.layerMask = mask;

            transform.position = origin;
            transform.rotation = Quaternion.LookRotation(directionAndVelocity, Vector3.up);

            timer.Start();
            fixedUpdateTimer.StartFixed();
            PlayShotSFX();

            State = WeaponProjectileState.Flying;
            MuzzlePrefabActive = true;

            if (collisionDetectionDistance <= 0)
            {
                if (IsBallistic)
                {
                    RayLength = (Time.fixedDeltaTime * (directionAndVelocity.magnitude + Physics.gravity.magnitude)) + 0.05f;
                }
                else
                {
                    RayLength = (Time.fixedDeltaTime * directionAndVelocity.magnitude) + 0.05f;
                }
            }
            else
            {
                RayLength = collisionDetectionDistance;
            }

            if (CheckCollision(origin, origin + directionAndVelocity * Time.fixedDeltaTime * 2, out var hit))
            {
                // we instantly would hit something in the next two frames. Shortcut that process.
                PerformHit(hit);
            }
            else
            {
                IgnoreFiredBy(source);
                ProjectilePrefabActive = true;
            }
        }

        protected virtual bool CheckCollision(Vector3 origin, Vector3 target, out HitInformation hit)
        {
            var directionVector = (target - origin);
            var count = Physics.RaycastNonAlloc(origin,
                                                directionVector,
                                                RaycastResults,
                                                RayLength,
                                                layerMask,
                                                QueryTriggerInteraction.Ignore);
            try
            {
                for (var i = 0; i < count; i++)
                {
                    var hitTest = new HitInformation(RaycastResults[i]);
                    if (CheckCollisionInstance(hitTest))
                    {
                        hit = hitTest;
                        return true;
                    }
                }

                hit = default;
                return false;
            }
            finally
            {
                Array.Clear(RaycastResults, 0, count);
            }
        }

        /// <summary>
        ///   Ignores collisions with the source object.
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        protected virtual bool CheckCollisionInstance(HitInformation hit)
        {
            var hitTransform = hit.transform;
            while (hitTransform != null)
            {
                if (hitTransform == source.transform)
                {
                    return false;
                }

                hitTransform = hitTransform.parent;
            }

            return true;
        }

        protected void PerformHit(HitInformation hit)
        {
            ProjectilePrefabActive = false;
            HitPrefabActive = true;
            TimeToLive = 0;
            State = WeaponProjectileState.Hit;
            OnProjectileCollision(hit);
        }

        protected virtual void OnProjectileCollision(HitInformation hit)
        {
            var transform = this.transform;
            transform.position = hit.point;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal);

            ApplyHitLocation(hit.transform, hit.point);
            PlayHitSFX();
        }

        void Update()
        {
            switch (State)
            {
                case WeaponProjectileState.Expired:
                {
                    return;
                }
                case WeaponProjectileState.Flying:
                {
                    timer.Update();
                    if (timer.TimePassed > TimeToLive)
                    {
                        TimeToLive = 0;
                        State = WeaponProjectileState.Expired;
                        OnProjectileFinished(true);
                    }
                    else
                    {
                        OnUpdateFlying();
                    }

                    break;
                }
                case WeaponProjectileState.Hit:
                {
                    if (isHitPrefabNotNull)
                    {
                        if (!hitEffect.activeSelf)
                        {
                            State = WeaponProjectileState.Expired;
                            TimeToLive = 0;
                            OnProjectileFinished(false);
                        }
                    }
                    else
                    {
                        State = WeaponProjectileState.Expired;
                        TimeToLive = 0;
                        OnProjectileFinished(false);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void OnUpdateFlying()
        {
        }

        protected virtual void OnProjectileFinished(bool timeout)
        {
            IgnoreFiredBy(source, false);

            HitPrefabActive = false;
            MuzzlePrefabActive = false;
            ProjectilePrefabActive = false;
            gameObject.SetActive(false);
            source = null;
            Pool.Release(this);
        }

        void FixedUpdate()
        {
            if (State != WeaponProjectileState.Flying)
            {
                return;
            }

            fixedUpdateTimer.FixedUpdate();
            OnFixedUpdate();
        }

        protected virtual void OnFixedUpdate()
        {
        }

        protected bool IsSource(GameObject o)
        {
            if (o.GetInstanceID() == sourceId)
            {
                return true;
            }

            var transformParent = o.transform.parent;
            if (transformParent != null)
            {
                return IsSource(transformParent.gameObject);
            }

            return false;
        }

        protected void IgnoreFiredBy(GameObject o, bool ignore = true)
        {
            sourceId = o.GetInstanceID();
            var selfColliders = gameObject.GetComponentsInChildrenNonAlloc(CollectedProjectileColliders, true, true);
            if (selfColliders.Count == 0)
            {
                return;
            }

            var otherComposite = o.GetComponent<CompositeColliderCollector>();
            if (otherComposite != null)
            {
                foreach (var selfCollider in selfColliders)
                {
                    foreach (var other in otherComposite)
                    {
                        Physics.IgnoreCollision(selfCollider, other, ignore);
                    }
                }

                return;
            }

            var otherCollider = o.GetComponentsInChildrenNonAlloc(CollectedColliders, true);
            foreach (var selfCollider in selfColliders)
            {
                foreach (var c in otherCollider)
                {
                    Physics.IgnoreCollision(selfCollider, c, ignore);
                }
            }
        }

        protected void PlayHitSFX()
        {
            if (hitSFX != null && GetComponent<AudioSource>())
            {
                GetComponent<AudioSource>().PlayOneShot(hitSFX);
            }
        }

        protected void ApplyHitLocation(Transform other, Vector3 hitPoint)
        {
            if (isHitPrefabNotNull)
            {
                var hit = hitEffect.GetComponent<ProjectileHitBehaviour>();
                if (hit != null)
                {
                    hit.OnHit(other);
                }
            }

            if (hitBehaviour != WeaponHitBehaviour.Single && damageRadius > 0)
            {
                var results = Physics.OverlapSphereNonAlloc(hitPoint, damageRadius, CollisionResults, layerMask, QueryTriggerInteraction.Ignore);
                for (var result = 0; result < results; result += 1)
                {
                    var hit = CollisionResults[result];
                    var hitTransform = hit.transform;
                    var areaHitReceiver = hitTransform.GetComponentInParent<IHitReceiver>();
                    if (areaHitReceiver != null)
                    {
                        var directionToHit = hitTransform.position - hitPoint;
                        if (hitBehaviour == WeaponHitBehaviour.Area && Physics.Raycast(hitPoint, directionToHit, out var areaHit, directionToHit.magnitude, layerMask))
                        {
                            var hr = areaHit.transform.GetComponentInParent<IHitReceiver>();
                            if (hr != areaHitReceiver)
                            {
                                continue;
                            }
                        }

                        areaHitReceiver.OnReceivedHit(this.source, this, hitPoint);
                    }
                }
            }
            else
            {
                var hitReceiver = other.gameObject.GetComponent<IHitReceiver>();
                hitReceiver?.OnReceivedHit(this.source, this, hitPoint);
            }
        }

        protected void PlayShotSFX()
        {
            if (shotSFX != null && GetComponent<AudioSource>())
            {
                GetComponent<AudioSource>().PlayOneShot(shotSFX);
            }
        }
    }

    public struct HitInformation
    {
        const int _RaycastHit = 1;
        const int _Collision = 2;

        readonly int discriminator;
        readonly RaycastHit raycastHit;
        readonly Collision collisionInfo;

        public HitInformation(RaycastHit raycastHit)
        {
            this.discriminator = _RaycastHit;
            this.collisionInfo = default;
            this.raycastHit = raycastHit;
        }

        public HitInformation(Collision collisionInfo)
        {
            this.discriminator = _Collision;
            this.raycastHit = default;
            this.collisionInfo = collisionInfo;
        }

        public Vector3 point
        {
            get
            {
                switch (discriminator)
                {
                    case _RaycastHit:
                        return raycastHit.point;
                    case _Collision when collisionInfo.contactCount > 0:
                        return collisionInfo.contacts[0].point;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public Vector3 normal
        {
            get
            {
                switch (discriminator)
                {
                    case _RaycastHit:
                        return raycastHit.normal;
                    case _Collision when collisionInfo.contactCount > 0:
                        return collisionInfo.contacts[0].normal;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public Transform transform
        {
            get
            {
                switch (discriminator)
                {
                    case _RaycastHit:
                        return raycastHit.transform;
                    case _Collision when collisionInfo.contactCount > 0:
                        return collisionInfo.contacts[0].otherCollider.transform;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
