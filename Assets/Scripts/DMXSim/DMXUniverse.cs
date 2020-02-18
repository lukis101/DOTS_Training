using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DMXUniverse : MonoBehaviour
{
    public int universeID;
    public FixtureController[] devices;
    public byte[] addresses;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Update(DMXInputManager input)
    {
        for (int i = 0; i < devices.Length; i++)
        {
            byte addr = addresses[i];
            devices[i].UpdateInputs(input, universeID, addr);
        }
    }
}
