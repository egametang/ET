using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESHorizontalSlider : UESDisplayObject
{
    protected float m_Value;
    protected float m_LeftValue;
    protected float m_RightValue;
    protected int drawingStyle;
    protected GUIStyle m_Slider;
    protected GUIStyle m_Thumb;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle(0, 100);
        Value = 30;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Value = GUI.HorizontalSlider(m_DrawingRect, m_Value, m_LeftValue, m_RightValue);
                break;

            case 2:
                m_Value = GUI.HorizontalSlider(m_DrawingRect, m_Value, m_LeftValue, m_RightValue, m_Slider,m_Thumb);
                break;
        }
    }

    public void SetDrawingStyle(float leftValue,float rightValue)
    {
        drawingStyle = 1;

        m_LeftValue = leftValue;
        m_RightValue = rightValue;
    }

    public void SetDrawingStyle(float leftValue, float rightValue,GUIStyle slider,GUIStyle thumb)
    {
        drawingStyle = 2;

        m_LeftValue = leftValue;
        m_RightValue = rightValue;
        m_Slider = slider;
        m_Thumb = thumb;
    }

    public float Value
    {
        get
        {
            return m_Value;
        }
        set
        {
            m_Value = value;
        }
    }
}
