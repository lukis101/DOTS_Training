using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public enum RotateAxis
{
    Pan,
    Tilt,
    Roll
}
public struct AxisRotator : IComponentData
{
    public float AngleMin;
    public float AngleMax;
    public float Velocity;
    public float TargetAngle;
    public float CurrentAngle;
}
public struct FixtureRotatorComponent : IComponentData
{
    public RotateAxis Axis;
}

public class FixtureRotator : FixtureComponent, IConvertGameObjectToEntity
{
    public RotateAxis m_axis;
    public float m_angleMin = -180; // deg
    public float m_angleMax = 180;  // deg
    public float m_velocity = 10; // deg/s
    protected float m_angleTarget = 0;
    protected float m_angleCurrent = 0;

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

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var axisdata = new AxisRotator
        {
            AngleMin = m_angleMin,
            AngleMax = m_angleMax,
            Velocity = m_velocity,
            TargetAngle = m_angleTarget,
            CurrentAngle = m_angleCurrent
        };
        var rotdata = new FixtureRotatorComponent
        {
            Axis = m_axis
        };

        dstManager.AddComponentData(entity, axisdata);
        dstManager.AddComponentData(entity, rotdata);
    }
}

[UpdateAfter(typeof(DMXInputManagerSystem))]
public class AxisRotatorSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dtime = Time.DeltaTime;
        var outputDeps = Entities.WithoutBurst().ForEach((ref AxisRotator axis, in DMXParameterComponent param) => {
            float curangle = axis.CurrentAngle;
            //float speed = 40f;
            float angdiff = axis.TargetAngle - curangle;
            axis.CurrentAngle = axis.TargetAngle;// curangle + angdiff * (dtime * speed);
            float target = Mathf.Lerp(axis.AngleMin, axis.AngleMax, param.CurValue);

            axis.TargetAngle = target;
            axis.CurrentAngle = target;
        }).Schedule(inputDeps);

        return outputDeps;
    }
}

[UpdateAfter(typeof(AxisRotatorSystem))]
public class FixturePanSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dtime = Time.DeltaTime;
        var outputDeps = Entities.WithoutBurst().ForEach((ref Rotation rot, ref FixtureRotatorComponent rotcomp, in AxisRotator axis) => {
            float curangle = axis.CurrentAngle;
            //curangle += dtime * 40f;
            curangle *= Mathf.Deg2Rad;
            quaternion q = quaternion.identity;// = Quaternion.Euler(rot.Value.value.x, curangle, rot.Value.value.y);
            switch (rotcomp.Axis)
            {
                case RotateAxis.Pan:
                    q = quaternion.RotateY(curangle);
                    break;
                case RotateAxis.Tilt:
                    q = quaternion.RotateX(curangle);
                    break;
                case RotateAxis.Roll:
                    q = quaternion.RotateZ(curangle);
                    break;
                default:
                    break;
            }
            rot = new Rotation { Value = q };
        }).Schedule(inputDeps);

        return outputDeps;
    }
}

