using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    [Range(100f, 500f)]
    public float mouseSensitivity = 100f;
    public Transform controllerBody;

    float xRotation = 0f;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = 0f;
        float mouseY = 0f;
        if(Input.GetMouseButton(1))
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }


        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Using localRotation instead of Rotate to be able to Clamp down to 180 deg.
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        controllerBody.Rotate(Vector3.up * mouseX);
       
    }
}
