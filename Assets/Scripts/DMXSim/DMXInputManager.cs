using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

public class DMXInputManager : MonoBehaviour
{
    const int UNIVERSES_MAX = 4;
    protected byte[] values;

    void Start()
    {
        values = new byte[UNIVERSES_MAX * 256];
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

public class DMXInputManagerSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dtime = Time.DeltaTime;
        var abstime = UnityEngine.Time.timeSinceLevelLoad;
        var outputDeps = Entities.WithoutBurst().ForEach((ref DMXParameterComponent param) => {
            param.CurValue = (float)(Math.Sin(abstime * 1)*0.5+0.5);
            //param = new DMXParameterComponent { Value = q };
        }).Schedule(inputDeps);

        return outputDeps;
    }
}