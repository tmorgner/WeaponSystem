using NaughtyAttributes;
using UnityEngine;
using UnityTools;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    /// <para>
    /// A weapon direction tracker attempts to align the gun's swiveling mount
    /// with the target that is tracked by another component.
    /// </para>
    /// <para>
    /// This library provides two implementations. The <see cref="WeaponAutoTargetDirectionTracker"/>
    /// is used for automatic guns and will point the gun at the currently selected
    /// target. The <see cref="RabbitStewdio.Unity.WeaponSystem.Weapons.WeaponPlatform.PlayerWeaponDirectionTracker"/>
    /// uses player input (for instance an VR pointer or the head movement) as target
    /// point. This allows you to point the gun towards the target the player is looking
    /// or pointing.
    /// </para>
    /// </summary>
    public abstract class WeaponDirectionTracker : MonoBehaviour
    {
        readonly struct Offset
        {
            public readonly float Top;
            public readonly float Bottom;
            public readonly float Left;
            public readonly float Right;

            public Offset(float left, float right, float up, float down)
            {
                this.Top = up;
                this.Bottom = down;
                this.Left = left;
                this.Right = right;
            }
        }

        [Required]
        [SerializeField]
        [Tooltip("The rotating weapon mount.")]
        Transform weaponMount;

        [Tooltip("The rotation speed. A speed of 1 means the gun will align with the target within 1 second. Match this to your weapon charge time for best results.")]
        [SerializeField] float rotationSpeed;

        [MinMaxSlider(-180, 180)]
        [SerializeField]
        Vector2 horizontalTrackingRange;

        [MinMaxSlider(-180, 180)]
        [SerializeField]
        Vector2 verticalTrackingRange;

        [Required]
        [SerializeField] 
        [Tooltip("A transform that represents the neutral position. Use the weapon base for this point.")]
        Transform neutralPosition;

        /// <summary>
        /// A transform that represents the neutral position. Use the weapon base for this point.
        /// </summary>
        public Transform NeutralPosition => neutralPosition;

        /// <summary>
        /// The rotating weapon mount.
        /// </summary>
        public Transform WeaponMount => weaponMount;

        /// <summary>
        ///  The rotation speed. A speed of 1 means the gun will align with the target within 1 second. Match this to your weapon charge time for best results.
        /// </summary>
        public float RotationSpeed => rotationSpeed;

        /// <summary>
        ///   The Horizontal tracking range relative to the neutral forward direction. The x component provides the minimum (left)
        ///   and the y component the maximum (right) rotation.
        /// </summary>
        public Vector2 HorizontalTrackingRange => horizontalTrackingRange;

        /// <summary>
        ///   The Horizontal tracking range relative to the neutral forward direction. The x component provides the minimum (up)
        ///   and the y component the maximum (down) rotation.
        /// </summary>
        public Vector2 VerticalTrackingRange => verticalTrackingRange;

        /// <summary>
        ///   A standard Unity event handler.
        /// </summary>
        protected void Update()
        {
            weaponMount.rotation = Quaternion.Lerp(weaponMount.rotation, QueryGazeDirection(), rotationSpeed * Time.deltaTime);
            UpdateOverride();
        }

        /// <summary>
        ///   An Unity-Event override point.
        /// </summary>
        protected virtual void UpdateOverride()
        {
        }

        /// <summary>
        ///   Returns the gaze direction of the gun. This normally should align with the
        ///   target point that is tracked or a neutral rotation if there is no target.
        /// </summary>
        /// <returns>The gaze direction rotation.</returns>
        protected abstract Quaternion QueryGazeDirection();

        /// <summary>
        ///   Returns a clamped neutral gaze direction.
        /// </summary>
        /// <returns>a clamped neutral gaze direction.</returns>
        protected Quaternion QueryNeutralPosition()
        {
            var horizontalTrackingRange = HorizontalTrackingRange;
            var verticalTrackingRange = VerticalTrackingRange;
            var neutral = NeutralPosition.rotation;
            var horizontal = (horizontalTrackingRange.x + horizontalTrackingRange.y) / 2;
            var vertical = (verticalTrackingRange.x + verticalTrackingRange.y) / 2;
            return neutral * Quaternion.Euler(horizontal, vertical, 0);
        }

        void OnDrawGizmosSelected()
        {
            if (weaponMount == null)
            {
                return;
            }

            DrawScanRange(weaponMount.position,
                          weaponMount.forward,
                          weaponMount.up,
                          10,
                          new Offset(horizontalTrackingRange.x,
                                     horizontalTrackingRange.y,
                                     verticalTrackingRange.x,
                                     verticalTrackingRange.y));
        }

        static void DrawScanRange(Vector3 position, Vector3 forward, Vector3 up, float rayRange, Offset angles)
        {
            var baseRotation = Quaternion.LookRotation(forward, up);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(position + baseRotation * Vector3.left * rayRange / 2, position + baseRotation * Vector3.right * rayRange / 2);

            var upRotation = Quaternion.Euler(angles.Top, 0, 0);
            var normal = baseRotation * upRotation * Vector3.up * rayRange;
            var forwardRay = baseRotation * upRotation * Vector3.forward;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(position, normal);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(position, forwardRay * rayRange);

            Gizmos.color = Color.yellow;
            DrawHorizontalArc(position, baseRotation, rayRange, angles.Bottom, angles.Left, angles.Right);
            DrawHorizontalArc(position, baseRotation, rayRange, angles.Top, angles.Left, angles.Right);
            Gizmos.color = Color.white;
            DrawHorizontalArc(position, baseRotation, rayRange, 0, angles.Left, angles.Right);

            DrawVerticalArc(position, baseRotation, rayRange, 0, angles.Top, angles.Bottom);
        }

        static void DrawVerticalArc(Vector3 position,
                                    Quaternion baseRotation,
                                    float rayRange,
                                    float horizontalAngle,
                                    float topAngle,
                                    float bottomAngle)
        {
            var top = Quaternion.Euler(topAngle, 0, 0);
            var bottom = Quaternion.Euler(bottomAngle, 0, 0);
            var normal = Quaternion.Euler(0, horizontalAngle, 0);
            var startDirection = baseRotation * top * normal * Vector3.forward;
            var endDirection = baseRotation * bottom * normal * Vector3.forward;

            Gizmos.DrawRay(position, startDirection * rayRange);
            Gizmos.DrawRay(position, endDirection * rayRange);

            GameObjectTools.DrawWireArc(position,
                                        baseRotation * normal * Vector3.right,
                                        startDirection,
                                        bottomAngle - topAngle,
                                        rayRange);
        }

        static void DrawHorizontalArc(Vector3 position,
                                      Quaternion baseRotation,
                                      float rayRange,
                                      float verticalAngle,
                                      float leftAngle,
                                      float rightAngle)
        {
            var left = Quaternion.Euler(0, leftAngle, 0);
            var right = Quaternion.Euler(0, rightAngle, 0);
            var down = Quaternion.Euler(verticalAngle, 0, 0);
            var startDirection = baseRotation * down * left * Vector3.forward;
            var endDirection = baseRotation * down * right * Vector3.forward;

            Gizmos.DrawRay(position, startDirection * rayRange);
            Gizmos.DrawRay(position, endDirection * rayRange);
            GameObjectTools.DrawWireArc(position,
                                        baseRotation * down * Vector3.up,
                                        startDirection,
                                        rightAngle - leftAngle,
                                        rayRange);
        }
    }
}
