using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DMXInputAnimator : MonoBehaviour
{
    protected DMXInputManager inputManager;

    void Start()
    {
        inputManager = FindObjectOfType<DMXInputManager>();
    }

    void Update()
    {
        for (int i = 0; i < 256; i++)
        {
            float ang = Time.time*0.3f + (float)i /4.0f;
            byte value = (byte)(((Mathf.Sin(ang) * 0.5f) + 0.5f) * 255.0f);
            inputManager.SetValue(0, i, value);
        }
    }
}
