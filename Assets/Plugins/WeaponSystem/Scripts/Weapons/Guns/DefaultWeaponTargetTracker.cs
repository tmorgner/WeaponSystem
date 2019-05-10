using System;
using System.Collections.Generic;
using NaughtyAttributes;
using RabbitStewdio.Unity.UnityTools;
using RabbitStewdio.Unity.UnityTools.SmartVars.RuntimeSets;
using UnityEngine;
using UnityEngine.Serialization;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A default implementation of a weapon target tracker. This class is
    ///   responsible for tracking potential targets via a trigger collider and
    ///   for selecting the best target out of that set of targets.
    ///
    ///   If the aiming mode is set to <see cref="TargetTrackerBase.AimingMode.Manual"/> this
    ///   tracker will still act as if targeting is assisted, and it is up to
    ///   the caller to disregard unsuitable information. 
    /// </summary>
    public class DefaultWeaponTargetTracker : TargetTrackerBase
    {
        [Required]
        [SerializeField]
        WeaponDefinition weaponDefinition;

        [Required]
        [SerializeField]
        Collider trackingCollider;

        [FormerlySerializedAs("rigidbody")]
        [SerializeField]
        Rigidbody monitoredRigidbody;

        Rigidbody effectiveRigidbody;
        TriggerEventForwarder eventForward;

        public WeaponDefinition WeaponDefinition => weaponDefinition;

        protected virtual bool AdjustColliders => true;

        /// <summary>
        ///   Unity event callback. Validates the tracking collider and if the collider is a box or sphere
        ///   makes sure that the collider is large enough to cover the tracking area.
        /// </summary>
        protected sealed override void AwakeOverride()
        {
            base.AwakeOverride();

            if (AdjustColliders && WeaponDefinition)
            {
                if (trackingCollider is SphereCollider targetArea)
                {
                    var maximumTrackingRange = WeaponDefinition.MaximumTrackingRange;
                    targetArea.center = new Vector3(0, 0, maximumTrackingRange / 2);
                    targetArea.radius = maximumTrackingRange / 2;
                }
                else if (trackingCollider is BoxCollider boxedTargetArea)
                {
                    var maximumTrackingRange = WeaponDefinition.MaximumTrackingRange;
                    boxedTargetArea.center = new Vector3(0, 0, maximumTrackingRange / 2);
                    boxedTargetArea.size = Vector3.one * maximumTrackingRange / 2;
                }
                else if (trackingCollider != null)
                {
                    var maximumTrackingRange = WeaponDefinition.MaximumTrackingRange;
                    var bounds = trackingCollider.bounds;
                    if (bounds.size.x >= maximumTrackingRange && bounds.size.y >= maximumTrackingRange && bounds.size.z >= maximumTrackingRange)
                    {
                        Debug.Log("Non-primitive collider used for tracking. Please make sure your collider covers the target area defined in the weapon definition.");
                    }
                }
            }

            AwakeOverride2();
        }

        protected virtual void AwakeOverride2()
        {
        }

        protected override void OnEnableOverride()
        {
            if (!effectiveRigidbody)
            {
                if (monitoredRigidbody)
                {
                    effectiveRigidbody = monitoredRigidbody;
                }
                else
                {
                    effectiveRigidbody = GetComponentInParent<Rigidbody>();
                }

                if (effectiveRigidbody.gameObject != gameObject)
                {
                    eventForward = effectiveRigidbody.gameObject.AddComponent<TriggerEventForwarder>();
                }
            }

            if (weaponDefinition && weaponDefinition.TargetSet)
            {
                weaponDefinition.TargetSet.OnEntryRemoved += OnEntryRemoved;
            }

            if (eventForward)
            {
                eventForward.TriggerEnter += OnTriggerEnter;
                eventForward.TriggerExit += OnTriggerExit;
            }
        }

        void OnDisable()
        {
            if (eventForward)
            {
                eventForward.TriggerEnter -= OnTriggerEnter;
                eventForward.TriggerExit -= OnTriggerExit;
                Destroy(eventForward);
            }

            if (WeaponDefinition && WeaponDefinition.TargetSet)
            {
                WeaponDefinition.TargetSet.OnEntryRemoved -= OnEntryRemoved;
            }
        }

        internal void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (!rb)
            {
                return;
            }

            OnEntryAdded(rb);
        }

        internal void OnTriggerExit(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (!rb)
            {
                return;
            }

            OnEntryRemoved(rb, false);
        }

        protected override float TargetConeExtend => (weaponDefinition) ? weaponDefinition.TargetCone : 0;
        public override float MaximumTargetRange => (weaponDefinition) ? weaponDefinition.MaximumTrackingRange : 0;
        protected override float ProjectileSpeed => (weaponDefinition) ? weaponDefinition.ProjectileSpeed : 0;
        protected override float AdditionalDelay => (weaponDefinition) ? weaponDefinition.FireDelay : 0;

        protected override LayerMask VisibilityLayerMask
        {
            get
            {
                if (weaponDefinition)
                {
                    return weaponDefinition.InteractWith;
                }

                return -1;
            }
        }

        protected override bool IsIgnored(Rigidbody hitRigidbody)
        {
            if (!hitRigidbody)
            {
                return true;
            }

            if (effectiveRigidbody)
            {
                return hitRigidbody == effectiveRigidbody;
            }

            return false;
        }

        protected override ICollection<Rigidbody> TargetSet => weaponDefinition.TargetSet.Values;

        protected override Rigidbody FindNearestTarget()
        {
            var targetSet = weaponDefinition.TargetSet;
            if (targetSet && targetSet.Count < 5)
            {
                trackingCollider.enabled = false;
                return TrackTargets(weaponDefinition.TargetSet.Values);
            }
            else
            {
                trackingCollider.enabled = true;
                return base.FindNearestTarget();
            }
        }
    }
}
