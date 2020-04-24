using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class DMXParameter : MonoBehaviour
{
    public string ParameterName;
    public byte address;
    public DMXInputResolution resolution;
    public int homeValue;
    public int minValue = 0;
    public int maxValue = 255;
    public FixtureFunction function;
    public FixtureComponent[] affectedComponents;
    public float currentValue;

    void Start()
    {
        currentValue = MapValue(homeValue);
    }

    void Update()
    {
        
    }

    public float MapValue(int value)
    {
        return (value - minValue) / (float)maxValue;
    }

    public void UpdateInput(int value)
    {
        // TODO clamp input value to actual range of DMXInputResolution
        currentValue = MapValue(value);
        //Debug.Log("New value: "+value);
        foreach (var component in affectedComponents)
        {
            component.SetParameterValue(function, currentValue);
        }
    }
}

public enum DMXInputResolution
{
    res_8its,
    res_16bits,
    res_24bits,
    res_32bits
}
// Theres should be a Component for every FixtureFunction to be used by actual fixture modules
public enum FixtureFunction
{
    func_Dimmer,
    func_Intensity,
    func_Color_1, // R
    func_Color_2, // G
    func_Color_3, // B
    func_Color_4, // Found in some devices, commonly white or amber
    func_Angle,
    func_Shutter,
    func_Rotation,
    func_Translation
}
