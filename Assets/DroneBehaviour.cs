using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the drone to fly in a flock formation
/// </summary>
public class DroneBehaviour : MonoBehaviour
{
    private List<Transform> _otherDrones;

    private void Awake()
    {
        DroneBehaviour[] droneScripts = GameObject.FindObjectsOfType<DroneBehaviour>();
        foreach (DroneBehaviour drone in droneScripts)
        {
            if (drone == this) continue;
            _otherDrones.Add(drone.transform);
        }
    }
    
    
}
