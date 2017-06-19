using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESIntPopup : UESDisplayObject
{
    protected int m_SelectedValue;
    protected int[] m_OptionValues;
    protected GUIContent[] m_DisplayedOptions;
    protected string[] m_StrDisplayedOptions;
    protected int drawingStyle;
    protected SerializedProperty m_Property;
    protected GUIContent m_Label;
    protected GUIStyle m_Style;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        var displayOptions = new string[]
        {
            "1",
            "2",
            "3"
        };
        var optionValues = new int[]
        {
            1,
            2,
            3
        };
        SetDrawingStyle("IntPopup", displayOptions, optionValues);
        SelectedValue = 1;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_SelectedValue = EditorGUI.IntPopup(m_DrawingRect, m_SelectedValue, m_DisplayedOptions, m_OptionValues);
                break;

            case 2:
                m_SelectedValue = EditorGUI.IntPopup(m_DrawingRect, m_SelectedValue, m_StrDisplayedOptions, m_OptionValues);
                break;

            case 3:
                EditorGUI.IntPopup(m_DrawingRect, m_Property, m_DisplayedOptions, m_OptionValues);
                break;

            case 4:
                m_SelectedValue = EditorGUI.IntPopup(m_DrawingRect, m_Label, m_SelectedValue, m_DisplayedOptions, m_OptionValues);
                break;

            case 5:
                m_SelectedValue = EditorGUI.IntPopup(m_DrawingRect, m_SelectedValue, m_DisplayedOptions, m_OptionValues,m_Style);
                break;

            case 6:
                m_SelectedValue = EditorGUI.IntPopup(m_DrawingRect, m_SelectedValue, m_StrDisplayedOptions, m_OptionValues, m_Style);
                break;

            case 7:
                EditorGUI.IntPopup(m_DrawingRect, m_Property, m_DisplayedOptions, m_OptionValues,m_Label);
                break;

            case 8:
                m_SelectedValue = EditorGUI.IntPopup(m_DrawingRect, m_LabelStr,m_SelectedValue, m_StrDisplayedOptions, m_OptionValues);
                break;

            case 9:
                m_SelectedValue = EditorGUI.IntPopup(m_DrawingRect, m_Label, m_SelectedValue, m_DisplayedOptions, m_OptionValues,m_Style);
                break;

            case 10:
                m_SelectedValue = EditorGUI.IntPopup(m_DrawingRect, m_LabelStr, m_SelectedValue, m_StrDisplayedOptions, m_OptionValues,m_Style);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent[] displayedOptions,int[] optionValues)
    {
        drawingStyle = 1;

        m_DisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
    }

    public void SetDrawingStyle(string[] displayedOptions, int[] optionValues)
    {
        drawingStyle = 2;

        m_StrDisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
    }

    public void SetDrawingStyle(SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues)
    {
        drawingStyle = 3;

        m_Property = property;
        m_DisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
    }

    public void SetDrawingStyle(GUIContent label,GUIContent[] displayedOptions, int[] optionValues)
    {
        drawingStyle = 4;

        m_Label = label;
        m_DisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
    }

    public void SetDrawingStyle(GUIContent[] displayedOptions, int[] optionValues,GUIStyle style)
    {
        drawingStyle = 5;

        m_DisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
        m_Style = style;
    }

    public void SetDrawingStyle(string[] displayedOptions, int[] optionValues,GUIStyle style)
    {
        drawingStyle = 6;

        m_StrDisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
        m_Style = style;
    }

    public void SetDrawingStyle(SerializedProperty property,GUIContent[] displayedOptions, int[] optionValues, GUIContent label)
    {
        drawingStyle = 7;

        m_Property = property;
        m_DisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
        m_Label = label;
    }

    public void SetDrawingStyle(string label,string[] displayedOptions, int[] optionValues)
    {
        drawingStyle = 8;

        m_StrDisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
        m_LabelStr = label;
    }

    public void SetDrawingStyle(GUIContent label, GUIContent[] displayedOptions, int[] optionValues,GUIStyle style)
    {
        drawingStyle = 9;

        m_Label = label;
        m_DisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
        m_Style = style;
    }

    public void SetDrawingStyle(string label, string[] displayedOptions, int[] optionValues,GUIStyle style)
    {
        drawingStyle = 10;

        m_StrDisplayedOptions = displayedOptions;
        m_OptionValues = optionValues;
        m_LabelStr = label;
        m_Style = style;
    }

    public int SelectedValue
    {
        get
        {
            return m_SelectedValue;
        }
        set
        {
            m_SelectedValue = value;
        }
    }
}
