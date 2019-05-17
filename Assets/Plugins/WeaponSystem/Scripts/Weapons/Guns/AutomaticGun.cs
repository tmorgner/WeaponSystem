using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   An automatic gun that continuously fires at all targets in the
    ///   tracking area. This class needs at least one configured barrel to work
    ///   correctly.
    /// </summary>
    public class AutomaticGun : GunBase
    {
        /// <summary>
        ///  The gun projectile spawn mode for multi-barrel guns.
        /// </summary>
        public enum SpawnMode
        {
            /// <summary>
            ///   All barrels shoot at the same time.
            /// </summary>
            All,
            /// <summary>
            ///   The gun shoots one barrel at a time.
            /// </summary>
            RoundRobin
        }

        [FormerlySerializedAs("barrels")]
        [SerializeField]
        List<GameObject> muzzlePoints;
        [SerializeField] GameObject pivotObject;
        [SerializeField] UnityEvent beginIdleEvent;
        [SerializeField] UnityEvent beginChargingEvent;
        [SerializeField] SpawnMode spawnMode;
        int nextMuzzleFired;

        /// <summary>
        ///   A Unity-Event that is fired when the gun became idle.
        /// </summary>
        public UnityEvent BeginIdleEvent => beginIdleEvent;

        /// <summary>
        ///   A Unity-Event that is fired when the gun started charging.
        /// </summary>
        public UnityEvent BeginChargingEvent => beginChargingEvent;

        /// <summary>
        ///   A list of muzzle points for the gun. These points are spawn points for projectiles.
        /// </summary>
        protected IReadOnlyList<GameObject> MuzzlePoints => muzzlePoints;

        /// <summary>
        ///   Returns the computed pivot point as origin point for targeting calculations.
        /// </summary>
        protected override Transform WeaponTargetTrackerOrigin => pivotObject.transform;

        /// <summary>
        ///   Recomputes the pivot object if necessary.
        /// </summary>
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

        /// <inheritdoc />
        protected override void BeginIdleOverride()
        {
            beginIdleEvent.Invoke();
        }

        /// <inheritdoc />
        protected override float BeginChargingOverride()
        {
            beginChargingEvent.Invoke();
            return base.BeginChargingOverride();
        }

        Vector3 ComputeBarrelCenterPosition()
        {
            if (muzzlePoints.Count == 0)
            {
                return transform.position;
            }

            var centre = new Vector3();
            foreach (var b in muzzlePoints)
            {
                centre += b.transform.position;
            }

            centre.x /= muzzlePoints.Count;
            centre.y /= muzzlePoints.Count;
            centre.z /= muzzlePoints.Count;
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

            if (muzzlePoints.Count == 0)
            {
                return transform.rotation;
            }

            var cumulativeVector = new Vector4();
            var firstBarrelRotation = muzzlePoints[0].transform.rotation;
            var result = firstBarrelRotation;
            for (var c = 1; c < muzzlePoints.Count; c += 1)
            {
                result = AverageQuaternion(ref cumulativeVector, muzzlePoints[c].transform.rotation, firstBarrelRotation, c);
            }

            return result;
        }

        /// <summary>
        ///   Fires the gun based on the defined fire mode.
        /// </summary>
        /// <param name="fireTarget">The target to be fired at</param>
        protected override void BeginFireOverride(Vector3 fireTarget)
        {
            if (spawnMode == SpawnMode.All)
            {
                foreach (var b in muzzlePoints)
                {
                    DoFire(b.transform.position, fireTarget);
                }
            }
            else if (muzzlePoints.Count > 0)
            {
                if (nextMuzzleFired >= muzzlePoints.Count)
                {
                    nextMuzzleFired = 0;
                }

                DoFire(muzzlePoints[nextMuzzleFired].transform.position, fireTarget);
                nextMuzzleFired = (nextMuzzleFired + 1) % muzzlePoints.Count;
            }
        }

        public int MuzzleFired => nextMuzzleFired;
    }
}
