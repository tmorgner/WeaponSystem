using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public class CompositeColliderCollector : MonoBehaviour, IEnumerable<Collider>
    {
        readonly List<Collider> colliders;

        public CompositeColliderCollector()
        {
            this.colliders = new List<Collider>();
        }

        void Awake()
        {
            CollectColliders(transform, colliders);
        }

        static void CollectColliders(Transform trs, List<Collider> list)
        {
            var component = trs.GetComponent<Collider>();
            if (component != null)
            {
                list.Add(component);
            }
            var transformChildCount = trs.childCount;
            for (var c = 0; c < transformChildCount; c += 1)
            {
                var go = trs.GetChild(c);
                CollectColliders(go, list);
            }
        }

        public List<Collider>.Enumerator GetEnumerator() => colliders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<Collider> IEnumerable<Collider>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}