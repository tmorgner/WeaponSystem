using System.Collections.Generic;
using RabbitStewdio.Unity.UnityTools;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///  A targeting selection strategy that weights the targets given in the potentialTargets list
    ///  and selects the best target to be fired at.
    /// </summary>
    public abstract class TargetSelectionStrategy : ScriptableObject
    {
        /// <summary>
        ///   Selects the best target by computing a preference as weight and feeding all selected targets with their
        ///   weight into the aiming measure instance provided.
        /// </summary>
        /// <param name="measure">A secondary filter that chooses one target out of the supplied targets based on player preference.</param>
        /// <param name="info">A query provider to compute weapon specific parameter for potential targets.</param>
        /// <param name="potentialTargets">A list of potential targets</param>
        public abstract void SelectTargets(IAimingMeasure measure, ITargetSelectionInformation info, ReadOnlyListWrapper<Rigidbody> potentialTargets);
    }
}