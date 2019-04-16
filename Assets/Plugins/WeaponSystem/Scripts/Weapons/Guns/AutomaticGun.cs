using System.Collections.Generic;
using RabbitStewdio.Unity.WeaponSystem.Weapons.Guns;
using UnityEngine;
using UnityEngine.Events;

namespace Plugins.WeaponSystem.Scripts.Weapons.Guns
{
    public class AutomaticGun : GunBase
    {
        [SerializeField] List<GameObject> barrels;
        [SerializeField] GameObject pivotObject;
        [SerializeField] UnityEvent beginIdleEvent;
        [SerializeField] UnityEvent beginChargingEvent;
        [SerializeField] UnityEvent fireEvent;

        public UnityEvent BeginIdleEvent => beginIdleEvent;

        public UnityEvent BeginChargingEvent => beginChargingEvent;

        public UnityEvent FireEvent => fireEvent;

        protected IReadOnlyList<GameObject> Barrels => barrels;

        protected override Transform WeaponTargetTrackerOrigin => pivotObject.transform;

        protected virtual void Awake()
        {
            if (pivotObject == null)
            {
                pivotObject = new GameObject("Pivot");
                pivotObject.transform.position = ComputeBarrelCenterPosition();
                pivotObject.transform.rotation = ComputeBarrelRotation();
                pivotObject.transform.SetParent(this.transform, true);
            }
        }

        protected override void BeginIdleOverride()
        {
            beginIdleEvent.Invoke();
        }

        protected override float BeginChargingOverride()
        {
             beginChargingEvent.Invoke();
            return base.BeginChargingOverride();
        }

        Vector3 ComputeBarrelCenterPosition()
        {
            if (barrels.Count == 0)
            {
                return transform.position;
            }

            var centre = new Vector3();
            foreach (var b in barrels)
            {
                centre += b.transform.position;
            }

            centre.x /= barrels.Count;
            centre.y /= barrels.Count;
            centre.z /= barrels.Count;
            return centre;
        }

        /// <summary>
        ///   Based on <see href="http://wiki.unity3d.com/index.php/Averaging_Quaternions_and_Vectors">a Unity Wiki post.</see>
        /// </summary>
        /// <returns>An average of all barrel rotations</returns>
        Quaternion ComputeBarrelRotation()
        {
            Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount)
            {
                //Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
                //q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
                if (!AreQuaternionsClose(newRotation, firstRotation))
                {
                    newRotation = InverseSignQuaternion(newRotation);
                }

                //Average the values
                var addDet = 1f / (float) addAmount;
                cumulative.w += newRotation.w;
                var w = cumulative.w * addDet;
                cumulative.x += newRotation.x;
                var x = cumulative.x * addDet;
                cumulative.y += newRotation.y;
                var y = cumulative.y * addDet;
                cumulative.z += newRotation.z;
                var z = cumulative.z * addDet;

                //note: if speed is an issue, you can skip the normalization step
                return NormalizeQuaternion(x, y, z, w);
            }

            Quaternion NormalizeQuaternion(float x, float y, float z, float w)
            {
                var lengthD = 1.0f / (w * w + x * x + y * y + z * z);
                w *= lengthD;
                x *= lengthD;
                y *= lengthD;
                z *= lengthD;

                return new Quaternion(x, y, z, w);
            }

            //Changes the sign of the quaternion components. This is not the same as the inverse.
            Quaternion InverseSignQuaternion(Quaternion q)
            {
                return new Quaternion(-q.x, -q.y, -q.z, -q.w);
            }

            //Returns true if the two input quaternions are close to each other. This can
            //be used to check whether or not one of two quaternions which are supposed to
            //be very similar but has its component signs reversed (q has the same rotation as
            //-q)
            bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
            {
                var dot = Quaternion.Dot(q1, q2);
                return !(dot < 0.0f);
            }

            if (barrels.Count == 0)
            {
                return transform.rotation;
            }

            var cumulativeVector = new Vector4();
            var firstBarrelRotation = barrels[0].transform.rotation;
            var result = firstBarrelRotation;
            for (var c = 1; c < barrels.Count; c += 1)
            {
                result = AverageQuaternion(ref cumulativeVector, barrels[c].transform.rotation, firstBarrelRotation, c);
            }

            return result;
        }

        protected override void BeginFireOverride(Vector3 fireTarget)
        {
            fireEvent.Invoke();
             foreach (var b in barrels)
            {
                DoFire(b.transform.position, fireTarget);
            }
        }
    }
}
