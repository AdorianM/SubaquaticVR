using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSliderValueChangedBoid : MonoBehaviour
{
    private BoidManager bm;

    private void Start()
    {
        bm = GetComponent<BoidManager>();
    }

    public void OnMinSpeedValueChanged(float value)
    {
        bm.settings.minSpeed = value;
    }

    public void OnMaxSpeedValueChanged(float value)
    {
        bm.settings.maxSpeed = value;
    }

    public void OnPerceptionRadiusValueChanged(float value)
    {
        bm.settings.perceptionRadius = value;
    }

    public void OnAvoidanceRadiusValueChanged(float value)
    {
        bm.settings.avoidanceRadius = value;
    }

    public void OnMaxSteerValueChanged(float value)
    {
        bm.settings.maxSteerForce = value;
    }

    public void OnAlignWeightValueChanged(float value)
    {
        bm.settings.alignWeight = value;
    }

    public void OnCohesionWeightValueChanged(float value)
    {
        bm.settings.cohesionWeight = value;
    }
    public void OnSeparateWeightValueChanged(float value)
    {
        bm.settings.seperateWeight = value;
    }
    public void OnTargetWeightValueChanged(float value)
    {
        bm.settings.targetWeight = value;
    }
}
