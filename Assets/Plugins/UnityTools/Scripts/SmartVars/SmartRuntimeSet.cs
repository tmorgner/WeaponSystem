using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace RabbitStewdio.Unity.UnityTools.SmartVars
{
    /// <summary>
    ///  This base class exists only because we cannot use interfaces in Unity due to serialisation constraints.
    /// </summary>
    public abstract class SmartRuntimeSet : ScriptableObject
    {
        public abstract Type GetMemberType();

        public abstract void Register(object o);
        public abstract void Unregister(object o);

        public abstract void Restore();

        void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            OnDisableOverride();
        }

        protected virtual void OnDisableOverride()
        {
        }

        void OnEnable()
        {
            Restore();
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            OnEnableOverride();
        }

        protected virtual void OnEnableOverride()
        {
        }

        void OnActiveSceneChanged(Scene prev, Scene next)
        {
            Restore();
        }
    }

    public class SmartRuntimeSet<T> : SmartRuntimeSet, IReadOnlyList<T>
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        static readonly List<T> empty = new List<T>();

        public event Action<T> OnEntryAdded;
        public event Action<T> OnEntryRemoved;

        [FormerlySerializedAs("Values")] 
        [SerializeField] 
        List<T> values;
        readonly HashSet<T> uniqueValues;
        readonly List<T> runtimeValues;

        public SmartRuntimeSet(IEqualityComparer<T> comparer)
        {
            values = new List<T>();
            uniqueValues = new HashSet<T>(comparer);
            runtimeValues = new List<T>();
        }

        public ReadOnlyListWrapper<T> Values => new ReadOnlyListWrapper<T>(runtimeValues);

        public bool Contains(T t)
        {
            return uniqueValues.Contains(t);
        }

        public override void Restore()
        {
            runtimeValues.Clear();
            uniqueValues.Clear();

            if (values != null)
            {
                foreach (var value in values)
                {
                    Register(value);
                }
            }
        }

        public override Type GetMemberType()
        {
            return typeof(T);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            if (runtimeValues == null)
            {
                return empty.GetEnumerator();
            }

            return runtimeValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                return runtimeValues.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                return runtimeValues[index];
            }
        }

        public override void Register(object o)
        {
            if (runtimeValues != null && o is T)
            {
                var t = (T) o;
                if (uniqueValues.Add(t))
                {
                    runtimeValues.Add(t);
                    OnEntryAdded?.Invoke(t);
                }
            }
        }

        public void RegisterTyped(T o) => Register(o);

        public override void Unregister(object o)
        {
            if (runtimeValues != null && o is T)
            {
                var t = (T) o;
                if (uniqueValues.Remove(t))
                {
                    runtimeValues.Remove(t);
                    OnEntryRemoved?.Invoke(t);
                }
            }
        }

        public void UnregisterTyped(T o) => Register(o);

        protected override void OnDisableOverride()
        {
            OnEntryAdded = null;
            OnEntryRemoved = null;
        }
    }
}
