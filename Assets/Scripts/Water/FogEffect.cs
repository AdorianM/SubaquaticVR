using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FogEffect : MonoBehaviour
{
    public Material mat;

    // Properties
    public Color fogColor;
    public float depthBegin;
    public float depthDistance;
    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    void Update()
    {
        mat.SetColor("_FogColor", fogColor);
        mat.SetFloat("_DepthStart", depthBegin);
        mat.SetFloat("_DepthDistance", depthDistance);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
