using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public abstract class FixtureComponent : MonoBehaviour
{
    //public FixtureController controller;
    public abstract void SetParameterValue(FixtureFunction function, float value);
    public abstract void AddInputComponent(DMXParameter param, int universe, int address, EntityManager dstManager, Entity entity);
}
