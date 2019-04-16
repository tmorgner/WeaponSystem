using System;

namespace UnityTools.SmartVars.SystemTypes
{
  [Serializable]
  public class SmartBoolReference : SmartReference<bool, SmartBoolVariable, SmartBoolConstant>
  {
  }
}