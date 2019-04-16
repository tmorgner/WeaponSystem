using System;

namespace UnityTools.SmartVars
{
  public abstract class SmartReference<TDataType>
  {
    public abstract TDataType GetValue();

    public static implicit operator TDataType(SmartReference<TDataType> reference)
    {
      return reference.GetValue();
    }
  }

  [Serializable]
  public abstract class SmartReference<TDataType, TVar, TConst> : SmartReference<TDataType>
    where TVar : SmartVariable<TDataType>
    where TConst : SmartConstant<TDataType>
  {
    public SmartReferenceType ReferenceType;
    public TVar Variable;
    public TConst ConstantValue;
    public TDataType InlineValue;

    public SmartReference()
    {
    }

    public SmartReference(TDataType value)
    {
      InlineValue = value;
      ReferenceType = SmartReferenceType.InlineValue;
    }

    public override TDataType GetValue()
    {
      switch (ReferenceType)
      {
        case SmartReferenceType.InlineValue:
          return InlineValue;
        case SmartReferenceType.Variable:
          return Variable.Value;
        case SmartReferenceType.ConstantReference:
          return ConstantValue.GetValue();
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
