using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.SmartVars.RuntimeSets
{
    [AddComponentMenu("Smart Variables/Smart Runtime Set Membership")]
    public class SmartRuntimeSetMemberBehaviour : MonoBehaviour
    {
        [Tooltip("Designer note on how this set should be used.")]
        [ResizableTextArea]
        [SerializeField]
        string usageNote;

        [SerializeField]
        List<SmartRuntimeSet> RuntimeSets;
        bool disableCalled;

        public SmartRuntimeSetMemberBehaviour()
        {
            RuntimeSets = new List<SmartRuntimeSet>();
        }

        void OnDisable()
        {
            foreach (var runtimeSet in RuntimeSets)
            {
                if (runtimeSet == null)
                {
                    continue;
                }

                var t = runtimeSet.GetMemberType();
                if (typeof(Component).IsAssignableFrom(t))
                {
                    var comp = GetComponents(t);
                    foreach (var c in comp)
                    {
                        runtimeSet.Unregister(c);
                    }
                }
            }

            disableCalled = true;
        }

        void OnEnable()
        {
            foreach (var runtimeSet in RuntimeSets)
            {
                if (runtimeSet == null)
                {
                    continue;
                }

                var t = runtimeSet.GetMemberType();
                if (typeof(Component).IsAssignableFrom(t))
                {
                    var comp = GetComponents(t);
                    foreach (var c in comp)
                    {
                        runtimeSet.Register(c);
                    }
                }
            }
        }

        void OnDestroy()
        {
            RuntimeSets.Clear();
        }
    }
}
