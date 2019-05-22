using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.SmartVars.SystemTypes
{
    public class NamedObjectReferenceUpdater : MonoBehaviour
    {
        [SerializeField] NamedObjectReferenceBase target;

        void OnDisable()
        {
            if (target != null)
            {
                if (target.ReferencedObject != gameObject)
                {
                    Debug.Log("Became inactive and found someone else's content " + target.name + " with " + target.ReferencedObject + ", wanted it to be " + name);
                    return;
                }
                Debug.Log("Became inactive and cleared target " + target.name + " with " + name);
                target.UpdateGameObjectReference(null);
            }
        }

        void OnEnable()
        {
            if (target != null)
            {
                Debug.Log("Became active and set target " + target.name + " with " + name);
                target.UpdateGameObjectReference(gameObject);
            }
        }
    }
}