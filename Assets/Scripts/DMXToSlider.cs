using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class DMXToSlider : MonoBehaviour
{
    protected Slider slider;
    protected DMXInputManager inputManager;
    public int universe;
    public int address;
    protected float lastValue;

    void Start()
    {
        slider = GetComponent<Slider>();
        inputManager = FindObjectOfType<DMXInputManager>();
    }

    void Update()
    {
        float curValue = inputManager.GetInput(universe, address) / 255.0f;
        if (lastValue != curValue)
        {
            lastValue = curValue;
            slider.value = curValue;
        }
        else if (slider.value != curValue)
        {
            lastValue = slider.value;
            inputManager.SetValue(universe, address, lastValue);
        }
    }
}
