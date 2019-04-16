using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    /// <summary>
    ///     Same as ParentConstraint, but I will be damned if I ever figure out how
    ///     Unity's parent constraint actually works.
    /// </summary>
    public class FollowObjectConstraint : ObjectConstraint
    {

        [SerializeField] Transform target;
        Quaternion rotation;
        Vector3 offset;
        bool haveTarget;



        public void SetTarget(Transform target)
        {
            haveTarget = target != null;
            this.target = target;
            offset = Quaternion.Inverse(target.rotation) * (transform.position - target.position);
            rotation = Quaternion.Inverse(target.rotation) * transform.rotation;
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

            transform.position = target.position + target.rotation * offset;
            transform.rotation = target.rotation * rotation;
        }

    }
}