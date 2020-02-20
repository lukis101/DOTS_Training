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

public struct FixtureLightComponent : IComponentData
{
    public LightType Shape;
    public Color Color;
    public float Intensity;
    public float Angle;
    public float AngleMin;
    public float AngleMax;
}
public struct FixtureLightIntensity : IComponentData
{
    public float Intensity;
}
public struct FixtureLightAngle : IComponentData
{
    public float Angle;
}

[RequireComponent(typeof(Light))]
public class FixtureLight : FixtureComponent, IConvertGameObjectToEntity
{
    public Light m_light;
    public GameObject m_beam;
    public LightType m_type;
    public Color m_FilterColor;
    public Color m_SourceColor;
    public Color m_color; // filtered output color
    // Max intensity in Lux at 5m
    public float m_intensity;
    public float m_beamAngleMin;
    public float m_beamAngleMax;
    public float m_falloff;
    public float m_beamAngleCurrent;

    private Material beamMat;

    void Start()
    {
        m_beamAngleCurrent = m_beamAngleMax;
        beamMat = m_beam.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        Color filteredColor = m_color * m_FilterColor;
        filteredColor.a = 0.2f;
        m_light.color = filteredColor;
        beamMat.color = filteredColor;
        m_light.spotAngle = m_beamAngleCurrent;
        float length = m_light.range;
        float r = length * Mathf.Tan(m_beamAngleCurrent* Mathf.Deg2Rad);
        m_beam.transform.localScale = new Vector3(r, length, r);
    }
    public override void SetValueTest(float test)
    {
        m_beamAngleCurrent = Mathf.Lerp(m_beamAngleMin, m_beamAngleMax, test);
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var hybridlight = new GameObject("Light_"+name);
        var hyblightcomp  = hybridlight.AddComponent<Light>();
        hyblightcomp.type = m_type;
        hyblightcomp.spotAngle = m_light.spotAngle;

        var bridgecomp = hybridlight.AddComponent<FixtureLightHybrid>();
        bridgecomp.ent = entity;

        var lightcomp = new FixtureLightComponent
        {
            Shape = m_type,
            Intensity = m_intensity
        };

        dstManager.AddComponentData(entity, lightcomp);
    }
}

[UpdateAfter(typeof(DMXInputManagerSystem))]
public class FixtureLightSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job1 = Entities.WithoutBurst().ForEach((ref FixtureLightComponent light, ref FixtureLightIntensity intens, in DMXParameterComponent param) => {
            light.Intensity = intens.Intensity;
        }).Schedule(inputDeps);

        var job2 = Entities.WithoutBurst().ForEach((ref FixtureLightComponent light, ref FixtureLightAngle ang, in DMXParameterComponent param) => {
            light.Angle = ang.Angle;
        }).Schedule(inputDeps);

        return JobHandle.CombineDependencies(job1, job2);
    }
}
