using System;
using UnityEngine;
using UnityEngine.Events;

namespace RabbitStewdio.Unity.UnityTools.SmartVars.SystemTypes
{
    public abstract class NamedObjectReferenceBase : ScriptableObject
    {
        public GameObject ReferencedObject { get; private set; }

        public UnityEvent ReferencedObjectChanged { get; }

        protected NamedObjectReferenceBase()
        {
            ReferencedObjectChanged = new UnityEvent();
        }

        internal void UpdateGameObjectReference(GameObject go)
        {
            var old = ReferencedObject;
            if (go == null)
            {
                ReferencedObject = go;
            }
            else
            {
                ReferencedObject = Filter(go);
            }

            if (old != ReferencedObject)
            {
                ReferencedObjectChanged?.Invoke();
            }
        }

        protected virtual GameObject Filter(GameObject go)
        {
            return go;
        }

        void OnEnable()
        {
            ReferencedObjectChanged.RemoveAllListeners();
            OnEnableOverride();
        }

        protected virtual void OnEnableOverride()
        {
            
        }

        void OnDisable()
        {
            OnDisableOverride();
            ReferencedObjectChanged.RemoveAllListeners();
            ReferencedObject = null;
        }

        protected virtual void OnDisableOverride()
        {

        }
    }
}