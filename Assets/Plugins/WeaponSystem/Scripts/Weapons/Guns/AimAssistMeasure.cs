using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A aiming measure filter that implements an aim-assist strategy. The target
    ///   will be selected based on how close it is to the current forward vector of the
    ///   gun.
    /// </summary>
    public class AimAssistMeasure : IAimingMeasure
    {
        readonly Transform aimingSource;
        float alignment;

        public AimAssistMeasure(Transform aimingSource)
        {
            this.aimingSource = aimingSource;
            this.alignment = float.MinValue;
        }

        /// <inheritdoc />
        public void Reset()
        {
            alignment = float.MinValue;
            Result = null;
        }

        /// <summary>
        ///  Select the target that is most closest aligned with the current aiming source.
        ///  This strategy minimizes lateral movement while shooting so that the player
        ///  can continue to believe that his input was the reason the target was hit.
        /// </summary>
        /// <param name="body">the potential target</param>
        /// <param name="targetWeight">the weight given by the previous selection, usually meaning 0 is most preferred, and 1 is least preferred.</param>
        /// <param name="target"></param>
        public void Track(Rigidbody body, float targetWeight, ref Vector3 target)
        {
            // returns 1 for perfectly aligned, 0 for 90-deg misaligned and less than 0 for behind the gun.
            // so clamping to 0/1 is fine and makes the weighting step easier.
            var dot = Mathf.Clamp01(Vector3.Dot(aimingSource.forward, (target - aimingSource.position).normalized));

            // select targets that are closely aligned, but also express a preference for targets close by.
            var preference = Mathf.Lerp(dot, targetWeight, 0.1f);
            if (preference > this.alignment)
            {
                this.Result = body;
                this.alignment = preference;
            }
        }

        /// <inheritdoc />
        public Rigidbody Result { get; private set; }
    }
}