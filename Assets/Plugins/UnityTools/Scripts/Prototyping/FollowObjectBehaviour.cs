using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.Prototyping
{
    public class FollowObjectBehaviour : MonoBehaviour
    {
        public GameObject Target;
        public float Distance;

        void LateUpdate()
        {
            if (!Target)
            {
                return;
            }

            var t = Target.transform;
            var fwd = -t.forward;
            var pos = t.position - fwd * Distance;

            transform.position = pos;
            transform.LookAt(t, t.up);
        }
    }
}
