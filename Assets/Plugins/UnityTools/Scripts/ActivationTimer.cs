using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public class ActivationTimer
    {
        float startTime;
        float frameTime;
        float lastFrameTime;
        float deltaSum;

        public float DeltaTime => frameTime - lastFrameTime;
        public float TimePassed => frameTime - startTime;

        // ReSharper disable once UnusedMember.Global
        public float Drift => deltaSum - frameTime;

        public void Start()
        {
            startTime = Time.time;
            frameTime = startTime;
            lastFrameTime = startTime;
            deltaSum = 0;
        }

        public void StartFixed()
        {
            startTime = Time.fixedTime;
            frameTime = startTime;
            lastFrameTime = startTime;
            deltaSum = 0;
        }

        public void Update()
        {
            deltaSum += Time.deltaTime;
            lastFrameTime = frameTime;
            frameTime = Time.time;
        }

        public void FixedUpdate()
        {
            deltaSum += Time.fixedDeltaTime;
            lastFrameTime = frameTime;
            frameTime = Time.fixedTime;
        }
    }
}