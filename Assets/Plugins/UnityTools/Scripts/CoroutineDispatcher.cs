using System.Collections;
using UnityEngine;

namespace UnityTools
{
    public class CoroutineDispatcher : MonoBehaviour
    {
        static CoroutineDispatcher instance;

        public static CoroutineDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("Coroutine Dispatcher Object")
                    {
                        hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor
                    };
                    instance = go.AddComponent<CoroutineDispatcher>();
                }

                return instance;
            }
        }

        void OnDestroy()
        {
            instance = null;
        }

        public new CoroutineHandle StartCoroutine(IEnumerator e)
        {
            var handle = base.StartCoroutine(e);
            return new CoroutineHandle(handle, this);
        }
    }

    public struct CoroutineHandle
    {
        readonly Coroutine coroutineHandle;
        readonly CoroutineDispatcher dispatcher;

        public CoroutineHandle(Coroutine coroutineHandle, CoroutineDispatcher dispatcher)
        {
            if (coroutineHandle == null)
            {
                throw new System.ArgumentNullException(nameof(coroutineHandle));
            }

            if (dispatcher == null)
            {
                throw new System.ArgumentNullException(nameof(dispatcher));
            }

            this.coroutineHandle = coroutineHandle;
            this.dispatcher = dispatcher;
        }

        public void Stop()
        {
            dispatcher.StopCoroutine(coroutineHandle);
        }
    }
}