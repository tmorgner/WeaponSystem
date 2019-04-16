using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools.Prototyping
{
    public class AutoRotate : MonoBehaviour
    {
        [SerializeField] Vector3 velocity;

        void Update()
        {
            transform.rotation *= Quaternion.Euler(Time.deltaTime * velocity);
        }
    }
}