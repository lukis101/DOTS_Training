using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

public class DMXInputManager : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        var fixtures = FindObjectsOfType<FixtureController>();
        foreach (var fixture in fixtures)
        {
            fixture.UpdateInputs(this);
        }
    }

    public byte GetInput(int universe, int addr)
    {
        if (universe < 0 || universe >= (DMXInputSingleton.UNIVERSES_MAX - 1))
            throw new ArgumentException("Invalid universe");
        if (addr < 0 || addr >= 256)
            throw new ArgumentException("Invalid address");
        return DMXInputSingleton.instance.values[universe * 256 + addr];
    }
    public void SetValue(int universe, int addr, byte value)
    {
        DMXInputSingleton.instance.values[universe * 256 + addr] = value;
    }
    public void SetValue(int universe, int addr, float value)
    {
        DMXInputSingleton.instance.values[universe * 256 + addr] = (byte)(Mathf.Clamp01(value)*255);
    }

    // For use within job systems
    public static float GetCurrentValueMapped(in int universe, in int address, in int minvalue, in int maxvalue)
    {
        // TODO: multi-channel values via DMXInputResolution
        int rawvalue = DMXInputSingleton.instance.values[universe * 256 + address];
        return (rawvalue - minvalue) / (float)(maxvalue - minvalue);
    }
}
