using System;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public class TriggerEventForwarder: MonoBehaviour
    {
        public event Action<Collider> TriggerEnter;
        public event Action<Collider> TriggerStay;
        public event Action<Collider> TriggerExit;

        void OnTriggerEnter(Collider other)
        {
            TriggerEnter?.Invoke(other);
        }

        void OnTriggerExit(Collider other)
        {
            TriggerExit?.Invoke(other);
        }

        void OnTriggerStay(Collider other)
        {
            TriggerStay?.Invoke(other);
        }
    }
}
