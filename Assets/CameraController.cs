using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float cameraSens;

    private float xRotation = 0;
    private float yRotation = 0;
    
    void Update()
    {
        
        transform.Translate(
            Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed, 
            Input.GetAxis("Jump") * Time.deltaTime * moveSpeed, 
            Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed, Space.Self);
        

        xRotation += -Input.GetAxis("Mouse Y") * cameraSens;
        yRotation += Input.GetAxis("Mouse X") * cameraSens;
        xRotation = Mathf.Clamp(xRotation, -85, 85);
        yRotation %= 360;
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
