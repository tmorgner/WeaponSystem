using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.Prototyping
{
    public class TraceCollisionsBehaviour : MonoBehaviour
    {
        [SerializeField] bool traceEnabled;

        void Reset()
        {
            traceEnabled = true;
        }

        void OnEnable()
        {
            // do nothing, but this tricks unity into showing the enable/disable controls.
        }

        void OnCollisionEnter(Collision other)
        {
            if (traceEnabled)
            {
                Debug.Log("On Collision Enter: Self=" + gameObject.GetPath() + " other=" + other.gameObject.GetPath());
            }
        }

        void OnCollisionExit(Collision other)
        {
            if (traceEnabled)
            {
                Debug.Log("On Collision Exit: Self=" + gameObject.GetPath() + " other=" + other.gameObject.GetPath());
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (traceEnabled)
            {
                Debug.Log("On Trigger Enter: Self=" + gameObject.GetPath() + " other=" + other.gameObject.GetPath());
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (traceEnabled)
            {
                Debug.Log("On Trigger Exit: Self=" + gameObject.GetPath() + " other=" + other.gameObject.GetPath());
            }
        }
    }
}