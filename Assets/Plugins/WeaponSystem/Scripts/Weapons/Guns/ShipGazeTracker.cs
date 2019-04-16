using NaughtyAttributes;
using UnityEngine;
using UnityTools;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public class ShipGazeTracker : MonoBehaviour
    {
        struct Offset
        {
            public float top;
            public float bottom;
            public float left;
            public float right;

            public Offset(float left, float right, float up, float down)
            {
                this.top = up;
                this.bottom = down;
                this.left = left;
                this.right = right;
            }
        }

        [SerializeField] WeaponControl weaponControl;
        [SerializeField] ShipWeapon weapon;
        [SerializeField] float rotationSpeed;

        [MinMaxSlider(-120, 120)]
        [SerializeField]
        Vector2 horizontalTrackingRange;

        [MinMaxSlider(-120, 120)]
        [SerializeField]
        Vector2 verticalTrackingRange;

        void Update()
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, QueryGazeDirection(), rotationSpeed * Time.deltaTime);

        }

        Quaternion QueryGazeDirection()
        {
            if (!weaponControl.PointerDirectionService.TargetDirection(out _,
                                                                       out _,
                                                                       out var hitPoint,
                                                                       out _))
            {
                return NeutralPosition();
            }

            var neutral = weaponControl.NeutralRotation;
            var up = neutral * Vector3.up;
            return Quaternion.LookRotation((hitPoint - weapon.transform.position), up);
        }

        Quaternion NeutralPosition()
        {
            var shipDirection = weaponControl.NeutralRotation;
            var horizontal = (horizontalTrackingRange.x + horizontalTrackingRange.y) / 2;
            var vertical = (verticalTrackingRange.x + verticalTrackingRange.y) / 2;
            return shipDirection * Quaternion.Euler(horizontal, vertical, 0);
        }

        void OnDrawGizmosSelected()
        {
            DrawScanRange(transform.position, transform.forward, transform.up, 10,
                          new Offset(horizontalTrackingRange.x, horizontalTrackingRange.y,
                                     verticalTrackingRange.x, verticalTrackingRange.y));
        }

        static void DrawScanRange(Vector3 position, Vector3 forward, Vector3 up, float rayRange, Offset angles)
        {
            var baseRotation = Quaternion.LookRotation(forward, up);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(position + baseRotation * Vector3.left * rayRange / 2, position + baseRotation * Vector3.right * rayRange / 2);

            var upRotation = Quaternion.Euler(angles.top, 0, 0);
            var normal = baseRotation * upRotation * Vector3.up * rayRange;
            var forwardRay = baseRotation * upRotation * Vector3.forward;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(position, normal);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(position, forwardRay * rayRange);


            Gizmos.color = Color.yellow;
            DrawHorizontalArc(position, baseRotation, rayRange, angles.bottom, angles.left, angles.right);
            DrawHorizontalArc(position, baseRotation, rayRange, angles.top, angles.left, angles.right);
            Gizmos.color = Color.white;
            DrawHorizontalArc(position, baseRotation, rayRange, 0, angles.left, angles.right);

            DrawVerticalArc(position, baseRotation, rayRange, 0, angles.top, angles.bottom);
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

            GameObjectTools.DrawWireArc(position, baseRotation * normal * Vector3.right,
                                        startDirection,
                                        bottomAngle - topAngle, rayRange);


        }

        static void DrawHorizontalArc(Vector3 position, Quaternion baseRotation, float rayRange,
                                      float verticalAngle, float leftAngle, float rightAngle)
        {

            var left = Quaternion.Euler(0, leftAngle, 0);
            var right = Quaternion.Euler(0, rightAngle, 0);
            var down = Quaternion.Euler(verticalAngle, 0, 0);
            var startDirection = baseRotation * down * left * Vector3.forward;
            var endDirection = baseRotation * down * right * Vector3.forward;

            Gizmos.DrawRay(position, startDirection * rayRange);
            Gizmos.DrawRay(position, endDirection * rayRange);
            GameObjectTools.DrawWireArc(position, baseRotation * down * Vector3.up,
                                        startDirection,
                                        rightAngle - leftAngle, rayRange);

        }
    }
}