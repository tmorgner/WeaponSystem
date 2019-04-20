namespace RabbitStewdio.Unity.UnityTools.SmartVars
{
  public abstract class SmartConstant<TValue> : SmartValue<TValue>
  {
    public TValue value;

    public override TValue GetValue()
    {
      return value;
    }
  }
}