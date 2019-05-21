using System.Collections.Generic;
using NaughtyAttributes;
using RabbitStewdio.Unity.UnityTools;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///  All the common target tracking code without a tight coupling to a weapon definition.
    /// </summary>
    public abstract class TargetTrackerBase : WeaponTargetTracker, ITargetSelectionInformation
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

        protected static readonly RaycastHit[] hits = new RaycastHit[4];

        [SerializeField] bool drawGizmos;
        
        [Required]
        [SerializeField]
        TargetSelectionStrategy targetSelectionStrategy;

        [SerializeField]
        Transform predictionSource;

        [SerializeField]
        AimingMode aimingMode;

        IAimingMeasure aiming;
        
        [ShowNonSerializedField]
        Rigidbody trackedTarget;



        readonly OrderedDictionary<Rigidbody, int> trackedBodies;

        protected TargetTrackerBase()
        {
            trackedBodies = new OrderedDictionary<Rigidbody, int>();
        }

        protected OrderedDictionary<Rigidbody, int> TrackedBodies => trackedBodies;

        protected Transform PredictionSource => predictionSource;

        /// <summary>
        ///  Returns whether this tracker has a target. If there is no valid target this will attempt
        ///  to find one before returning.
        /// </summary>
        public override bool HasTarget
        {
            get
            {
                RevalidateTrackedTarget();
                return trackedTarget && trackedTarget.gameObject.activeInHierarchy;
            }
        }

        protected Rigidbody RevalidateTrackedTarget()
        {
            if (trackedTarget && 
                trackedTarget.gameObject.activeInHierarchy && 
                IsValidTarget(trackedTarget))
            {
                return trackedTarget;
            }

            trackedTarget = FindNearestTarget();
            return trackedTarget;
        }

        protected Rigidbody TrackTargets(ReadOnlyListWrapper<Rigidbody> targetSet)
        {
            aiming.Reset();
            targetSelectionStrategy.SelectTargets(aiming, this, targetSet);
            return aiming.Result;
        }

        /// <summary>
        ///   Attempts to predict the target position towards the currently selected target.
        /// </summary>
        /// <param name="distance">The computed distance to the target.</param>
        /// <param name="position">the predicted target position</param>
        /// <returns></returns>
        public override bool PredictCurrentTargetPosition(out float distance, out Vector3 position)
        {
            if (trackedTarget)
            {
                return PredictTargetPosition(trackedTarget, false, out distance, out position);
            }

            this.Log("No Current Target");
            position = default;
            distance = 0;
            return false;
        }

        protected override void AwakeOverride()
        {
            if (predictionSource == null)
            {
                predictionSource = transform;
            }

            if (aimingMode == AimingMode.Automatic)
            {
                aiming = new AutoAimingMeasure();
            }
            else
            {
                aiming = new AimAssistMeasure(PredictionSource);
            }
        }

        protected void OnEntryRemoved(Rigidbody rb) => OnEntryRemoved(rb, true);

        protected void OnEntryRemoved(Rigidbody rb, bool force)
        {
            if (trackedBodies.TryGetValue(rb, out var refCount))
            {
                if (refCount <= 1)
                {
                    trackedBodies.Remove(rb);
                    OnTargetRemoved(rb);
                    return;
                }

                if (!force)
                {
                    trackedBodies[rb] = refCount - 1;
                }
            }

            // just in case
            if (force && trackedBodies.Remove(rb))
            {
                OnTargetRemoved(rb);
            }
        }

        protected abstract bool IsValidTarget(Rigidbody body);

        protected virtual void OnTargetRemoved(Rigidbody rb) { }

        protected void OnEntryAdded(Rigidbody rb)
        {
            if (IsValidTarget(rb))
            {
                if (trackedBodies.TryGetValue(rb, out var refCount))
                {
                    trackedBodies[rb] = refCount + 1;
                }
                else
                {
                    trackedBodies[rb] = 1;
                    OnTargetAdded(rb);
                }
            }
            else
            {
                this.Log("Rejected " + rb + " as it is not part of target set");
            }
        }

        protected virtual void OnTargetAdded(Rigidbody rb) { }

        protected abstract float TargetConeExtend { get; }

        public abstract float MaximumTargetRange { get; }

        protected abstract float ProjectileSpeed { get; }
        protected abstract float AdditionalDelay { get; }
        protected abstract LayerMask VisibilityLayerMask { get; }

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
            if (targetRay.magnitude > MaximumTargetRange)
            {
                // Not in range
                position = Vector3.zero;
                distance = 0;
                return false;
            }

            var timeToTarget = targetRay.magnitude / ProjectileSpeed;
            if (targetSelection)
            {
                timeToTarget += AdditionalDelay;
            }

            var actualPosition = GetActualPosition(possibleTarget);
            var futureTargetPosition = actualPosition + possibleTarget.velocity * timeToTarget;
  
            // check whether the predicted position for the target is within range.
            var targetDirection = Vector3.Normalize(futureTargetPosition - rayOrigin);
            var cosineAngle = Vector3.Dot(targetDirection, transform.forward);
            var targetConeExtendsCos = Mathf.Cos(TargetConeExtend * Mathf.Deg2Rad);
            if (cosineAngle < targetConeExtendsCos)
            {
                // Not in range
                position = Vector3.zero;
                distance = 0;
                return false;
            }

            position = futureTargetPosition;
            distance = Vector3.Distance(futureTargetPosition, rayOrigin);
            return distance < MaximumTargetRange;
        }

        Vector3 GetActualPosition(Rigidbody possibleTarget)
        {
            Vector3 actualPosition;
            var marker = possibleTarget.GetComponentInChildren<HitAreaMarker>();
            if (marker)
            {
                actualPosition = marker.transform.position;
            }
            else
            {
                actualPosition = possibleTarget.ClosestPointOnBounds(predictionSource.position);
            }

            return actualPosition;
        }

        /// <inheritdoc />
        bool ITargetSelectionInformation.IsVisible(Rigidbody possibleTarget)
        {
            return IsVisible(possibleTarget);
        }

        protected bool IsVisible(Rigidbody possibleTarget)
        {
            var predictionSourcePosition = predictionSource.position;
            var actualPosition = GetActualPosition(possibleTarget);
            var direction = actualPosition - predictionSourcePosition;
            var count = Physics.RaycastNonAlloc(predictionSourcePosition,
                                                direction,
                                                hits,
                                                direction.magnitude + 1.1f,
                                                VisibilityLayerMask);
            if (count > 0)
            {
                foreach (var hit in hits)
                {
                    if (IsIgnored(hit.rigidbody))
                    {
                        continue;
                    }

                    if (hit.rigidbody == possibleTarget)
                    {
                        return true;
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        ///   Returns the current aiming mode.
        /// </summary>
        public AimingMode Mode => aimingMode;

        protected abstract bool IsIgnored(Rigidbody hitRigidbody);

        protected virtual Rigidbody FindNearestTarget()
        {
            var bodiesTracked = this.TrackedBodies;
            if (bodiesTracked.Count == 0)
            {
                return null;
            }

            return TrackTargets(bodiesTracked.Keys);
        }

        /// <summary>
        ///   Clears the currently tracked target. Use this to force a new search for a potentially better target.
        /// </summary>
        public override void ResetTarget()
        {
            aiming.Reset();
            trackedTarget = null;
        }

        void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
            {
                return;
            }
            
            var targetRange = 0f;
            var targetCone = 0f;
            if (MaximumTargetRange <= 0)
            {
                Gizmos.color = Color.red;
                targetRange = 100;
                targetCone = 45f;
            }
            else
            {
                Gizmos.color = Color.white;
                targetRange = MaximumTargetRange;
                targetCone = TargetConeExtend;
            }

            var target = (PredictionSource) ? PredictionSource.transform : transform;
            GameObjectTools.DrawCone(target, targetCone, targetRange);
        }
    }
}
