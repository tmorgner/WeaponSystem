using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A aiming measure that selects targets based on the weight given by the target selection strategy.
    /// </summary>
    public class AutoAimingMeasure : IAimingMeasure
    {
        float selectedWeight;

        public AutoAimingMeasure()
        {
            selectedWeight = float.MaxValue;
        }

        /// <summary>
        ///    Resets the the aiming.
        /// </summary>
        public void Reset()
        {
            selectedWeight = float.MaxValue;
            Result = null;
        }

        /// <summary>
        ///   Adds a new selectable target to the selection choice. This aiming measure will
        ///   select targets with the lowest weight.
        /// </summary>
        /// <param name="body">The potential target</param>
        /// <param name="targetWeight">the preference weight</param>
        /// <param name="target">the target position</param>
        public void Track(Rigidbody body, float targetWeight, ref Vector3 target)
        {
            if (targetWeight < this.selectedWeight)
            {
                this.Result = body;
                this.selectedWeight = targetWeight;
            }
        }

        /// <inheritdoc />
        public Rigidbody Result
        {
            get;
            private set;
        }
    }
}