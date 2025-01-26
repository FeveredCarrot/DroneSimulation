using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    [SerializeField] private Propeller[] props;

    [SerializeField] private Vector3 movementSpeed, movementDampening, rotationSpeed, rotationDampening;

    [SerializeField] private Vector3 targetPos;
    [SerializeField] private Transform targetTransform;
    private Vector3 targetForce = Vector3.zero;

    private Rigidbody _rigid;

    private Vector3 _lastPosError = Vector3.zero;
    private Vector3 _lastDirError = Vector3.zero;
    private Vector3 PosError
    {
        get
        {
            return targetPos - transform.position;
        }
    }
    
    private Vector3 PosErrorDelta
    {
        get
        {
            return PosError - _lastPosError;
        }
    }
    
    private Vector3 DirError
    {
        get
        {
            Vector3 dirError = transform.InverseTransformVector(targetForce);
            if (targetForce.y < 0)
            {
                dirError = -dirError;
            }
            
            float yawInputDefault = Vector3.SignedAngle(transform.forward, Vector3.forward, transform.up) / 360f;
            float yawInputTowardsTarget = Vector3.SignedAngle(transform.forward, PosError, transform.up) / 360f;
            Vector3 longitudinalError = Vector3.ProjectOnPlane(PosError, Vector3.up);
            dirError.y = Mathf.Lerp(yawInputDefault, yawInputTowardsTarget, longitudinalError.magnitude - 3f);
            
            return dirError;
            
        }
    }
    
    private Vector3 DirErrorDelta
    {
        get
        {
            return DirError - _lastDirError;
        }
    }

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

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
        _lastPosError = PosError;
        _lastDirError = DirError;
    }

    private void FixedUpdate()
    {
        if (targetTransform) targetPos = targetTransform.position;
        
        Vector3 proportionalInput = PosError;
        proportionalInput.x *= movementSpeed.x;
        proportionalInput.y *= movementSpeed.y;
        proportionalInput.z *= movementSpeed.z;
        proportionalInput = Vector3.ClampMagnitude(proportionalInput, TotalMaxForce);

        Vector3 dampeningInput = PosErrorDelta;
        dampeningInput.x *= movementDampening.x;
        dampeningInput.y *= movementDampening.y;
        dampeningInput.z *= movementDampening.z;
        //dampeningInput = Vector3.ClampMagnitude(dampeningInput, TotalMaxForce);

        Vector3 externalForceInput = -Physics.gravity * _rigid.mass;

        targetForce = proportionalInput + dampeningInput + externalForceInput;
        //targetForce = Vector3.ClampMagnitude(targetForce, TotalMaxForce);
        Debug.DrawRay(transform.position, targetForce, Color.red);
        
        // Vertical Axis
        foreach (Propeller prop in props)
        {
            prop.Throttle = Vector3.Dot(transform.up, targetForce);
        }

        //targetForce.y = 0;
        
        Vector3 rotationProportionalInput = new Vector3(
            DirError.x * rotationSpeed.x, 
            DirError.y * rotationSpeed.y,
            DirError.z * rotationSpeed.z);
        
        Vector3 rotationDampeningInput = new Vector3(
            DirErrorDelta.x * rotationDampening.x, 
            DirErrorDelta.y * rotationDampening.y, 
            DirErrorDelta.z * rotationDampening.z);
        
        Vector3 directionInput = rotationProportionalInput + rotationDampeningInput;


        Debug.Log(DirError.y);
        props[0].Throttle += directionInput.x - directionInput.z + directionInput.y;
        props[1].Throttle += -directionInput.x - directionInput.z - directionInput.y;
        props[2].Throttle += directionInput.x + directionInput.z - directionInput.y;
        props[3].Throttle += -directionInput.x + directionInput.z + directionInput.y;
        
        _lastPosError = PosError;
        _lastDirError = DirError;
    }
}
