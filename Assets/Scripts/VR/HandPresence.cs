using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public InputDeviceCharacteristics characteristics;
    public GameObject handModelPrefab;
    private GameObject handModel;

    private Animator handAnimator;
    private InputDevice targetDevice;
    

    void Start()
    {
        TryInitialize();
    }

    void TryInitialize()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        if (devices.Count > 0)
        {
            targetDevice = devices[0];

            handModel = Instantiate(handModelPrefab, transform);
            handAnimator = handModel.GetComponent<Animator>();
        }
    }

    void UpdateAnimation()
    {
        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }

        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }
    }
    void Update()
    {
        if(!targetDevice.isValid)
        {
            TryInitialize();
        } else
        {
            handModel.SetActive(true);
            UpdateAnimation();
        }
    }
}
