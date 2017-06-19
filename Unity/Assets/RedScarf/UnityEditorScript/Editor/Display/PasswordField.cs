using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESPasswordField : UESDisplayObject {

    protected string m_Password;
    protected int drawingStyle;
    protected GUIStyle m_Style;
    protected GUIContent m_Label;
    protected string m_LabelStr;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("PasswordField");
        Password = "123456";
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Password = EditorGUI.PasswordField(m_DrawingRect,m_Password);
                break;

            case 2:
                m_Password = EditorGUI.PasswordField(m_DrawingRect, m_Label, m_Password);
                break;

            case 3:
                m_Password = EditorGUI.PasswordField(m_DrawingRect,m_Password,m_Style);
                break;

            case 4:
                m_Password = EditorGUI.PasswordField(m_DrawingRect,m_LabelStr ,m_Password);
                break;

            case 5:
                m_Password = EditorGUI.PasswordField(m_DrawingRect, m_Label, m_Password,m_Style);
                break;

            case 6:
                m_Password = EditorGUI.PasswordField(m_DrawingRect, m_LabelStr, m_Password,m_Style);
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

        m_Label = label;
    }

    public void SetDrawingStyle(GUIStyle style)
    {
        drawingStyle = 3;

        m_Style = style;
    }

    public void SetDrawingStyle(string label)
    {
        drawingStyle = 4;

        m_LabelStr = label;
    }

    public void SetDrawingStyle(GUIContent label,GUIStyle style)
    {
        drawingStyle = 5;

        m_Label = label;
        m_Style = style;
    }

    public void SetDrawingStyle(string label, GUIStyle style)
    {
        drawingStyle = 6;

        m_LabelStr = label;
        m_Style = style;
    }

    public string Password
    {
        get
        {
            return m_Password;
        }
        set
        {
            m_Password = value;
        }
    }
}
