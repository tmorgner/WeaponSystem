using System.Collections.Generic;
using RabbitStewdio.Unity.UnityTools;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A target selection strategy that selects the closest target.
    /// </summary>
    [CreateAssetMenu(menuName = "Weapons/Target Selectors/By Distance")]
    public class TargetByDistanceSelectionStrategy : TargetSelectionStrategy
    {
        /// <inheritdoc />
        public override void SelectTargets(IAimingMeasure aiming,
                                                  ITargetSelectionInformation info,
                                                  ReadOnlyListWrapper<Rigidbody> potentialTargets)
        {
            foreach (var possibleTarget in potentialTargets)
            {
                if (info.PredictTargetPosition(possibleTarget, out var distance, out var predictedPosition))
                {
                    if (info.IsVisible(possibleTarget))
                    {
                        var weight = Mathf.Clamp01(distance / info.MaximumTargetRange);
                        aiming.Track(possibleTarget, weight, ref predictedPosition);
                    }
                }
            }
        }
    }
}
