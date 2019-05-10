using System.Collections.Generic;
using RabbitStewdio.Unity.UnityTools;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A target selection strategy that selects targets with a random weighting.
    /// </summary>
    [CreateAssetMenu(menuName = "Weapons/Target Selectors/Random")]
    public class RandomTargetSelectionStrategy : TargetSelectionStrategy
    {
        /// <inheritdoc />
        public override void SelectTargets(IAimingMeasure aiming, ITargetSelectionInformation info, ReadOnlyListWrapper<Rigidbody> potentialTargets)
        {
            foreach (var possibleTarget in potentialTargets)
            {
                if (info.PredictTargetPosition(possibleTarget, out _, out var predictedPosition))
                {
                    if (info.IsVisible(possibleTarget))
                    {
                        aiming.Track(possibleTarget, Random.value, ref predictedPosition);
                    }
                }
            }
        }
    }
}
