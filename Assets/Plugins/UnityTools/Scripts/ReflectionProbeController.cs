using UnityEngine;
using UnityEngine.Rendering;

namespace RabbitStewdio.Unity.UnityTools
{
    public class ReflectionProbeController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] float refreshInterval;
#pragma warning restore 649
        ReflectionProbe probe;
        bool rendering;
        int renderId;
        float nextRenderTime;

        void Awake()
        {
            probe = GetComponent<ReflectionProbe>();
            if (probe != null)
            {
                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
            }
            else
            {
                enabled = false;
            }
        }

        void Update()
        {
            if (probe == null)
            {
                return;
            }

            if (rendering)
            {
                if (!probe.IsFinishedRendering(renderId))
                {
                    return;
                }

                rendering = false;
            }

            if (nextRenderTime < Time.time)
            {
                renderId = probe.RenderProbe();
                rendering = true;
                nextRenderTime = Time.time + refreshInterval;
            }
        }
    }
}