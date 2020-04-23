using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public enum LightSourceType
{
    Other,
    LED,
    Incadescent,
    Discharge,
    Fluorescent,
    Laser,
    Plasma,
    Gas // neon?
}
public enum LightFalloffType
{
    RoundProfile,
    RoundSharp,
    RoundPC,
    RoundFresnel,
    RoundPAR,
    SquareSymmetric,
    SquareAsymmetric
}

public struct FixtureFunction_Intensity : IComponentData
{
    public int Address;
    public int Universe;
    public int MinValue;
    public int MaxValue;

    public float Intensity;
}
public struct FixtureFunction_Angle : IComponentData
{
    public int Address;
    public int Universe;
    public int MinValue;
    public int MaxValue;

    public float TargetAngle;
    public float CurrentAngle;
}

public struct FixtureLightComponent : IComponentData
{
    public LightType Shape;
    public Color Color;
    public float Intensity;
    public float Angle;
    public float AngleMin;
    public float AngleMax;
}

[RequireComponent(typeof(Light))]
public class FixtureLight : FixtureComponent, IConvertGameObjectToEntity
{
    public Light m_light;
    public LightType m_type;
    public Color m_FilterColor;
    public Color m_SourceColor;
    public Color m_color; // filtered output color
    // Max intensity in Lux at 5m
    // 100w LED spot at 17deg angle and 5m is around 3150 lux
    public float m_intensity;
    public float m_intensityCurrent;
    public float m_beamAngleMin;
    public float m_beamAngleMax;
    public float m_falloff;
    public float m_beamAngleCurrent;

    private Material beamMat;

    void Start()
    {
        m_beamAngleCurrent = m_beamAngleMax;
        beamMat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        m_light.intensity = m_intensityCurrent * (m_intensity / 3150 * 2);
        beamMat.SetFloat("_Intensity", 4 * m_intensityCurrent);

        Color filteredColor = m_color * m_FilterColor;
        filteredColor.a = 0.2f;
        m_light.color = filteredColor;
        beamMat.color = filteredColor;
        m_light.spotAngle = m_beamAngleCurrent;
        float length = 10;// m_light.range;
        float r = GetBeamScale(length, m_beamAngleCurrent);
        transform.localScale = new Vector3(r, r, length);
    }
    public override void SetParameterValue(FixtureFunction function, float value)
    {
        if (function == FixtureFunction.func_Angle)
            m_beamAngleCurrent = Mathf.Lerp(m_beamAngleMin, m_beamAngleMax, value);
        if (function == FixtureFunction.func_Intensity)
            m_intensityCurrent = value;
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        /*var hybridlight = new GameObject("Light_"+name);
        var hyblightcomp  = hybridlight.AddComponent<Light>();
        hyblightcomp.type = m_type;
        hyblightcomp.spotAngle = m_light.spotAngle;*/

        //var bridgecomp = hybridlight.AddComponent<FixtureLightHybrid>();
        //bridgecomp.ent = entity;

        var lightcomp = new FixtureLightComponent
        {
            Shape = m_type,
            Color = m_color,
            Intensity = m_intensity,
            Angle = m_beamAngleMin,
            AngleMin = m_beamAngleMin,
            AngleMax = m_beamAngleMax
        };

        dstManager.AddComponentData(entity, lightcomp);
    }

    public override void AddInputComponent(DMXParameter param, int universe, int address, EntityManager dstManager, Entity entity)
    {
        switch (param.function)
        {
            case FixtureFunction.func_Intensity:
                var comp_intensity = new FixtureFunction_Intensity
                {
                    Address = address + param.address,
                    Universe = universe,
                    MinValue = param.minValue,
                    MaxValue = param.maxValue,
                };
                dstManager.AddComponentData(entity, comp_intensity);
                break;
            case FixtureFunction.func_Color_1:
                break;
            case FixtureFunction.func_Color_2:
                break;
            case FixtureFunction.func_Color_3:
                break;
            case FixtureFunction.func_Color_4:
                break;
            case FixtureFunction.func_Angle:
                var comp_angle = new FixtureFunction_Angle
                {
                    Address = address + param.address,
                    Universe = universe,
                    MinValue = param.minValue,
                    MaxValue = param.maxValue,
                };
                dstManager.AddComponentData(entity, comp_angle);
                break;
            case FixtureFunction.func_Shutter:
                break;
            default:
                break;
        }
    }

    public static float GetBeamScale(float length, float angle)
    {
        return length * Mathf.Tan(angle * Mathf.Deg2Rad);
    }
}

[UpdateAfter(typeof(DMXInputManagerSystem))]
public class FixtureLightIntesitySystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job1 = Entities.WithoutBurst().ForEach((ref FixtureLightComponent light, ref FixtureFunction_Intensity input) => {
            float invalue = DMXInputManager.GetCurrentValueMapped(input.Universe, input.Address, input.MinValue, input.MaxValue);
            input.Intensity = invalue;

            light.Intensity = invalue;
        }).Schedule(inputDeps);

        return job1;
        //return JobHandle.CombineDependencies(job1, job2); // TODO find how to write to same comp in parallel
    }
}
[UpdateAfter(typeof(FixtureLightIntesitySystem))]
public class FixtureLightAngleSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job2 = Entities.WithoutBurst().ForEach((ref FixtureLightComponent light, ref FixtureFunction_Angle input) => {
            float invalue = DMXInputManager.GetCurrentValueMapped(input.Universe, input.Address, input.MinValue, input.MaxValue);
            input.TargetAngle = invalue;
            // TODO: simulate mechanical inertia
            input.CurrentAngle = input.TargetAngle;

            light.Angle = Mathf.Lerp(light.AngleMin, light.AngleMax, input.CurrentAngle);
        }).Schedule(inputDeps);

        return job2;
    }
}

[UpdateAfter(typeof(FixtureLightAngleSystem))]
public class FixtureLightSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var outputDeps = Entities.ForEach((ref FixtureLightComponent light, ref NonUniformScale scale) => {
            // TODO: proper light range(length)
            float length = 10;
            float r = FixtureLight.GetBeamScale(length, light.Angle);
            scale = new NonUniformScale { Value = new Vector3(r, r, length) };
        }).Schedule(inputDeps);

        return outputDeps;
    }
}
