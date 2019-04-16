using UnityEngine;

namespace UnityTools.SmartVars
{
  public class SmartVariable<TValue> : SmartValue<TValue>
  {
    [SerializeField] TValue value;
    public override TValue GetValue()
    {
      return Value;
    }

    protected SmartVariable()
    {
    }

    public TValue Value
    {
      get
      {
        return value;
      }
      set
      {
        this.value = value;
      }
    }
  }
}