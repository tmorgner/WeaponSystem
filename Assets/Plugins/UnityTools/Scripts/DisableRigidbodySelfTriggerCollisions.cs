using System;
using System.Collections.Generic;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public class DisableRigidbodySelfTriggerCollisions : MonoBehaviour
    {
        static readonly List<Collider> colliderBuffer = new List<Collider>(32);

        void Awake()
        {
            var colliders = gameObject.GetComponentsInChildrenNonAlloc(colliderBuffer, true, true);
            foreach (var c1 in colliders)
            {
                if (!c1.isTrigger)
                {
                    continue;
                }

                foreach (var c2 in colliders)
                {
                    if (c1 == c2)
                    {
                        continue;
                    }

                   // Physics.IgnoreCollision(c1, c2, true);
                }
            }
        }
    }
}