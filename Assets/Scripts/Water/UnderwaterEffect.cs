using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UnderwaterEffect : MonoBehaviour
{
    public Material mat;

    [Range(0.001f, 0.1f)]
    public float pixelOffset = 0.001f;
    [Range(0.1f, 20f)]
    public float noiseScale = 0.1f;
    [Range(0.1f, 20f)]
    public float noiseFrequency = 0.1f;
    [Range(0.1f, 30f)]
    public float noiseSpeed = 0.1f;

    public float depthStart = 0;
    public float depthDistance = 200;

    void Update()
    {
        mat.SetFloat("_PixelOffset", pixelOffset);
        mat.SetFloat("_NoiseScale", noiseScale);
        mat.SetFloat("_NoiseFrequency", noiseFrequency);
        mat.SetFloat("_NoiseSpeed", noiseSpeed);
        mat.SetFloat("_DepthStart", depthStart);
        mat.SetFloat("_DepthDistance", depthDistance);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
