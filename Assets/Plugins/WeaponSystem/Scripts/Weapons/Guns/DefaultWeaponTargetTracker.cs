using System.Collections.Generic;
using NaughtyAttributes;
using RabbitStewdio.Unity.UnityTools;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A default implementation of a weapon target tracker. This class is responsible for
    ///   tracking potential targets via a trigger collider and for selecting the best target
    ///   out of that set of targets.
    ///
    ///   If the aiming mode is set to <see cref="AimingMode.Manual"/> this tracker will still
    ///   act as if targeting is assisted, and it is up to the caller to disregard unsuitable
    ///   information. 
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DefaultWeaponTargetTracker : WeaponTargetTracker, ITargetSelectionInformation
    {
        /// <summary>
        ///  The aiming mode for the tracker.
        /// </summary>
        public enum AimingMode
        {
            /// <summary>
            ///   Attempts to aim as closely as possible to the current view direction.
            /// </summary>
            Assisted = 0,
            Automatic = 1,
            Manual = 2
        }

        [Required]
        [SerializeField] 
        TargetSelectionStrategy targetSelectionStrategy;

        [Required]
        [SerializeField] 
        Collider trackingCollider;

        [SerializeField] 
        Transform predictionSource;

        [Required]
        [SerializeField] 
        WeaponDefinition weaponDefinition;

        [SerializeField] 
        AimingMode aimingMode;

        readonly List<Rigidbody> trackedTargets;
        float targetConeExtent;
        Rigidbody trackedTarget;
        Rigidbody body;
        IAimingMeasure aiming;
        static readonly RaycastHit[] hits = new RaycastHit[2];

        public DefaultWeaponTargetTracker()
        {
            trackedTargets = new List<Rigidbody>(16);
        }

        public WeaponDefinition WeaponDefinition => weaponDefinition;

        /// <summary>
        ///   Returns the current aiming mode.
        /// </summary>
        public AimingMode Mode => aimingMode;

        /// <summary>
        ///  Returns whether this tracker has a target. If there is no valid target this will attempt
        ///  to find one before returning.
        /// </summary>
        public override bool HasTarget
        {
            get
            {
                RevalidateTrackedTarget();
                return trackedTarget != null && trackedTarget.gameObject.activeInHierarchy;
            }
        }

        protected Rigidbody RevalidateTrackedTarget()
        {
            if (trackedTarget == null || !trackedTarget.gameObject.activeInHierarchy)
            {
                trackedTarget = FindNearestTarget();
            }

            return trackedTarget;
        }

        /// <summary>
        ///   Attempts to predict the target position towards the currently selected target.
        /// </summary>
        /// <param name="distance">The computed distance to the target.</param>
        /// <param name="position">the predicted target position</param>
        /// <returns></returns>
        public override bool PredictCurrentTargetPosition(out float distance, out Vector3 position)
        {
            if (trackedTarget != null)
            {
                return PredictTargetPosition(trackedTarget, false, out distance, out position);
            }

            position = default;
            distance = 0;
            return false;
        }

        /// <summary>
        ///   Unity event callback. Validates the tracking collider and if the collider is a box or sphere
        ///   makes sure that the collider is large enough to cover the tracking area.
        /// </summary>
        internal void Awake()
        {
            body = GetComponent<Rigidbody>();
            targetConeExtent = Mathf.Cos(weaponDefinition.TargetCone * Mathf.Deg2Rad);

            if (predictionSource == null)
            {
                predictionSource = transform;
            }

            if (trackingCollider is SphereCollider targetArea)
            {
                var maximumTrackingRange = weaponDefinition.MaximumTrackingRange;
                targetArea.center = new Vector3(0, 0, maximumTrackingRange / 2);
                targetArea.radius = maximumTrackingRange / 2;
            }
            else if (trackingCollider is BoxCollider boxedTargetArea)
            {
                var maximumTrackingRange = weaponDefinition.MaximumTrackingRange;
                boxedTargetArea.center = new Vector3(0, 0, maximumTrackingRange / 2);
                boxedTargetArea.size = Vector3.one * maximumTrackingRange / 2;
            }
            else if (trackingCollider != null)
            {
                var maximumTrackingRange = weaponDefinition.MaximumTrackingRange;
                var bounds = trackingCollider.bounds;
                if (bounds.size.x >= maximumTrackingRange && bounds.size.y >= maximumTrackingRange && bounds.size.z >= maximumTrackingRange)
                {
                    Debug.Log("Non-primitive collider used for tracking. Please make sure your collider covers the target area defined in the weapon definition.");
                }
            }

            if (aimingMode == AimingMode.Automatic)
            {
                aiming = new AutoAimingMeasure();
            }
            else
            {
                aiming = new AimAssistMeasure(predictionSource);
            }

            AwakeOverride();
        }

        /// <summary>
        ///   Extension point for sub classes.
        /// </summary>
        protected virtual void AwakeOverride()
        {
        }

        internal void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (!rb)
            {
                return;
            }

            var targetSet = weaponDefinition.TargetSet;
            if (targetSet == null || targetSet.Contains(rb))
            {
                trackedTargets.Add(rb);
            }
        }

        internal void OnTriggerExit(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (!rb)
            {
                return;
            }

            var targetSet = weaponDefinition.TargetSet;
            if (targetSet == null || targetSet.Contains(rb))
            {
                trackedTargets.Remove(rb);
            }
        }

        public float MaximumTargetRange => weaponDefinition.MaximumTrackingRange;

        bool ITargetSelectionInformation.PredictTargetPosition(Rigidbody possibleTarget, out float distance, out Vector3 position)
        {
            return PredictTargetPosition(possibleTarget, true, out distance, out position);
        }

        /// <summary>
        /// <para>
        /// Attempts to predict the target <paramref name="position"/> where the
        /// gun would have to shoot at to hit the target assuming it maintains
        /// course and speed. If <paramref name="targetSelection"/> is true,
        /// this will take the initial charge into account.
        /// </para>
        /// <para>
        /// This method returns <see langword="false"/> if the target would be
        /// outside of the tracking range when the projectile hits.
        /// </para>
        /// </summary>
        /// <param name="possibleTarget">
        /// The possible target that should be shot at
        /// </param>
        /// <param name="targetSelection">
        /// a flag indicating whether this prediction should account for weapon
        /// charge time
        /// </param>
        /// <param name="distance">
        /// the resulting distance to the firing spot
        /// </param>
        /// <param name="position">the future target position</param>
        /// <returns>
        /// <see langword="true"/> if the target can be fired within the current
        /// tracking range, <see langword="false"/> otherwise.
        /// </returns>
        protected bool PredictTargetPosition(Rigidbody possibleTarget, bool targetSelection, out float distance, out Vector3 position)
        {
            var rayOrigin = predictionSource.position;

            var targetRay = possibleTarget.position - rayOrigin;
            if (targetRay.magnitude > weaponDefinition.MaximumTrackingRange)
            {
                // Not in range
                position = Vector3.zero;
                distance = 0;
                return false;
            }

            var timeToTarget = targetRay.magnitude / weaponDefinition.ProjectileSpeed;
            if (targetSelection)
            {
                timeToTarget += weaponDefinition.FireDelay;
            }

            var futureTargetPosition = possibleTarget.position + possibleTarget.velocity * timeToTarget;

            // check whether the predicted position for the target is within range.
            var targetDirection = Vector3.Normalize(futureTargetPosition - rayOrigin);
            var cosineAngle = Vector3.Dot(targetDirection, transform.forward);
            if (cosineAngle < targetConeExtent)
            {
                // Not in range
                position = Vector3.zero;
                distance = 0;
                return false;
            }

            position = futureTargetPosition;
            distance = Vector3.Distance(futureTargetPosition, rayOrigin);
            return distance < weaponDefinition.MaximumTrackingRange;
        }

        Rigidbody FindNearestTarget()
        {
            var targetSet = weaponDefinition.TargetSet;
            if (targetSet && targetSet.Count < 5)
            {
                trackingCollider.enabled = false;
                return TrackViaTargetSet();
            }
            else
            {
                trackingCollider.enabled = true;
                return TrackViaCollider();
            }
        }

        Rigidbody TrackViaTargetSet()
        {
            aiming.Reset();
            targetSelectionStrategy.SelectTargets(aiming, this, weaponDefinition.TargetSet);
            return aiming.Result;
        }

        /// <inheritdoc />
        bool ITargetSelectionInformation.IsVisible(Rigidbody possibleTarget)
        {
            var predictionSourcePosition = predictionSource.position;
            var direction = possibleTarget.position - predictionSourcePosition;
            var count = Physics.RaycastNonAlloc(predictionSourcePosition,
                                                direction,
                                                hits,
                                                direction.magnitude + 0.5f,
                                                weaponDefinition.InteractWith);
            if (count > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.rigidbody == body)
                    {
                        continue;
                    }

                    if (hit.rigidbody == possibleTarget)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        Rigidbody TrackViaCollider()
        {
            if (trackedTargets.Count == 0)
            {
                return null;
            }

            aiming.Reset();
            targetSelectionStrategy.SelectTargets(aiming, this, trackedTargets);
            return aiming.Result;
        }

        void OnDrawGizmosSelected()
        {
            if (weaponDefinition != null)
            {
                var target = (trackingCollider != null) ? trackingCollider.transform : transform;
                GameObjectTools.DrawCone(target, weaponDefinition.TargetCone, weaponDefinition.MaximumTrackingRange);
            }
        }

        /// <summary>
        ///   Clears the currently tracked target. Use this to force a new search for a potentially better target.
        /// </summary>
        public override void ResetTarget()
        {
            aiming.Reset();
            trackedTarget = null;
        }
    }
}
