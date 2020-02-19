using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class DMXFixtureAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int address;
    public int universe;
    //protected FixtureComponent[] components;
    public DMXParameter[] parameters;

    // Start is called before the first frame update
    void Start()
    {
        parameters = GetComponentsInChildren<DMXParameter>(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var fixture = new DMXParameterComponent
        {
            Address = parameters.Length,
            Universe = 0

        };

        dstManager.AddComponentData(entity, fixture);
    }
}
