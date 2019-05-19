using NaughtyAttributes;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    /// <summary>
    ///     Same as ParentConstraint, but I will be damned if I ever figure out how
    ///     Unity's parent constraint actually works.
    /// </summary>
    public class FollowObjectConstraint : ObjectConstraint
    {
        [SerializeField] bool recenterToParent;
        [SerializeField] Transform target;
        Quaternion rotation;
        [ShowNativeProperty]
        Vector3 Eulers => rotation.eulerAngles; 
        [ShowNonSerializedField]
        Vector3 offset;
        [ShowNonSerializedField]
        bool haveTarget;

        public void SetTarget(Transform target)
        {
            haveTarget = target != null;
            this.target = target;
            if (target && !recenterToParent)
            {
                var inverse = Quaternion.Inverse(target.rotation);
                offset = inverse * (transform.position - target.position);
                rotation = inverse * transform.rotation;
            }
            else
            {
                offset = Vector3.zero;
                rotation = Quaternion.identity;
            }
        }
        
        void Awake()
        {
            haveTarget = target != null;
            if (haveTarget)
            {
                SetTarget(target);
            }
        }

        protected override void OnFollow()
        {
            if (!haveTarget)
            {
                return;
            }

            haveTarget = target != null;
            if (!haveTarget)
            {
                target = null;
                return;
            }

            transform.position = target.position + target.rotation * offset;
            transform.rotation = target.rotation * rotation;
        }

    }
}