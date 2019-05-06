using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.SmartVars.RuntimeSets
{
  [CreateAssetMenu(menuName = "Smart Variables/Runtime Sets/UnityEngine/Transform")]
  public class SmartTransformSet : SmartRuntimeComponentSet<Transform>
  {
    public bool ContainedAsParent(Transform t)
    {
      var key = t;
      do
      {
        if (Contains(key))
        {
          return true;
        }

        key = key.parent;
      } while (key);

      return false;
    }
  }
}