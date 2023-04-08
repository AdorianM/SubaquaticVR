using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointScatterer : MonoBehaviour
{

    public GameObject shape;
    [Range(0f, 50f)]
    public float RotationSpeed = 5f;
    
    public Vector3 Scale = new Vector3(0.1f, 0.1f, 0.1f);

    private List<GameObject> instances = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Vector3[] rays = BoidHelper.directions;
        

        for (int i = 0; i < rays.Length; i++)
        {
            shape.transform.localScale = Scale;
            Instantiate(shape, transform.position - rays[i], Quaternion.identity, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float rotation = RotationSpeed * Time.deltaTime;
        transform.Rotate(rotation, rotation, 0f);
    }

    private void Awake()
    {
        var oldSpheres = FindObjectsOfType<Sphere>();
        for (int i = oldSpheres.Length - 1; i >= 0; i--)
        {
            DestroyImmediate(oldSpheres[i].gameObject);
        }
    }
}
