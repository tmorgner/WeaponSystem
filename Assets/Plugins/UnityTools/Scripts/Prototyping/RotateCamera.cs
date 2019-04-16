using UnityEngine;

namespace UnityTools.Prototyping
{
  public class RotateCamera: MonoBehaviour
  {
    public GameObject Camera;
    public float Velocity;
    public float ZoomSpeed;

    void Update()
    {
      if (Input.GetMouseButton(1))
      {
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * Velocity);
        transform.Rotate(Vector3.right, Input.GetAxis("Mouse Y") * Velocity);
      }
       
      transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * Velocity);
      transform.Rotate(Vector3.right, Input.GetAxis("Vertical") * Velocity);

      var zoom = Input.GetAxis("Mouse ScrollWheel");
      var directionChange = Camera.transform.forward * ZoomSpeed * zoom;
      Camera.transform.position += directionChange;
    }
  }
}