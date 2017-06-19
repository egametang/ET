using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESFoldout : UESDisplayObject {

    protected bool m_Foldout;
    protected bool m_ToggleOnLabelClick;
    protected int drawingStyle;
    protected GUIContent m_Content;
    protected string m_ContentStr;
    protected GUIStyle m_Style;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("Foldout",true);
        Foldout = true;
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Foldout = EditorGUI.Foldout(m_DrawingRect, m_Foldout, m_Content);
                break;

            case 2:
                m_Foldout = EditorGUI.Foldout(m_DrawingRect, m_Foldout, m_ContentStr);
                break;

            case 3:
                m_Foldout = EditorGUI.Foldout(m_DrawingRect, m_Foldout, m_Content, m_ToggleOnLabelClick);
                break;

            case 4:
                m_Foldout = EditorGUI.Foldout(m_DrawingRect, m_Foldout, m_Content, m_Style);
                break;

            case 5:
                m_Foldout = EditorGUI.Foldout(m_DrawingRect, m_Foldout, m_ContentStr, m_ToggleOnLabelClick);
                break;

            case 6:
                m_Foldout = EditorGUI.Foldout(m_DrawingRect, m_Foldout, m_ContentStr, m_Style);
                break;

            case 7:
                m_Foldout = EditorGUI.Foldout(m_DrawingRect, m_Foldout, m_Content, m_ToggleOnLabelClick, m_Style);
                break;

            case 8:
                m_Foldout = EditorGUI.Foldout(m_DrawingRect, m_Foldout, m_ContentStr, m_ToggleOnLabelClick, m_Style);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent content)
    {
        drawingStyle = 1;

        m_Content = content;
    }

    public void SetDrawingStyle(string content)
    {
        drawingStyle = 2;

        m_ContentStr = content;
    }

    public void SetDrawingStyle(GUIContent content,bool toggleOnLabelClick)
    {
        drawingStyle = 3;

        m_Content = content;
        m_ToggleOnLabelClick = toggleOnLabelClick;
    }

    public void SetDrawingStyle(GUIContent content,GUIStyle style)
    {
        drawingStyle = 4;

        m_Content = content;
        m_Style = style;
    }

    public void SetDrawingStyle(string content, bool toggleOnLabelClick)
    {
        drawingStyle = 5;

        m_ContentStr = content;
        m_ToggleOnLabelClick = toggleOnLabelClick;
    }

    public void SetDrawingStyle(string content, GUIStyle style)
    {
        drawingStyle = 6;

        m_ContentStr = content;
        m_Style = style;
    }

    public void SetDrawingStyle(GUIContent content, bool toggleOnLabelClick,GUIStyle style)
    {
        drawingStyle = 7;

        m_Content = content;
        m_ToggleOnLabelClick = toggleOnLabelClick;
        m_Style = style;
    }

    public void SetDrawingStyle(string content, bool toggleOnLabelClick, GUIStyle style)
    {
        drawingStyle = 8;

        m_ContentStr = content;
        m_ToggleOnLabelClick = toggleOnLabelClick;
        m_Style = style;
    }

    public bool Foldout
    {
        get
        {
            return m_Foldout;
        }
        set
        {
            m_Foldout = value;
        }
    }
}
