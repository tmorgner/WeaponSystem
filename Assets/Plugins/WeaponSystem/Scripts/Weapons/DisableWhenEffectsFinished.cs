using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons
{
    /// <summary>
    ///   A small utility script that disables the game object when a
    ///   effect has finished playing. This class supports particle
    ///   effects, sound and animations.
    /// </summary>
    [AddComponentMenu("Weapons/Helper/Disable When Effects Finished")]
    public class DisableWhenEffectsFinished : MonoBehaviour
    {
        ParticleSystem[] particles;
        AudioSource[] audioSources;
        Animation[] animations;
        int frameCountActive;

        void Awake()
        {
            particles = GetComponentsInChildren<ParticleSystem>();
            audioSources = GetComponentsInChildren<AudioSource>();
            animations = GetComponentsInChildren<Animation>();
        }

        void OnEnable()
        {
            frameCountActive = Time.frameCount;
        }

        void Update()
        {
            if (Time.frameCount == frameCountActive)
            {
                // dont test on the first frame, as the effect behaviours might need 
                // some time to become active.
                return;
            }

            if (!IsPlaying())
            {
                gameObject.SetActive(false);
            }
        }

        bool IsPlaying()
        {
            return IsPlayingParticles() || IsPlayingAudio() || IsPlayingAnimations();
        }

        bool IsPlayingParticles()
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

        bool IsPlayingAudio()
        {
            if (audioSources == null)
            {
                return false;
            }

            foreach (var p in audioSources)
            {
                if (p.isPlaying)
                {
                    return true;
                }
            }

            return false;
        }

        bool IsPlayingAnimations()
        {
            if (animations == null)
            {
                return false;
            }

            foreach (var p in animations)
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