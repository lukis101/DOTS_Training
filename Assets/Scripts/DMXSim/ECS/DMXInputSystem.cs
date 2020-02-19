using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class DMXInputSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dtime = Time.DeltaTime;
        var outputDeps = Entities.WithoutBurst().ForEach((ref Rotation rot, ref DMXFixtureRotatorComponent comp) => {
            float curangle = comp.CurrentAngle;
            curangle += dtime * 40f;
            quaternion rot2 = Quaternion.Euler(rot.Value.value.x, curangle, rot.Value.value.y);
            rot = new Rotation { Value = rot2 };
            comp.CurrentAngle = curangle;
        }).Schedule(inputDeps);

        return outputDeps;
    }
}
