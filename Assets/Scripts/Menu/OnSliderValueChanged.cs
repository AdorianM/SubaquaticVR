using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSliderValueChanged : MonoBehaviour
{
    private NoiseDensity nd;

    private void Start()
    {
        nd = GetComponent<NoiseDensity>();
    }

    private void UpdateMesh()
    {
        if (FindObjectOfType<MeshGenerator>())
        {
            FindObjectOfType<MeshGenerator>().RequestMeshUpdate();
        }
    }

    public void OnSeedValueChanged(float value)
    {
        nd.seed = Mathf.RoundToInt(value);
        UpdateMesh();
    }

    public void OnOctaveValueChanged(float value)
    {
        nd.numOctaves = Mathf.RoundToInt(value);
        UpdateMesh();
    }

    public void OnLacunarityValueChanged(float value)
    {
        nd.lacunarity = value;
        UpdateMesh();
    }

    public void OnPersistanceValueChanged(float value)
    {
        nd.persistence = value;
        UpdateMesh();
    }

    public void OnNoiseScaleValueChanged(float value)
    {
        nd.noiseScale = value;
        UpdateMesh();
    }

    public void OnNoiseWeightValueChanged(float value)
    {
        nd.noiseWeight = value;
        UpdateMesh();
    }

    public void OnCloseEdgesValueChanged(float value)
    {
        nd.closeEdges = value > 0;
        UpdateMesh();
    }

    public void onFloorOffsetValueChanged(float value)
    {
        nd.floorOffset = value;
        UpdateMesh();
    }

    public void onWeightMultiplierValueChanged(float value)
    {
        nd.weightMultiplier = value;
        UpdateMesh();
    }

    public void onXInfluenceValueChanged(float value)
    {
        nd.shaderParams.x = value;
        UpdateMesh();
    }

    public void onYInfluenceValueChanged(float value)
    {
        nd.shaderParams.y = value;
        UpdateMesh();
    }
}
