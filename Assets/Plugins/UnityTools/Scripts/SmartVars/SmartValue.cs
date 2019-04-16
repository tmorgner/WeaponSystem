using System.Collections.Generic;
using UnityEngine;


namespace UnityTools.SmartVars
{
  /// <summary>
  ///  A named reference that is shared between instances. The value base class here does not
  ///  hold any values, values are held by SmartVariables (filled by publishers) and SmartConstValues
  ///  (which hold design time data).
  /// </summary>
  public abstract class SmartValue<TDataType> : ScriptableObject, ISerializationCallbackReceiver
  {
#if UNITY_EDITOR
    [Multiline] public string DeveloperDescription = "";
#endif

    public WeakEvent ValueChanged = new WeakEvent();
    public abstract TDataType GetValue();
    TDataType lastValue;
    public bool Enabled { get; private set; }

    void OnDisable()
    {
      Enabled = false;
    }

    void OnEnable()
    {
      Enabled = true;
    }

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
      if (Enabled)
      {
        OnChanged();
      }
      else
      {
        lastValue = GetValue();
      }
    }

    void OnChanged()
    {
      var value = GetValue();
      if (!EqualityComparer<TDataType>.Default.Equals(lastValue, value))
      {
        lastValue = value;
        if (ValueChanged != null)
        {
          ValueChanged.Invoke();
        }
      }
    }
  }
}
