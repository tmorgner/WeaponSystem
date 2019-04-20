using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RabbitStewdio.Unity.UnityTools
{
    public abstract class ScriptableServiceObject<TBehaviour> : ScriptableObject where TBehaviour : MonoBehaviour
    {
        [SerializeField] bool hideInEditor;

        public UnityEvent Initialized { get; private set; }
        TBehaviour serviceBehaviour;
        bool initialized;

        protected ScriptableServiceObject()
        {
            Initialized = new UnityEvent();
        }

        public bool IsInitialized => initialized;

        void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            OnEnableOverride();
            Application.quitting += OnQuitting;

            if (Application.isPlaying)
            {
                OnActiveSceneChanged(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
            }
        }

        void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Application.quitting -= OnQuitting;
            OnDisableOverride();
            OnQuitting();
        }

        void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (!initialized)
            {
                serviceBehaviour = OnInitialize();
                initialized = true;
            }
            else if (serviceBehaviour == null)
            {
                serviceBehaviour = OnInitialize();
            }
        }

        protected TBehaviour ServiceBehaviour => serviceBehaviour;


        protected TBehaviour OnInitialize()
        {
            var go = new GameObject();
            go.hideFlags = HideFlags.NotEditable | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            if (hideInEditor)
            {
                go.hideFlags |= HideFlags.HideInHierarchy;
            }
            go.name = $"{typeof(TBehaviour).Name}-Service";

            var service = go.AddComponent<TBehaviour>();
            OnConfigureService(service);
            Initialized.Invoke();
            return service;
        }

        protected virtual void OnConfigureService(TBehaviour b)
        {
        }

        protected virtual void OnEnableOverride() { }
        protected virtual void OnDisableOverride() { }

        protected void OnQuitting()
        {
            if (ServiceBehaviour != null)
            {
                OnQuittingOverride(ServiceBehaviour);
                Destroy(serviceBehaviour.gameObject);
                serviceBehaviour = null;
            }
        }

        protected virtual void OnQuittingOverride(TBehaviour behaviour)
        {

        }
    }
}