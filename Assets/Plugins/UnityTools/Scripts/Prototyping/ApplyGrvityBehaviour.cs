using UnityEngine;

namespace UnityTools.Prototyping
{
    [RequireComponent(typeof(CharacterController))]
    public class ApplyGrvityBehaviour : MonoBehaviour
    {

        RaycastHit[] hits;
        bool grounded;
        CharacterController controller;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            hits = new RaycastHit[1];
        }

        void FixedUpdate()
        {
            var travel = Physics.gravity * Time.fixedDeltaTime;
            controller.Move(travel);
        }
    }
}