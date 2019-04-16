using System.Collections.Generic;
using UnityEngine;
using UnityTools;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public abstract class WeaponTargetTracker : MonoBehaviour
    {
        public abstract bool HasTarget { get; }
        public abstract float PredictPosition(out Vector3 predictedPosition);
        public abstract void ResetTarget();
    }

    [RequireComponent(typeof(Rigidbody))]
    public class DefaultWeaponTargetTracker : WeaponTargetTracker
    {
        public enum AimingMode
        {
            Assisted, Automatic, Manual
        }

        [SerializeField] Collider trackingCollider;
        [SerializeField] Transform predictionSource;
        [SerializeField] WeaponDefinition weaponDefinition;
        [SerializeField] AimingMode aimingMode;
        readonly List<Rigidbody> trackedTargets;
        float targetConeExtent;
        Rigidbody trackedTarget;
        Rigidbody body;
        IAimingMeasure aiming;
        static readonly RaycastHit[] hits = new RaycastHit[2];

        public DefaultWeaponTargetTracker()
        {
            trackedTargets = new List<Rigidbody>();
        }

        public AimingMode Mode => aimingMode;

        public override bool HasTarget
        {
            get
            {
                if (trackedTarget == null || !trackedTarget.gameObject.activeInHierarchy)
                {
                    trackedTarget = FindNearestTarget();
                }

                return trackedTarget != null && trackedTarget.gameObject.activeInHierarchy;
            }
        }

        public override float PredictPosition(out Vector3 position)
        {
            if (HasTarget)
            {
                return PredictPosition(trackedTarget, out position);
            }

            position = default;
            return float.MaxValue;
        }

        void Awake()
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

            if (aimingMode == AimingMode.Automatic)
            {
                aiming = new AutoAimingMeasure();
            }
            else
            {
                aiming = new AimAssistMeasure(predictionSource);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            var targetSet = weaponDefinition.TargetSet;
            if (rb != null && targetSet.Contains(rb))
            {
                trackedTargets.Add(rb);
            }
        }

        void OnTriggerExit(Collider other)
        {
            var rb = other.attachedRigidbody;
            var targetSet = weaponDefinition.TargetSet;
            if (rb != null && targetSet.Contains(rb))
            {
                trackedTargets.Remove(rb);
            }
        }


        float PredictPosition(Rigidbody possibleTarget, out Vector3 position)
        {
            var rayOrigin = predictionSource.position;

            var targetRay = possibleTarget.position - rayOrigin;
            if (targetRay.magnitude > weaponDefinition.MaximumTrackingRange)
            {
                // Not in range
                position = Vector3.zero;
                return float.MaxValue;
            }

            var timeToTarget = targetRay.magnitude / weaponDefinition.ProjectileSpeed;
            var futureTargetPosition = possibleTarget.position + possibleTarget.velocity * timeToTarget;

            // check whether the predicted position for the target is within range.
            var targetDirection = Vector3.Normalize(futureTargetPosition - rayOrigin);
            var cosineAngle = Vector3.Dot(targetDirection, transform.forward);
            if (cosineAngle < targetConeExtent)
            {
                // Not in range
                position = Vector3.zero;
                return float.MaxValue;
            }

            position = futureTargetPosition;
            return Vector3.Distance(futureTargetPosition, rayOrigin);
        }

        Rigidbody FindNearestTarget()
        {
            if (weaponDefinition.TargetSet.Count < 5)
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
            foreach (var possibleTarget in weaponDefinition.TargetSet)
            {
                var distance = PredictPosition(possibleTarget, out var predictedPosition);
                if (distance < weaponDefinition.MaximumTrackingRange && IsVisible(possibleTarget))
                {
                    aiming.Track(possibleTarget, distance, ref predictedPosition);
                }
            }

            return aiming.Result;
        }

        bool IsVisible(Rigidbody possibleTarget)
        {
            var direction = possibleTarget.position - predictionSource.position;
            var count = Physics.RaycastNonAlloc(predictionSource.position,
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

            foreach (var possibleTarget in trackedTargets)
            {
                var distance = PredictPosition(possibleTarget, out var predictedPosition);
                if (distance < weaponDefinition.MaximumTrackingRange && IsVisible(possibleTarget))
                {
                    aiming.Track(possibleTarget, distance, ref predictedPosition);
                }
            }

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

        public override void ResetTarget()
        {
            trackedTarget = null;
        }

        interface IAimingMeasure
        {
            void Reset();
            void Track(Rigidbody body, float distance, ref Vector3 target);
            Rigidbody Result { get; }
        }

        class AimAssistMeasure : IAimingMeasure
        {
            readonly Transform aimingSource;
            float alignment;
            Rigidbody body;

            public AimAssistMeasure(Transform aimingSource)
            {
                this.aimingSource = aimingSource;
                alignment = float.MinValue;
            }

            public void Reset()
            {
                alignment = float.MinValue;
            }

            /// <summary>
            ///  Select the target that is most closest to the current firing path.
            /// </summary>
            /// <param name="body"></param>
            /// <param name="distance"></param>
            /// <param name="target"></param>
            public void Track(Rigidbody body, float distance, ref Vector3 target)
            {
                var dot = Vector3.Dot(aimingSource.forward, (target - aimingSource.position).normalized);
                if (dot > this.alignment)
                {
                    this.body = body;
                    this.alignment = dot;
                }
            }

            public Rigidbody Result => body;
        }

        class AutoAimingMeasure: IAimingMeasure
        {
            float distance;
            Rigidbody body;

            public AutoAimingMeasure()
            {
                distance = float.MaxValue;
            }

            public void Reset()
            {
                distance = float.MaxValue;
            }

            public void Track(Rigidbody body, float distance, ref Vector3 target)
            {
                if (distance < this.distance)
                {
                    this.body = body;
                    this.distance = distance;
                }
            }

            public Rigidbody Result => body;
        }
    }
}