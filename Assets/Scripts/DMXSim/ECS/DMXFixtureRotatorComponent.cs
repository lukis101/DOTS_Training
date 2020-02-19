using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct DMXFixtureRotatorComponent : IComponentData
{
    public RotateAxis MovementAxis;
    public float AngleMin;
    public float AngleMax;
    public float TargetAngle;
    public float CurrentAngle;
    public float CurrentVelocity;
}
