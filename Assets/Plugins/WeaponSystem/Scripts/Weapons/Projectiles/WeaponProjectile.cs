using System;
using System.Collections.Generic;
using RabbitStewdio.Unity.GunShip.Game;
using RabbitStewdio.Unity.UnityTools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityTools;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    /// <summary>
    ///     A weapon projectile. This base class manages the common state of all
    ///     projectiles and ensures that pooling works correctly. Instead of working
    ///     with this class, you probably want to use
    ///     <see cref="ArtificialWeaponProjectile" /> or
    ///     <see cref="PhysicalWeaponProjectile" /> instances instead.
    /// </summary>
    /// <remarks>
    ///     Projectiles are activated by other game objects. To activate the
    ///     projectile call the <see cref="WeaponProjectile.Fire" /> method. This
    ///     method will always activate the muzzle flash effect of the projectile.
    ///     If a ray-cast predicts that the projectile would hit the target within
    ///     one frame this implementation will not activate the projectile body and
    ///     will directly activate the hit behaviour.
    ///
    ///     All effect game objects have must disable themselves after playing their
    ///     effects. The WeaponProjectile will delay returning to the pool until
    ///     all effects are finished playing, signaled by being disabled again.
    /// </remarks>
    public abstract class WeaponProjectile : MonoBehaviour, IHitDamageSource
    {
        /// <summary>
        ///     The current projectile state
        /// </summary>
        public enum WeaponProjectileState
        {
            /// <summary>
            ///     The projectile is not active.
            /// </summary>
            Expired,
            /// <summary>
            ///     The projectile is currently flying towards the target.
            /// </summary>
            Flying,
            /// <summary>
            ///     The projectile has hit something and is currently playing the
            ///     hit effect.
            /// </summary>
            Hit
        }

        /// <summary>
        ///     Defines how the projectile interacts with the target object and
        ///     nearby other targets during a collision.
        /// </summary>
        public enum WeaponHitBehaviour
        {
            /// <summary>
            ///     Only hits the target. This is equal to using a rifle bullet.
            /// </summary>
            Single,

            /// <summary>
            ///     Hits the area around the target.
            /// </summary>
            Area
        }

        static readonly RaycastHit[] raycastResults;
        static readonly Collider[] collisionResults;
        static readonly List<Collider> collectedColliders;
        static readonly List<Collider> collectedProjectileColliders;

        [FormerlySerializedAs("projectilePrefab")] 
        [SerializeField]
        GameObject projectileBody;
        
        [FormerlySerializedAs("muzzlePrefab")] 
        [SerializeField]
        GameObject muzzleFlashEffect;

        [FormerlySerializedAs("hitPrefab")] 
        [SerializeField]
        GameObject hitEffect;

        [Tooltip("The distance used during a forward looking raycast to detect whether the bullet has passed through small objects. If set to zero, an automatically computed value is used.")]
        [SerializeField] 
        float collisionDetectionDistance;
        
        [Tooltip("The bullets damage per hit. This hint will be passed on to the HitReceiver.")]
        [SerializeField] 
        float damagePerHit;
        
        [Tooltip("Defines how the projectile interacts with the target object and nearby other targets during a collision.")]
        [SerializeField] 
        WeaponHitBehaviour hitBehaviour;
        
        [Tooltip("Defines the damage radius for area effect projectiles.")]
        [FormerlySerializedAs("damageArea")]
        [SerializeField]
        float damageRadius;

        /// <summary>
        ///   An accurate timer that counts the seconds since this game object was fired using the <see cref="Time.time"/> measure.
        /// </summary>
        protected readonly ActivationTimer Timer;
        
        /// <summary>
        ///   An accurate timer that counts the seconds since this game object was fired using the <see cref="Time.fixedTime"/> measure.
        /// </summary>
        protected readonly ActivationTimer FixedUpdateTimer;

        LayerMask layerMask;
        int sourceId;
        bool isMuzzlePrefabNotNull;
        bool isHitPrefabNotNull;
        bool isProjectilePrefabNotNull;
        GameObject source;

        /// <summary>
        ///   Defines whether the projectile is affected by gravity.
        /// </summary>
        public abstract bool IsBallistic { get; }

        /// <summary>
        ///   The current projectile state.
        /// </summary>
        public WeaponProjectileState State { get; protected set; }

        /// <summary>
        ///   The projectile pool associated with this projectile. Common users of this library should never touch this property.
        /// </summary>
        internal WeaponProjectilePoolBehaviour Pool { get; set; }

        /// <summary>
        ///   The projectile's initial direction. Note that this is not updated during the flight.
        /// </summary>
        public Vector3 Direction { get; private set; }

        /// <summary>
        ///   The projectile's initial velocity. Note that this is not updated during the flight.
        /// </summary>
        public float Velocity { get; private set; }

        /// <summary>
        ///   The projectile's starting point. Note that this is not updated during the flight.
        /// </summary>
        public Vector3 Origin { get; private set; }

        /// <summary>
        ///   The projectile's forward looking ray sensor. This measure is used to raycast to the next frame's
        ///   position to detect possibly missed collisions while the projectile moves.
        /// </summary>
        public float RayLength { get; private set; }

        /// <summary>
        ///   The projectile's time to live at the start of the flight. This is defined in the weapon behaviour.
        /// </summary>
        public float TimeToLive { get; set; }

        /// <summary>
        ///   The bullets damage per hit. This hint will be passed on to the HitReceiver.
        /// </summary>
        public float DamagePerHit => damagePerHit;

        /// <summary>
        ///   The current game object of this projectile. Exposed as part of the <see cref="IHitDamageSource"/> interface.
        /// </summary>
        public GameObject GameObject => gameObject;

        /// <summary>
        ///   Defines whether the hit game object is currently playing.
        /// </summary>
        public bool HitEffectActive
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

        /// <summary>
        ///   Defines whether the muzzle fire game object is currently playing.
        /// </summary>
        public bool MuzzleEffectActive
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

        /// <summary>
        ///   Defines whether the projectile body game object is currently playing.
        /// </summary>
        public bool ProjectileBodyActive
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
            Timer = new ActivationTimer();
            FixedUpdateTimer = new ActivationTimer();
        }

        static WeaponProjectile()
        {
            raycastResults = new RaycastHit[256];
            collisionResults = new Collider[256];
            collectedColliders = new List<Collider>();
            collectedProjectileColliders = new List<Collider>();
        }

        /// <summary>
        ///    Unity Event Callback; Internal so that overrides are detected by the compiler.
        /// </summary>
        internal void Awake()
        {
            if (projectileBody != null)
            {
                isProjectilePrefabNotNull = true;
            }

            if (hitEffect != null)
            {
                isHitPrefabNotNull = true;
                if (hitEffect.GetComponent<DisableWhenEffectsFinished>() == null)
                {
                    hitEffect.AddComponent<DisableWhenEffectsFinished>();
                }
            }

            if (muzzleFlashEffect != null)
            {
                isMuzzlePrefabNotNull = true;
                if (muzzleFlashEffect.GetComponent<DisableWhenEffectsFinished>() == null)
                {
                    muzzleFlashEffect.AddComponent<DisableWhenEffectsFinished>();
                }
            }

            AwakeOverride();
        }

        /// <summary>
        ///   Unity event callback override point.
        /// </summary>
        protected virtual void AwakeOverride() { }

        /// <summary>
        ///    Unity Event Callback; Internal so that overrides are detected by the compiler.
        /// </summary>
        internal void OnEnable()
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

            OnEnableOverride();
        }

        /// <summary>
        ///   Unity event callback override point.
        /// </summary>
        protected virtual void OnEnableOverride() {}


        /// <summary>
        ///    Unity Event Callback; Internal so that overrides are detected by the compiler.
        /// </summary>
        internal void Update()
        {
            switch (State)
            {
                case WeaponProjectileState.Expired:
                {
                    return;
                }
                case WeaponProjectileState.Flying:
                {
                    Timer.Update();
                    if (Timer.TimePassed > TimeToLive)
                    {
                        TimeToLive = 0;
                        State = WeaponProjectileState.Expired;
                        OnProjectileFinished(true);
                    }
                    else
                    {
                        OnUpdateFlyingOverride();
                    }

                    break;
                }
                case WeaponProjectileState.Hit:
                {
                    if (isHitPrefabNotNull)
                    {
                        if (!IsPlayingEffects())
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

        /// <summary>
        ///   Unity event callback override point. This is only called while the project is still flying.
        /// </summary>
        protected virtual void OnUpdateFlyingOverride()
        {
        }

        /// <summary>
        ///    Unity Event Callback; Internal so that overrides are detected by the compiler.
        /// </summary>
        internal void FixedUpdate()
        {
            if (State != WeaponProjectileState.Flying)
            {
                return;
            }

            FixedUpdateTimer.FixedUpdate();
            FixedUpdateOverride();
        }

        /// <summary>
        ///   Unity event callback override point.
        /// </summary>
        protected virtual void FixedUpdateOverride()
        {
        }

        /// <summary>
        ///   Checks whether any of the effect objects are still active.
        /// </summary>
        /// <returns>true if effects are still playing, false otherwise.</returns>
        protected virtual bool IsPlayingEffects()
        {
            return (isHitPrefabNotNull && hitEffect.activeSelf) || (isMuzzlePrefabNotNull && muzzleFlashEffect.activeSelf);
        }

        /// <summary>
        ///   Fires the projectile from the <see cref="origin"/> position towards <see cref="directionAndVelocity"/>. 
        /// </summary>
        /// <param name="source">`The source object that launched the projectile. This will be used to avoid collisions with this object during launch and passed on to the hit-receiver when the projectile hits a target</param>
        /// <param name="origin">The starting point of the projectiles journey</param>
        /// <param name="directionAndVelocity">The initial direction and velocity of the projectile.</param>
        /// <param name="timeToLive">The maximum time the projectile will remain active.</param>
        /// <param name="mask">The projectile's collision mask</param>
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

            Timer.Start();
            FixedUpdateTimer.StartFixed();

            State = WeaponProjectileState.Flying;
            PlayMuzzleEffect();

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
                ProjectileBodyActive = true;
            }
        }

        /// <summary>
        ///   Plays the muzzle effect. If the effect has a ProjectileHitBehaviour this will be
        ///   configured to follow the source object.
        /// </summary>
        protected void PlayMuzzleEffect()
        {
            if (isMuzzlePrefabNotNull)
            {
                if (muzzleFlashEffect.TryGetComponent<ProjectileHitBehaviour>(out var hit))
                {
                    hit.OnHit(source.transform);
                }

                muzzleFlashEffect.SetActive(true);
            }
        }

        /// <summary>
        ///   Tests whether the projectile collides with other objects in the next frame. This base
        ///   implementation uses a forward looking raycast to detect collisions.
        /// </summary>
        /// <param name="origin">The origin point for the collision check.</param>
        /// <param name="target">The target point for the collision check</param>
        /// <param name="hit">the first hit position.</param>
        /// <returns>true if the check hits something, false otherwise.</returns>
        protected virtual bool CheckCollision(Vector3 origin, Vector3 target, out HitInformation hit)
        {
            var directionVector = (target - origin);
            var count = Physics.RaycastNonAlloc(origin,
                                                directionVector,
                                                raycastResults,
                                                RayLength,
                                                layerMask,
                                                QueryTriggerInteraction.Ignore);
            try
            {
                for (var i = 0; i < count; i++)
                {
                    var hitTest = new HitInformation(raycastResults[i]);
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
                Array.Clear(raycastResults, 0, count);
            }
        }

        /// <summary>
        ///     Ignores collisions with the <see cref="source" /> object.
        /// </summary>
        /// <param name="hit">the hit information that should be validated</param>
        /// <returns>true if the hit is valid, false if the hit should be ignored.</returns>
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

        /// <summary>
        ///   Callback when something has been hit. This will deactivate the body and
        ///   will activate the hit effect calling <see cref="OnProjectileCollision"/>.
        /// </summary>
        /// <param name="hit"></param>
        protected void PerformHit(HitInformation hit)
        {
            ProjectileBodyActive = false;
            HitEffectActive = true;
            TimeToLive = 0;
            State = WeaponProjectileState.Hit;
            OnProjectileCollision(hit);
        }

        /// <summary>
        ///   Adjusts the projectile so that it is aligned with the normal position of the hit.
        ///   This ensures that any animation played will look as if it sits straight on the
        ///   target object's surface.
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void OnProjectileCollision(HitInformation hit)
        {
            var transform = this.transform;
            transform.position = hit.point;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal);

            ApplyHitLocation(hit.transform, hit.point);
        }

        /// <summary>
        ///   Event handler that is called when the projectile is going to be reclaimed by the pool.
        /// </summary>
        /// <param name="timeout">indicate whether the projectile has run out of time before hitting something.</param>
        protected virtual void OnProjectileFinished(bool timeout)
        {
            IgnoreFiredBy(source, false);

            HitEffectActive = false;
            MuzzleEffectActive = false;
            ProjectileBodyActive = false;
            gameObject.SetActive(false);
            source = null;
            Pool.Release(this);
        }
        
        /// <summary>
        ///   Reconfigures the physics system to ignore or allow collisions between this object and the object given.
        /// </summary>
        /// <param name="o">The other object</param>
        /// <param name="ignore">a flag indicating whether collisions between this object and the given object are allowed.</param>
        protected void IgnoreFiredBy(GameObject o, bool ignore = true)
        {
            sourceId = o.GetInstanceID();
            var selfColliders = gameObject.GetComponentsInChildrenNonAlloc(collectedProjectileColliders, true, true);
            if (selfColliders.Count == 0)
            {
                return;
            }

            if (o.TryGetComponent<CompositeColliderCollector>(out var otherComposite))
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

            var otherCollider = o.GetComponentsInChildrenNonAlloc(collectedColliders, true);
            foreach (var selfCollider in selfColliders)
            {
                foreach (var c in otherCollider)
                {
                    Physics.IgnoreCollision(selfCollider, c, ignore);
                }
            }
        }

        /// <summary>
        ///   Processes a hit at the given location with the given target as primary target.
        /// </summary>
        /// <param name="other">The game object that has been hit</param>
        /// <param name="hitPoint">The exact hit point.</param>
        protected void ApplyHitLocation(Transform other, Vector3 hitPoint)
        {
            if (isHitPrefabNotNull)
            {
                if (hitEffect.TryGetComponent<ProjectileHitBehaviour>(out var hit))
                {
                    hit.OnHit(other);
                }
            }

            if (hitBehaviour != WeaponHitBehaviour.Single && damageRadius > 0)
            {
                var results = Physics.OverlapSphereNonAlloc(hitPoint, damageRadius, collisionResults, layerMask, QueryTriggerInteraction.Ignore);
                for (var result = 0; result < results; result += 1)
                {
                    var hit = collisionResults[result];
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
    }

    /// <summary>
    ///   A union structure that unifies RayCast hits and Collider-hits.
    /// </summary>
    public readonly struct HitInformation
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

        /// <summary>
        ///  The impact point in world space where the raycast or collider has hit.
        /// </summary>
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

        /// <summary>
        ///  The normal of the surface where the raycast or collider has hit.
        /// </summary>
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

        /// <summary>
        ///  The transform of the game object where the raycast or collider has hit.
        /// </summary>
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
