using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESIntSlider : UESDisplayObject
{
    protected int m_Value;
    protected int m_LeftValue;
    protected int m_RightValue;
    protected int drawingStyle;
    protected SerializedProperty m_Property;
    protected GUIContent m_Label;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("IntSlider", 0, 100);
        Value = 30;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Value = EditorGUI.IntSlider(m_DrawingRect, m_Value, m_LeftValue, m_RightValue);
                break;

            case 2:
                EditorGUI.IntSlider(m_DrawingRect, m_Property, m_LeftValue, m_RightValue);
                break;

            case 3:
                m_Value = EditorGUI.IntSlider(m_DrawingRect,m_Label, m_Value, m_LeftValue, m_RightValue);
                break;

            case 4:
                EditorGUI.IntSlider(m_DrawingRect,m_Property, m_LeftValue, m_RightValue,m_Label);
                break;

            case 5:
                EditorGUI.IntSlider(m_DrawingRect, m_Property, m_LeftValue, m_RightValue, m_LabelStr);
                break;

            case 6:
                m_Value = EditorGUI.IntSlider(m_DrawingRect,m_LabelStr, m_Value, m_LeftValue, m_RightValue);
                break;
        }
    }

    public void SetDrawingStyle(int leftValue,int rightValue)
    {
        drawingStyle = 1;

        m_LeftValue = leftValue;
        m_RightValue = rightValue;
    }

    public void SetDrawingStyle(SerializedProperty property,int leftValue, int rightValue)
    {
        drawingStyle = 2;

        m_LeftValue = leftValue;
        m_RightValue = rightValue;
        m_Property = property;
    }

    public void SetDrawingStyle(GUIContent label, int leftValue, int rightValue)
    {
        drawingStyle = 3;

        m_LeftValue = leftValue;
        m_RightValue = rightValue;
        m_Label = label;
    }

    public void SetDrawingStyle(SerializedProperty property, int leftValue, int rightValue,GUIContent label)
    {
        drawingStyle = 4;

        m_LeftValue = leftValue;
        m_RightValue = rightValue;
        m_Property = property;
        m_Label = label;
    }

    public void SetDrawingStyle(SerializedProperty property, int leftValue, int rightValue, string label)
    {
        drawingStyle = 5;

        m_LeftValue = leftValue;
        m_RightValue = rightValue;
        m_Property = property;
        m_LabelStr = label;
    }

    public void SetDrawingStyle(string label, int leftValue, int rightValue)
    {
        drawingStyle = 6;

        m_LeftValue = leftValue;
        m_RightValue = rightValue;
        m_LabelStr = label;
    }

    public int Value
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
