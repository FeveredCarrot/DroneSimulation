using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneTargeter : MonoBehaviour
{
    [SerializeField] private Transform targetPos;
    [SerializeField] private float targetDist = 1;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            targetPos.position = transform.position + (transform.forward * targetDist);
        }
    }
}
