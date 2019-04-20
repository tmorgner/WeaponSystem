using RabbitStewdio.Unity.UnityTools.SmartVars.RuntimeSets;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.SmartVars.Helpers
{
  /// <summary>
  ///  Tags a given object and automatically adds it to the given runtime set.
  /// </summary>
  [AddComponentMenu("Smart Variables/Tag")]
  public class TagObject: MonoBehaviour
  {
    public SmartTransformSet TagObjectCollection;

    void OnDisable()
    {
      TagObjectCollection.Unregister(transform);
    }

    void OnEnable()
    {
      TagObjectCollection.Register(transform);
    }
  }
}