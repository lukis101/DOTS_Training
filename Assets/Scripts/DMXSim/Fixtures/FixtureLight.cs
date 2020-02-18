using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FixtureLight : FixtureComponent
{
    public Light m_light;
    public GameObject m_beam;
    public LightType m_type;
    public Color m_color;
    public Color m_FilterColor;
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
}

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