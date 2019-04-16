using UnityEngine;

namespace UnityTools
{
  public abstract class DelayStartupBehaviour: MonoBehaviour
  {
    protected abstract void OnEnableComponent();
    protected abstract void OnDisableComponent();
    bool started;

    void Start()
    {
      started = true;
      OnEnableComponent();
    }

    void OnEnable()
    {
      if (started)
      {
        OnEnableComponent();
      }
    }

    void OnDisable()
    {
      OnDisableComponent();
    }
  }
}
