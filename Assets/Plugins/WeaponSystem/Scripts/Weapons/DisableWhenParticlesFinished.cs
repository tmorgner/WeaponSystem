using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons
{
    public class DisableWhenParticlesFinished : MonoBehaviour
    {
        ParticleSystem[] particles;
        int frameCountActive;

        void Awake()
        {
            particles = GetComponentsInChildren<ParticleSystem>();
        }

        void OnEnable()
        {
            frameCountActive = Time.frameCount;
        }

        void Update()
        {
            if (Time.frameCount == frameCountActive)
            {
                return;
            }

            if (!IsPlaying())
            {
                gameObject.SetActive(false);
            }
        }

        bool IsPlaying()
        {
            if (particles == null)
            {
                return false;
            }

            foreach (var p in particles)
            {
                if (p.isPlaying)
                {
                    return true;
                }
            }

            return false;
        }
    }
}