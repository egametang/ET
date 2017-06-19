using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESToolbar : UESDisplayObject
{
    protected int m_Selected;
    protected GUIContent[] m_Content;
    protected Texture[] m_Images;
    protected string[] m_Texts;
    protected GUIStyle m_Style;
    protected int drawingStyle;

    protected override void Awake()
    {
        base.Awake();
        var texts = new string[]
        {
            "1",
            "2",
            "3"
        };
        SetDrawingStyle(texts);
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Selected = GUI.Toolbar(m_DrawingRect, m_Selected, m_Content);
                break;

            case 2:
                m_Selected = GUI.Toolbar(m_DrawingRect, m_Selected, m_Texts);
                break;

            case 3:
                m_Selected = GUI.Toolbar(m_DrawingRect, m_Selected, m_Images);
                break;

            case 4:
                m_Selected = GUI.Toolbar(m_DrawingRect, m_Selected, m_Content,m_Style);
                break;

            case 5:
                m_Selected = GUI.Toolbar(m_DrawingRect, m_Selected, m_Texts,m_Style);
                break;

            case 6:
                m_Selected = GUI.Toolbar(m_DrawingRect, m_Selected, m_Images,m_Style);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent[] content)
    {
        drawingStyle = 1;

        m_Content = content;
    }

    public void SetDrawingStyle(string[] texts)
    {
        drawingStyle = 2;

        m_Texts = texts;
    }

    public void SetDrawingStyle(Texture[] images)
    {
        drawingStyle = 3;

        m_Images = images;
    }

    public void SetDrawingStyle(GUIContent[] content,GUIStyle style)
    {
        drawingStyle = 4;

        m_Content = content;
        m_Style = style;
    }

    public void SetDrawingStyle(string[] texts, GUIStyle style)
    {
        drawingStyle = 5;

        m_Texts = texts;
        m_Style = style;
    }

    public void SetDrawingStyle(Texture[] images, GUIStyle style)
    {
        drawingStyle = 6;

        m_Images = images;
        m_Style = style;
    }

    public int Selected
    {
        get
        {
            return m_Selected;
        }
        set
        {
            m_Selected = value;
        }
    }
}
