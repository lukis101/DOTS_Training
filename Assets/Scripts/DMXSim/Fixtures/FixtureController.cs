using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixtureController : MonoBehaviour
{
    protected FixtureComponent[] components;
    protected DMXParameter[] parameters;

    void Start()
    {
        components = GetComponentsInChildren<FixtureComponent>(false);
        parameters = GetComponents<DMXParameter>();
    }

    void LateUpdate()
    {
        if (components == null) return;
        foreach (var component in components)
        {

        }
    }
    public void UpdateInputs(DMXInputManager input, int universe, int offset)
    {
        foreach (var parameter in parameters)
        {
            int value = (int)input.GetInput(universe, parameter.address + offset);
            if (parameter.resolution >= DMXInputResolution.res_16bits)
                value = value<<8 + (int)input.GetInput(universe, parameter.address + offset + 1);
            if (parameter.resolution >= DMXInputResolution.res_24bits)
                value = value << 8 + (int)input.GetInput(universe, parameter.address + offset + 2);
            if (parameter.resolution >= DMXInputResolution.res_32bits)
                value = value << 8 + (int)input.GetInput(universe, parameter.address + offset + 3);
            parameter.UpdateInput(value);
        }
    }
}
