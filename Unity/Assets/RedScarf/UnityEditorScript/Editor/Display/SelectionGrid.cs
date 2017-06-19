using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESSelectionGrid : UESDisplayObject
{
    protected int m_Selected;
    protected int m_XCount;
    protected GUIContent[] m_Content;
    protected int drawingStyle;
    protected string[] m_Texts;
    protected Texture[] m_Images;
    protected GUIStyle m_Style;

    protected override void Awake()
    {
        base.Awake();
        var texts = new string[]
        {
            "1",
            "2",
            "3"
        };
        SetDrawingStyle(texts, texts.Length);
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                m_Selected = GUI.SelectionGrid(m_DrawingRect, m_Selected, m_Content, m_XCount);
                break;

            case 2:
                m_Selected = GUI.SelectionGrid(m_DrawingRect, m_Selected, m_Texts, m_XCount);
                break;

            case 3:
                m_Selected = GUI.SelectionGrid(m_DrawingRect, m_Selected, m_Images, m_XCount);
                break;

            case 4:
                m_Selected = GUI.SelectionGrid(m_DrawingRect, m_Selected, m_Content, m_XCount,m_Style);
                break;

            case 5:
                m_Selected = GUI.SelectionGrid(m_DrawingRect, m_Selected, m_Texts, m_XCount,m_Style);
                break;

            case 6:
                m_Selected = GUI.SelectionGrid(m_DrawingRect, m_Selected, m_Images, m_XCount,m_Style);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent[]content,int xCount)
    {
        drawingStyle = 1;

        m_Content = content;
        m_XCount = xCount;
    }

    public void SetDrawingStyle(string[] texts, int xCount)
    {
        drawingStyle = 2;

        m_Texts = texts;
        m_XCount = xCount;
    }

    public void SetDrawingStyle(Texture[] images, int xCount)
    {
        drawingStyle = 3;

        m_Images=images;
        m_XCount = xCount;
    }

    public void SetDrawingStyle(GUIContent[] content, int xCount,GUIStyle style)
    {
        drawingStyle = 4;

        m_Content = content;
        m_XCount = xCount;
        m_Style = style;
    }

    public void SetDrawingStyle(string[] texts, int xCount, GUIStyle style)
    {
        drawingStyle = 5;

        m_Texts = texts;
        m_XCount = xCount;
        m_Style = style;
    }

    public void SetDrawingStyle(Texture[] images, int xCount, GUIStyle style)
    {
        drawingStyle = 6;

        m_Images = images;
        m_XCount = xCount;
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