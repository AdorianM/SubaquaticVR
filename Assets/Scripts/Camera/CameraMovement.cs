using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [Range(5f, 100f)]
    public float speed = 5f;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W)) {
            transform.position = transform.position + Camera.main.transform.forward * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.position = transform.position - Camera.main.transform.forward * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position = transform.position - Camera.main.transform.right * speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position = transform.position + Camera.main.transform.right * speed * Time.deltaTime;
        }

    }
}
