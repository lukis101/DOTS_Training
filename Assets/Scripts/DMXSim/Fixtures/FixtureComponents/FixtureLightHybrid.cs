using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class FixtureLightHybrid : MonoBehaviour
{
    public Entity ent;
    protected Light m_light;
    protected EntityManager m_emanager;

    void Start()
    {
        m_emanager = World.DefaultGameObjectInjectionWorld.EntityManager;
        m_light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        var localtoworld = m_emanager.GetComponentData<LocalToWorld>(ent);
        var lightstate = m_emanager.GetComponentData<FixtureLightComponent>(ent);

        //transform.rotation = localtoworld.Rotation; // does not work
        transform.forward = localtoworld.Forward; // works instead
        transform.position = localtoworld.Position;

        m_light.intensity = lightstate.Intensity;
        m_light.spotAngle = lightstate.Intensity;
    }
}
