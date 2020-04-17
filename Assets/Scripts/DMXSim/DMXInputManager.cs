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
}

public class DMXInputManagerSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dtime = Time.DeltaTime;
        var abstime = UnityEngine.Time.timeSinceLevelLoad;
        var outputDeps = Entities.WithoutBurst().ForEach((ref DMXParameterComponent param) => {
            param.CurValue = DMXInputSingleton.instance.values[param.Universe * 256 + param.Address] / 255.0f;
        }).Schedule(inputDeps);

        return outputDeps;
    }
}
