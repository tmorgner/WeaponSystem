using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.SmartVars.SystemTypes
{
    public abstract class NamedObjectReferenceBase : ScriptableObject
    {
        public GameObject ReferencedObject { get; private set; }

        internal void UpdateGameObjectReference(GameObject go)
        {
            if (go == null)
            {
                ReferencedObject = go;
            }
            else
            {
                ReferencedObject = Filter(go);
            }
        }

        protected virtual GameObject Filter(GameObject go)
        {
            return go;
        }

        void OnDisable()
        {
            OnDisableOverride();
            ReferencedObject = null;
        }

        protected virtual void OnDisableOverride()
        {

        }
    }
}