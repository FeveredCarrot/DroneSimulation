using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneuvers the drone to the target position
/// </summary>
public class DroneController : MonoBehaviour
{
    /// <summary>
    /// References to the four propellers in order of top left, top right, bottom left, bottom right
    /// </summary>
    [SerializeField] private Propeller[] props;

    /// <summary>
    /// PID controller coefficient
    /// </summary>
    [SerializeField] private Vector3 movementSpeed, movementDampening, rotationSpeed, rotationDampening;

    /// <summary>
    /// Position the drone will navigate towards.
    /// </summary>
    public Vector3 TargetPos
    {
        get
        {
            if (targetTransform) return targetTransform.position;
            return _targetPos;
        }
        set
        {
            if (!targetTransform) _targetPos = value;
        }
        
    }
    
    private Vector3 _targetPos;
    
    /// <summary>
    /// If set, <see cref="TargetPos"/> will be locked to the position of <see cref="targetTransform"/>.
    /// </summary>
    public Transform targetTransform;
    
    private Vector3 _lastTargetPos = Vector3.zero;
    private Vector3 _lastPosError = Vector3.zero;
    private Vector3 _lastRotError = Vector3.zero;
    
    private Rigidbody _rigid;

    /// <summary>
    /// Target force to apply for the drone to navigate to <see cref="TargetPos"/>. Controlled by positional PID
    /// </summary>
    private Vector3 TargetForce()
    {
        Vector3 proportionalInput = PosError;
        proportionalInput.x *= movementSpeed.x;
        proportionalInput.y *= movementSpeed.y;
        proportionalInput.z *= movementSpeed.z;
        proportionalInput = Vector3.ClampMagnitude(proportionalInput, TotalMaxForce);

        Vector3 dampeningInput = PosErrorDelta;
        dampeningInput.x *= movementDampening.x;
        dampeningInput.y *= movementDampening.y;
        dampeningInput.z *= movementDampening.z;
        
        Vector3 externalForceInput = -Physics.gravity * _rigid.mass / 4f;
        
        Vector3 targetForce = proportionalInput + dampeningInput + externalForceInput;
        
        Debug.DrawRay(transform.position, targetForce, Color.red);

        return targetForce;
    }
    
    /// <summary>
    /// Difference between drone position and target position
    /// </summary>
    private Vector3 PosError => TargetPos - transform.position;

    /// <summary>
    /// Change in positional error this physics step
    /// </summary>
    private Vector3 PosErrorDelta
    {
        get
        {
            // Change in error should not include movement of the target position
            // Only account for changes in error due to the drone's movement
            Vector3 targetPosDelta = _lastTargetPos - TargetPos;
            return PosError - _lastPosError + targetPosDelta;
        }
    }
    
    /// <summary>
    /// Rotational error
    /// </summary>
    private Vector3 RotError(Vector3 targetForce)
    {
        // Rotation error is X and Z axis of target force in local space
        // Drone's Y axis should align with target force vector
        Vector3 dirError = transform.InverseTransformVector(targetForce);
        
        // Do not flip drone when descending, reverse propeller
        if (targetForce.y < 0)
        {
           // dirError = -dirError;
        }
        
        // Yaw to face the target position. If arrived at the target position, yaw to face world forward.
        float yawInputTowardsTarget = Vector3.SignedAngle(transform.forward, PosError, transform.up) / 360f;
        float yawInputDefault = Vector3.SignedAngle(transform.forward, Vector3.forward, transform.up) / 360f;
        // Only yaw to face target position when not arrived at target position
        // Do not yaw if 
        Vector3 longitudinalError = Vector3.ProjectOnPlane(PosError, Vector3.up);
        dirError.y = Mathf.Lerp(yawInputDefault, yawInputTowardsTarget, longitudinalError.magnitude - 3f);
        
        return dirError;
    }
    
    /// <summary>
    /// Change in rotation error this physics step
    /// </summary>
    /// <param name="rotError"></param>
    /// <returns></returns>
    private Vector3 RotErrorDelta(Vector3 rotError)
    {
        return rotError - _lastRotError;
    }

    /// <summary>
    /// The maximum total force that can be applied by all four propellers
    /// </summary>
    private float TotalMaxForce
    {
        get
        {
            float maxForce = 0;
            
            foreach (Propeller prop in props)
            {
                maxForce += prop.MaxForce;
            }

            return maxForce;
        }
    }

    /// <summary>
    /// The amount to rotate around each axis to align the drone with the <see cref="TargetForce"/> vector.
    /// </summary>
    private Vector3 RotationInput(Vector3 rotError, Vector3 rotErrorDelta)
    {
        Vector3 rotationProportionalInput = new Vector3(
            rotError.x * rotationSpeed.x, 
            rotError.y * rotationSpeed.y,
            rotError.z * rotationSpeed.z);
        
        Vector3 rotationDampeningInput = new Vector3(
            rotErrorDelta.x * rotationDampening.x, 
            rotErrorDelta.y * rotationDampening.y, 
            rotErrorDelta.z * rotationDampening.z);
        
        Vector3 rotationInput = rotationProportionalInput + rotationDampeningInput;
        return rotationInput;
    }

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
        _lastPosError = PosError;
        _lastRotError = RotError(TargetForce());
    }

    private void FixedUpdate()
    {
        Vector3 targetForce = TargetForce();
        
        // Set base throttle for all props to keep drone hovering / moving vertically towards target
        foreach (Propeller prop in props)
        {
            prop.Throttle = Vector3.Dot(targetForce, transform.up);
        }
        
        Vector3 rotError = RotError(targetForce);
        Vector3 rotErrorDelta = RotErrorDelta(rotError);
        Vector3 rotationInput = RotationInput(rotError, rotErrorDelta);
        
        // Adjust propeller throttle to rotate drone towards target rotation
        props[0].Throttle += rotationInput.x - rotationInput.z + rotationInput.y;
        props[1].Throttle += -rotationInput.x - rotationInput.z - rotationInput.y;
        props[2].Throttle += rotationInput.x + rotationInput.z - rotationInput.y;
        props[3].Throttle += -rotationInput.x + rotationInput.z + rotationInput.y;
        
        // Update previous error values for calculating deltas
        _lastPosError = PosError;
        _lastRotError = rotError;
        _lastTargetPos = TargetPos;
    }
}
