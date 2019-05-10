using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RabbitStewdio.Unity.UnityTools
{
    /// <summary>
    ///   A MonoBehaviour that sends update events at semi-random intervals.
    /// </summary>
    public class TickBehaviour: MonoBehaviour
    {
        public event Action OnActivate;
        public event Action OnDeactivate;
        public event Action OnUpdate;
        [SerializeField] float tickInterval; 
        [SerializeField] float tickDeviation;
        float nextTick;

        public void SetUp(float tickInterval, float tickDeviation)
        {
            this.tickDeviation = tickDeviation;
            this.tickInterval = tickInterval;
            nextTick = NextTimeForTick;
        } 
        
        void Update()
        {
            if (nextTick > Time.time)
            {
                return;
            }
            
            OnUpdate?.Invoke();
            nextTick = NextTimeForTick;
        }

        float NextTimeForTick => tickInterval + Time.time + Random.value * tickDeviation - tickDeviation / 2;

        void OnDisable()
        {
            OnDeactivate?.Invoke();
        }

        void OnEnable()
        {
            OnActivate?.Invoke();
            nextTick = NextTimeForTick;
        }
        
        
    }
}