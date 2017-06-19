using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESVerticalSlider : UESDisplayObject
{
    protected float m_Value;
    protected float m_TopValue;
    protected float m_BottomValue;
    protected int drawingStyle;
    protected GUIStyle m_Slider;
    protected GUIStyle m_Thumb;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle(0, 100);
        Value = 30;
        Rect = new Rect(Vector2.zero, new Vector2(INIT_WIDTH, INIT_HEIGHT*2));
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Value = GUI.VerticalSlider(m_DrawingRect, m_Value, m_TopValue, m_BottomValue);
                break;

            case 2:
                m_Value = GUI.VerticalSlider(m_DrawingRect, m_Value, m_TopValue, m_BottomValue,m_Slider,m_Thumb);
                break;
        }
    }

    public void SetDrawingStyle(float topValue,float bottomValue)
    {
        drawingStyle = 1;

        m_TopValue = topValue;
        m_BottomValue = bottomValue;
    }

    public void SetDrawingStyle(float topValue, float bottomValue,GUIStyle slider,GUIStyle thumb)
    {
        drawingStyle = 2;

        m_TopValue = topValue;
        m_BottomValue = bottomValue;
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
