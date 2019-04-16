using UnityEngine;

namespace RabbitStewdio.Unity.GunShip.Scripts.VRHelpers
{
    public class NamedObjectReferenceUpdater : MonoBehaviour
    {
        [SerializeField] NamedObjectReferenceBase target;

        void OnDisable()
        {
            if (target != null)
            {
                target.UpdateGameObjectReference(null);
            }
        }

        void OnEnable()
        {
            if (target != null)
            {
                target.UpdateGameObjectReference(gameObject);
            }
        }
    }
}