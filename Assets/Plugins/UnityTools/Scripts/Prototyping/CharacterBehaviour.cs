using UnityEngine;
using UnityEngine.Events;

namespace UnityTools.Prototyping
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterBehaviour : MonoBehaviour
    {
        [SerializeField] KeyCode strafeKey;
        [SerializeField] KeyCode strafeKey2;
        [SerializeField] float forwardMovementSpeed;
        [SerializeField] float reverseMovementSpeed;
        [SerializeField] float strafeSpeed;
        [SerializeField] float movementThreshold;
        [SerializeField] float rotationSpeed;
        [SerializeField] bool independentHeadMovement;
        [SerializeField] bool invertHeadLook;
        CharacterController characterController;
        [SerializeField] Transform head;

        public CollisionFlags MovementResult { get; set; }
        public UnityEvent MovementBlocked;
        public UnityEvent Moving;

        void Reset()
        {
            strafeKey = KeyCode.LeftControl;
            strafeKey2 = KeyCode.RightControl;
            forwardMovementSpeed = 2;
            reverseMovementSpeed = 1;
            strafeSpeed = 1;
            movementThreshold = 0.1f;
            rotationSpeed = 5f;
        }

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }

        void FixedUpdate()
        {
            var h = Input.GetAxis("Horizontal");
            if (!independentHeadMovement)
            {
                h += Input.GetAxis("Mouse X");
            }

            var v = Input.GetAxis("Vertical");
            var velocity = Vector3.zero;
            if (v < 0)
            {
                velocity += transform.forward * v * reverseMovementSpeed * Time.fixedDeltaTime;
            }
            else
            {
                velocity += transform.forward * v * forwardMovementSpeed * Time.fixedDeltaTime;
            }

            if (Input.GetKey(strafeKey) || Input.GetKey(strafeKey2))
            {
                velocity += transform.right * h * strafeSpeed * Time.fixedDeltaTime;
            }
            else
            {
                transform.Rotate(Vector3.up, h * rotationSpeed * Time.fixedDeltaTime);
            }

            HandleMouseLook();

            MovementResult = characterController.Move(velocity);

            if (MovementResult != 0)
            {
                MovementBlocked?.Invoke();
            }

            if (characterController.velocity.magnitude > movementThreshold)
            {
                Moving?.Invoke();
            }
        }

        void HandleMouseLook()
        {
            var mouseLookAxisX = independentHeadMovement ? Input.GetAxis("Mouse X") : 0;
            var mouseLookAxisY = Input.GetAxis("Mouse Y") * (invertHeadLook ? 1 : -1);
            var angleY = mouseLookAxisX * Time.fixedDeltaTime * rotationSpeed;
            var angleX = mouseLookAxisY * Time.fixedDeltaTime * rotationSpeed;

            var eulers = head.localRotation.eulerAngles;
            eulers.y += angleY;
            eulers.x = Mathf.Clamp(Mathf.DeltaAngle(0, eulers.x + angleX), -45, 45);

            head.localRotation = Quaternion.Euler(eulers.x, eulers.y, 0);
        }
    }
}