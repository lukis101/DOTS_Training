using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DMXInputManager : MonoBehaviour
{
    const int UNIVERSES_MAX = 4;
    protected byte[] values;
    public DMXUniverse[] universes;

    void Start()
    {
        values = new byte[UNIVERSES_MAX * 256];
    }

    void Update()
    {
        universes = FindObjectsOfType<DMXUniverse>();
        foreach (var universe in universes)
        {
            universe.Update(this);
        }
    }

    public byte GetInput(int universe, int addr)
    {
        if (universe < 0 || universe >= (UNIVERSES_MAX - 1))
            throw new ArgumentException("Invalid universe");
        if (addr < 0 || addr >= 256)
            throw new ArgumentException("Invalid address");
        return values[universe * 256 + addr];
    }
    public void SetValue(int universe, int addr, byte value)
    {
        values[universe * 256 + addr] = value;
    }
    public void SetValue(int universe, int addr, float value)
    {
        values[universe * 256 + addr] = (byte)(Mathf.Clamp01(value)*255);
    }
}
