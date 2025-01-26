using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour
{
    public float MaxForce
    {
        get { return maxForce; }
        set { maxForce = value; }
    }

    [SerializeField]
    private float maxForce;

    [SerializeField] private bool clockwise;
    
    public float Throttle
    {
        get { return throttle; }
        set { throttle = value; }
    }
    
    [SerializeField] private float throttle;

    [SerializeField] private float torque;
    
    [SerializeField] private Rigidbody rigid;

    private void FixedUpdate()
    {
        Vector3 force = transform.up * Throttle;
        force = Vector3.ClampMagnitude(force, MaxForce);
        rigid.AddForceAtPosition(force, transform.position);
        int direction;
        if (clockwise) direction = 1;
        else direction = -1;
        rigid.AddRelativeTorque(Vector3.up * (transform.localPosition.magnitude * force.magnitude * torque * direction));
        Debug.DrawRay(transform.position, force, Color.green);
    }
}
