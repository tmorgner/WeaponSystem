using System;
using System.Collections.Generic;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public class DetectColliderOverlaps : MonoBehaviour
    {
        public struct TriggerInfo
        {
            GameObject self;
            HashSet<GameObject> others;

            public TriggerInfo(GameObject self)
            {
                this.self = self;
                this.others = null;
            }

            public bool Register(GameObject other)
            {
                if (others == null)
                {
                    others = new HashSet<GameObject>();
                }

                return others.Add(other);
            }

            public GameObject Self => self;

            public void Clear()
            {
                others?.Clear();
            }

            public bool IsEmpty()
            {
                return others == null || others.Count == 0;
            }

            public string Print()
            {
                StringBuilder s = new StringBuilder();
                s.Append($"GameObject: {self.GetPath()}\n");
                if (others != null)
                {
                    var count = 0;
                    foreach (var x in others)
                    {
                        s.Append($"  {count}: {x.GetPath()}\n");
                    }
                }

                return s.ToString();
            }
        }

        public class ColliderDetectorProbe : MonoBehaviour
        {
            public DetectColliderOverlaps parent;

            void OnEnable()
            {
                if (parent)
                {
                    parent.Added(this);
                }
            }

            void OnDisable()
            {
                if (parent)
                {
                    parent.Removed(this);
                }
            }

            void OnTriggerStay(Collider other)
            {
                parent.OnTriggerStayReived(this, other);
            }

            void OnApplicationQuit()
            {
                Destroy(this);
            }
        }

        [SerializeField]
        bool detailedLog;
        [SerializeField]
        float updateTime;

        [ShowNonSerializedField]
        public int collidersDetected;
        int currentFrameAccumulator;

        readonly Dictionary<int, TriggerInfo> collidersAdded;
        float refresh;

        public DetectColliderOverlaps()
        {
            collidersAdded = new Dictionary<int, TriggerInfo>();
            updateTime = 5;
        }

        void Update()
        {
            if (refresh < Time.time)
            {
                Debug.Log("Refreshing Objects, have " + currentFrameAccumulator + " collisions on record.");
                if (detailedLog)
                {
                    foreach (var c in collidersAdded)
                    {
                        if (c.Value.IsEmpty())
                        {
                            continue;
                        }

                        Debug.Log(c.Value.Print());
                    }
                }

                var colliders = FindObjectsOfType<Collider>();
                foreach (var c in colliders)
                {
                    var self = c.gameObject;
                    if (collidersAdded.TryGetValue(self.GetInstanceID(), out _))
                    {
                        continue;
                    }

                    var p = self.AddComponent<ColliderDetectorProbe>();
                    p.parent = this;
                    collidersAdded[self.GetInstanceID()] = new TriggerInfo(self);
                }

                refresh = Time.time + updateTime;
            }
        }

        void FixedUpdate()
        {
            collidersDetected = currentFrameAccumulator;

            foreach (var v in collidersAdded.Values)
            {
                v.Clear();
            }

            currentFrameAccumulator = 0;
        }

        void OnTriggerStayReived(ColliderDetectorProbe colliderDetectorProbe, Collider other)
        {
            var self = colliderDetectorProbe.gameObject;
            if (collidersAdded.TryGetValue(self.GetInstanceID(), out var x))
            {
                x.Register(other.gameObject);
                currentFrameAccumulator += 1;
                collidersAdded[self.GetInstanceID()] = x;
            }
        }

        void Removed(ColliderDetectorProbe colliderDetectorProbe)
        {
            collidersAdded.Remove(colliderDetectorProbe.gameObject.GetInstanceID());
        }

        void Added(ColliderDetectorProbe colliderDetectorProbe)
        {
            var self = colliderDetectorProbe.gameObject;
            if (collidersAdded.TryGetValue(self.GetInstanceID(), out _))
            {
                return;
            }

            collidersAdded[self.GetInstanceID()] = new TriggerInfo(self);
        }
    }
}
