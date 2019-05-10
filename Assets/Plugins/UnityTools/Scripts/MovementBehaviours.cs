using UnityEngine;

public class MovementBehaviours
{
    /// <summary>
    ///  Arrive behaviour. Return true if at target, false otherwise. 
    ///  Desired velocity is given as out parameter.
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="targetRadius"></param>
    /// <param name="slowRadius"></param>
    /// <param name="position"></param>
    /// <param name="maxSpeed"></param>
    /// <param name="velocity"></param>
    /// <returns></returns>
    public static bool MoveTowards2D(Vector2 targetPosition,
                                     float targetRadius,
                                     float slowRadius,
                                     Vector2 position,
                                     float maxSpeed,
                                     out Vector2 velocity)
    {
        var direction = targetPosition - position;
        var distance = direction.magnitude;

        if (distance < targetRadius)
        {
            velocity = Vector2.zero;
            return true;
        }

        float targetSpeed;
        if (slowRadius > 0)
        {
            targetSpeed = maxSpeed * Mathf.Clamp01(distance / slowRadius);
        }
        else
        {
            targetSpeed = maxSpeed;
        }

        var targetVelocity = direction;
        targetVelocity.Normalize();
        targetVelocity *= targetSpeed;

        velocity = targetVelocity;
        return false;
    }
    
    /// <summary>
    ///  Arrive behaviour. Return true if at target, false otherwise. 
    ///  Desired velocity is given as out parameter.
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="targetRadius"></param>
    /// <param name="slowRadius"></param>
    /// <param name="position"></param>
    /// <param name="maxSpeed"></param>
    /// <param name="velocity"></param>
    /// <returns></returns>
    public static bool MoveTowards3D(Vector3 targetPosition,
                                     float targetRadius,
                                     float slowRadius,
                                     Vector3 position,
                                     float maxSpeed,
                                     out Vector3 velocity)
    {
        if (targetPosition == position)
        {
            velocity = Vector2.zero;
            return true;
        }

        var direction = targetPosition - position;
        var distance = direction.magnitude;

        if (distance < targetRadius)
        {
            velocity = Vector2.zero;
            return true;
        }

        float targetSpeed;
        if (slowRadius > 0)
        {
            targetSpeed = maxSpeed * Mathf.Clamp01(distance / slowRadius);
        }
        else
        {
            targetSpeed = maxSpeed;
        }

        var targetVelocity = direction;
        targetVelocity.Normalize();
        targetVelocity *= targetSpeed;

        velocity = targetVelocity;
        return false;
    }
}