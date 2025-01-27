using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies force and torque to a drone proportional to the <see cref="Throttle"/>
/// </summary>
public class Propeller : MonoBehaviour
{
    /// <summary>
    /// The maximum amount of force the propeller can generate
    /// </summary>
    public float MaxForce
    {
        get { return maxForce; }
        set { maxForce = value; }
    }

    [SerializeField] private float maxForce;

    [SerializeField] private bool clockwise;
    
    /// <summary>
    /// The amount of force the propeller will apply. Limited by <see cref="MaxForce"/>
    /// </summary>
    public float Throttle
    {
        get { return _throttle; }
        set { _throttle = value; }
    }
    
    private float _throttle;

    /// <summary>
    /// Propeller torque coefficient
    /// </summary>
    [SerializeField] private float torque;
    
    [SerializeField] private Rigidbody rigid;

    private void FixedUpdate()
    {
        // Apply Force
        Vector3 force = transform.up * Throttle;
        force = Vector3.ClampMagnitude(force, MaxForce);
        rigid.AddForceAtPosition(force, transform.position);
        
        // Apply Torque
        int direction;
        if (clockwise) direction = 1;
        else direction = -1;
        rigid.AddRelativeTorque(Vector3.up * (transform.localPosition.magnitude * force.magnitude * torque * direction));
        
        Debug.DrawRay(transform.position, force, Color.green);
    }
}
