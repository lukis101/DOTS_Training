using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixtureRotator : FixtureComponent
{
    public RotateAxis m_axis;
    public float m_angleMin = -180;
    public float m_angleMax = 180;
    public float m_angleTarget = 0;
    public float m_angleCurrent = 0;
    
    void Start()
    {
    }

    void Update()
    {
        Vector3 axis;
        switch (m_axis)
        {
            default:
            case RotateAxis.Pan:
                axis = Vector3.up;
                break;
            case RotateAxis.Tilt:
                axis = Vector3.left;
                break;
            case RotateAxis.Roll:
                axis = Vector3.forward;
                break;
        }
        m_angleCurrent = m_angleTarget; // TODO: constrain, slow interpolate
        transform.localRotation = Quaternion.AngleAxis(m_angleCurrent, axis);
    }

    public override void SetValueTest(float test)
    {
        m_angleTarget = Mathf.Lerp(m_angleMin, m_angleMax, test);
    }
}

public enum RotateAxis
{
    Pan,
    Tilt,
    Roll
}
