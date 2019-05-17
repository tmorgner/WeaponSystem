using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public class RemountChildrenToRoot : MonoBehaviour
    {
        void Start()
        {
            if (Application.isEditor)
            {
                return;
            }

            for (var i = transform.childCount -1; i >= 0; i -= 1)
            {
                var c = transform.GetChild(i);
                c.SetParent(null, true);
            }
        }
    }
}