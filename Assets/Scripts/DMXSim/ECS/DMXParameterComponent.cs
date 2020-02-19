
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

//[GenerateAuthoringComponent]
public struct DMXParameterComponent : IComponentData
{
    public int Address;
    public int Universe;
    public int MinValue;
    public int MaxValue;
    public float CurValue;
}