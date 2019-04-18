using NaughtyAttributes;
using UnityEngine;

namespace UnityTools.Prototyping
{
  public class ApplyForce: MonoBehaviour
  {
    public GameObject Source;
    public GameObject Target;
    public int PushForce;
    [MinMaxSlider(0, 1)]
    public Vector2 RandomRange;
    public ForceMode ForceMode;
    public float TraceTargetTime;
    public bool ContinuousForce;

    Rigidbody rigidBody;
    GameObject target;
    float traceEndTime;

    void Reset()
    {
      ForceMode = ForceMode.Force;
    }

    void Awake()
    {
      rigidBody = Target.GetComponent<Rigidbody>();
      target = Target ?? this.gameObject;
    }

    void FixedUpdate()
    {
      char forceIndicator = ' ';
      if (Input.GetMouseButtonDown(0) || (ContinuousForce && Input.GetMouseButton(0)))
      {
        Debug.Log("Apply Force");
        var direction = target.transform.position - Source.transform.position;
        rigidBody.AddForce(direction * PushForce, ForceMode);
        traceEndTime = Time.time + TraceTargetTime;
        forceIndicator = '*';
      }

      if (Time.time < traceEndTime)
      {
        Debug.Log(forceIndicator + "Target Velocity: " + target.name + 
                  " => (Linear:" + rigidBody.velocity + ", Angular:" + rigidBody.angularVelocity);
      }
    }
  }
}