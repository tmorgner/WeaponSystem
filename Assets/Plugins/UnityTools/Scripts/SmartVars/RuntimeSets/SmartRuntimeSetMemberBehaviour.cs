using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.SmartVars.RuntimeSets
{
  [AddComponentMenu("Smart Variables/Smart Runtime Set Membership")]
  public class SmartRuntimeSetMemberBehaviour : MonoBehaviour
  {
    public List<SmartRuntimeSet> RuntimeSets;

    public SmartRuntimeSetMemberBehaviour()
    {
      RuntimeSets = new List<SmartRuntimeSet>();
    }

    void OnDisable()
    {
      foreach (var runtimeSet in RuntimeSets)
      {
        if (runtimeSet == null)
        {
          continue;
        }

        var t = runtimeSet.GetMemberType();
        if (typeof(Component).IsAssignableFrom(t))
        {
          var comp = GetComponents(t);
          foreach (var c in comp)
          {
            runtimeSet.Unregister(c);
          }
        }
      }
    }

    void OnEnable()
    {
      foreach (var runtimeSet in RuntimeSets)
      {
        if (runtimeSet == null)
        {
          continue;
        }

        var t = runtimeSet.GetMemberType();
        if (typeof(Component).IsAssignableFrom(t))
        {
          var comp = GetComponents(t);
          foreach (var c in comp)
          {
            runtimeSet.Register(c);
          }
        }
      }
    }

    void OnDestroy()
    {
      RuntimeSets.Clear();
    }
  }
}