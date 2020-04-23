using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class FixtureController : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public int address;
    public int universe;
    protected FixtureComponent[] components;
    protected DMXParameter[] parameters;

    void Start()
    {
        components = GetComponentsInChildren<FixtureComponent>(false);
        parameters = GetComponentsInChildren<DMXParameter>();
    }

    void LateUpdate()
    {
        if (components == null) return;
        foreach (var component in components)
        {

        }
    }
    public void UpdateInputs(DMXInputManager input)
    {
        int offset = address;
        foreach (var parameter in parameters)
        {
            int value = (int)input.GetInput(universe, parameter.address + offset);
            if (parameter.resolution >= DMXInputResolution.res_16bits)
                value = value<<8 + (int)input.GetInput(universe, parameter.address + offset + 1);
            if (parameter.resolution >= DMXInputResolution.res_24bits)
                value = value << 8 + (int)input.GetInput(universe, parameter.address + offset + 2);
            if (parameter.resolution >= DMXInputResolution.res_32bits)
                value = value << 8 + (int)input.GetInput(universe, parameter.address + offset + 3);
            parameter.UpdateInput(value);
        }
    }


    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        parameters = GetComponentsInChildren<DMXParameter>(false);
        foreach (var param in parameters)
            foreach (var fixturecomponent in param.affectedComponents)
                referencedPrefabs.Add(fixturecomponent.gameObject);
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        parameters = GetComponentsInChildren<DMXParameter>(false);
        //var rotors = GetComponentsInChildren<DMXFixtureRotatorComponent>(false);
        //var rotor = rotors[0];
        //var param = parameters[0];

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        //var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);
        //var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(param.gameObject, settings);

        //var eent = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObject, settings);
        //var instance = entityManager.Instantiate(prefab);
        //var instance = entityManager.CreateArchetype(Translate);
        //var aarchetype = entityManager.CreateArchetype(typeof(Rotation), typeof(Translation), typeof(DMXParameterComponent));
        //var prefab = entityManager.CreateEntity(aarchetype);

        foreach (var param in parameters)
        {
            foreach (var fixturecomponent in param.affectedComponents)
            {
                Entity fixturecomponentent = conversionSystem.GetPrimaryEntity(fixturecomponent.gameObject);

                fixturecomponent.AddInputComponent(param, universe, address, dstManager, fixturecomponentent);
            }
        }

        //dstManager.AddComponentData(entity, fixture);
    }
}
