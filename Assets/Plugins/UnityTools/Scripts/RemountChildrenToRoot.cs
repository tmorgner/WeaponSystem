using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public class RemountChildrenToRoot : MonoBehaviour, ISerializationCallbackReceiver
    {
        [ReadOnly]
        [SerializeField]
        List<GameObject> gameObjects;
        List<GameObject> internalObjects;

        public RemountChildrenToRoot()
        {
            internalObjects = new List<GameObject>();
            gameObjects = new List<GameObject>();
        }

        void Start()
        {
            for (var i = transform.childCount -1; i >= 0; i -= 1)
            {
                var c = transform.GetChild(i);
                c.SetParent(null, true);
                internalObjects.Add(c.gameObject);
                gameObjects.Add(c.gameObject);
            }
        }

        public void OnBeforeSerialize()
        {
            gameObjects.Clear();
        }

        public void OnAfterDeserialize()
        {
            gameObjects.AddRange(internalObjects);
        }
    }
}