using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UI;

public class UESColorField : UESDisplayObject
{
    protected Color m_Color;
    protected int drawingStyle;
    protected bool m_ShowEyedropper;
    protected bool m_ShowAlpha;
    protected bool m_Hdr;
    protected ColorPickerHDRConfig m_ColorPickerHDRConfig;
    protected GUIContent m_Label;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle(new GUIContent("ColorField"),true,true,false,null);
        Color = Color.blue;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Color = EditorGUI.ColorField(m_DrawingRect, m_Color);
                break;

            case 2:
                m_Color = EditorGUI.ColorField(m_DrawingRect, m_Label, m_Color);
                break;

            case 3:
                m_Color = EditorGUI.ColorField(m_DrawingRect, m_LabelStr, m_Color);
                break;

            case 4:
                m_Color = EditorGUI.ColorField(m_DrawingRect, m_Label, m_Color, m_ShowEyedropper, m_ShowAlpha, m_Hdr, m_ColorPickerHDRConfig);
                break;
        }
    }

    public void SetDrawingStyle()
    {
        drawingStyle = 1;
    }

    public void SetDrawingStyle(GUIContent label)
    {
        drawingStyle = 2;

        label = m_Label;
    }

    public void SetDrawingStyle(string label)
    {
        drawingStyle = 3;

        m_LabelStr = label;
    }

    public void SetDrawingStyle(GUIContent label, bool showEyedropper, bool showAlpha,bool hdr, ColorPickerHDRConfig colorPickerHDRConfig)
    {
        drawingStyle = 4;

        m_Label = label;
        m_ShowEyedropper = showEyedropper;
        m_ShowAlpha = showAlpha;
        m_Hdr = hdr;
        m_ColorPickerHDRConfig = colorPickerHDRConfig;
    }

    public Color Color
    {
        get
        {
            return m_Color;
        }
        set
        {
            m_Color = value;
        }
    }
}