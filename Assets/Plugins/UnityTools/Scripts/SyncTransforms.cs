using System;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    [DefaultExecutionOrder(-10001)]
    public class SyncTransforms: MonoBehaviour
    {
        void OnDisable()
        {
            Physics.autoSyncTransforms = true;
        }

        void FixedUpdate()
        {
            Physics.SyncTransforms();
        }

        void Update()
        {
            Physics.SyncTransforms();
        }

        void OnEnable()
        {
            Physics.autoSyncTransforms = false;
        }
    }
}
