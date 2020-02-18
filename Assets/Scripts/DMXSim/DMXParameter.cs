using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            component.SetValueTest(currentValue);
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
public enum FixtureFunction
{
    func_Dimmer,
    func_Intensity,
    func_Color_R,
    func_Color_G,
    func_Color_B,
    func_Rotation,
    func_Translation
}