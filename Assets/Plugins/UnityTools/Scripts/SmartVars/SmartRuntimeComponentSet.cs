using System.Collections.Generic;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.SmartVars
{
  public class SmartRuntimeComponentSet<T> : SmartRuntimeSet<T> where T: Component
  {
    class ComponentComparer : IEqualityComparer<T>
    {
      public bool Equals(T x, T y)
      {
        if (ReferenceEquals(x, y))
        {
          return true;
        }

        // Avoid Unity's weird overload, or we would never be able to 
        // remove destroyed objects from the set.
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
        {
          return false;
        }

        return x.GetInstanceID() == y.GetInstanceID();
      }

      public int GetHashCode(T obj)
      {
        return obj.GetHashCode();
      }
    }

    static readonly IEqualityComparer<T> sharedComparer = new ComponentComparer();

    public SmartRuntimeComponentSet() : base(sharedComparer)
    {
      
    }
  }
}