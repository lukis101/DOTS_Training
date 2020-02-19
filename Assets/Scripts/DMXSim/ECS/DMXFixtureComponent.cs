
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct DMXFixtureComponent : IComponentData
{
    public int address;
    public int universe;
    public Entity parts;
}
