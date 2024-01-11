using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIEffect_Demo_ColorControl : MonoBehaviour
{
    [SerializeField] private Color m_Color;
    [SerializeField] private ColorEvent m_ColorEvent = new ColorEvent();

    [System.Serializable]
    public class ColorEvent : UnityEvent<Color>
    {
    }

    private void Start()
    {
        var sliders = GetComponentsInChildren<Slider>();
        for (var i = 0; i < sliders.Length; i++)
        {
            var channel = i;
            if (channel == 0)
                sliders[channel].value = m_Color.r;
            else if (channel == 1)
                sliders[channel].value = m_Color.g;
            else if (channel == 2)
                sliders[channel].value = m_Color.b;
            else
                sliders[channel].value = m_Color.a;
            sliders[i].onValueChanged.AddListener(value => ChangeColor(channel, value));
        }
    }

    private void ChangeColor(int channel, float value)
    {
        var old = m_Color;
        if (channel == 0)
            m_Color.r = value;
        else if (channel == 1)
            m_Color.g = value;
        else if (channel == 2)
            m_Color.b = value;
        else
            m_Color.a = value;

        if (old != m_Color)
            m_ColorEvent.Invoke(m_Color);
    }
}
