using NaughtyAttributes;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public interface ISelectiveLogBehaviour
    {
        bool LogPath { get; }
        bool EnableLogging { get; }
        bool EnableWarnings{ get; }
        string Name { get; }
        string Path { get; }
    }

    public abstract class SelectiveLogBehaviour : MonoBehaviour, ISelectiveLogBehaviour
    {
        [BoxGroup("Logging")]
        [SerializeField] bool enableLogging;
        
        [BoxGroup("Logging")]
        [EnableIf(nameof(enableLogging))]
        [SerializeField] bool logPath;

        [BoxGroup("Logging")]
        [EnableIf(nameof(enableLogging))]
        [SerializeField] bool enableWarnings;

        public bool LogPath
        {
            get => logPath;
            set => logPath = value;
        }

        public bool EnableLogging
        {
            get => enableLogging;
            set => enableLogging = value;
        }

        public bool EnableWarnings
        {
            get => enableWarnings;
            set => enableWarnings = value;
        }

        public string Name { get; private set; }
        bool awakeCalled;

        public string Path => GameObjectTools.GetPath(this.transform);

        protected void Awake()
        {
            Name = name;
            awakeCalled = true;
            AwakeOverride();
        }

        protected  virtual void AwakeOverride()
        {
        }

        protected void OnEnable()
        {
            Name = name;
            if (!awakeCalled)
            {
                Awake();
            }

            OnEnableOverride();
        }

        protected virtual void OnEnableOverride()
        {
            
        }
    }
}