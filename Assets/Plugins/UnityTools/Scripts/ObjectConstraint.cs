using System;
using RabbitStewdio.Unity.Plugins.UnityTools.Scripts;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public abstract class ObjectConstraint : MonoBehaviour
    {
        /// <summary>
        ///     The moment at which to follow.
        /// </summary>
        [Flags]
        public enum FollowMoment
        {
            None = 0,
            /// <summary>
            ///     Follow in the FixedUpdate method.
            /// </summary>
            OnFixedUpdate = 1,

            /// <summary>
            ///     Follow in the Update method.
            /// </summary>
            OnUpdate = 2,

            /// <summary>
            ///     Follow in the LateUpdate method.
            /// </summary>
            OnLateUpdate = 4,

            /// <summary>
            ///     Follow in the OnPreRender method. (This script doesn't have to be attached to a camera).
            /// </summary>
            OnPreRender = 8,

            /// <summary>
            ///     Follow in the OnPreCull method. (This script doesn't have to be attached to a camera).
            /// </summary>
            OnPreCull = 16,

            UpdateAndRender = OnUpdate | OnPreCull | OnLateUpdate
        }

        [SerializeField] FollowMoment moment;

        protected virtual void Reset()
        {
            moment = FollowMoment.OnPreRender | FollowMoment.OnUpdate | FollowMoment.OnLateUpdate;
        }

        public FollowMoment Moment
        {
            get => moment;
            set
            {
                if (isActiveAndEnabled)
                {
                    if (moment.IsFlagSet(FollowMoment.OnPreCull))
                    {
                        Camera.onPreCull -= HandleCameraCallbacks;
                    }

                    if (moment.IsFlagSet(FollowMoment.OnPreRender))
                    {
                        Camera.onPreRender -= HandleCameraCallbacks;
                    }
                }

                moment = value;
                if (isActiveAndEnabled)
                {
                    if (moment.IsFlagSet(FollowMoment.OnPreCull))
                    {
                        Camera.onPreCull += HandleCameraCallbacks;
                    }

                    if (moment.IsFlagSet(FollowMoment.OnPreRender))
                    {
                        Camera.onPreRender += HandleCameraCallbacks;
                    }
                }
            }
        }

        void HandleCameraCallbacks(Camera cam)
        {
            OnFollow();
        }


        protected virtual void FixedUpdate()
        {
            if (moment.IsFlagSet(FollowMoment.OnFixedUpdate))
            {
                OnFollow();
            }
        }

        protected virtual void LateUpdate()
        {
            if (moment.IsFlagSet(FollowMoment.OnLateUpdate))
            {
                OnFollow();
            }
        }

        protected virtual void Update()
        {
            if (moment.IsFlagSet(FollowMoment.OnUpdate))
            {
                OnFollow();
            }
        }


        protected virtual void OnEnable()
        {
            if (moment.IsFlagSet(FollowMoment.OnPreRender))
            {
                Camera.onPreRender += HandleCameraCallbacks;
            }

            if (moment.IsFlagSet(FollowMoment.OnPreCull))
            {
                Camera.onPreCull += HandleCameraCallbacks;
            }
        }

        protected virtual void OnDisable()
        {
            if (moment.IsFlagSet(FollowMoment.OnPreRender))
            {
                Camera.onPreRender -= HandleCameraCallbacks;
            }

            if (moment.IsFlagSet(FollowMoment.OnPreCull))
            {
                Camera.onPreCull -= HandleCameraCallbacks;
            }
        }

        protected abstract void OnFollow();
    }
}