using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UESBox : UESDisplayObject {

    protected string m_Text;
    protected GUIContent m_Content;
    protected Texture m_Image;
    protected GUIStyle m_Style;
    protected int drawingStyle;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("Box");
        Rect = new Rect(Vector2.zero,new Vector2(INIT_WIDTH,32));
    }

    public override void OnGUI()
    {
        base.OnGUI();
        switch (drawingStyle)
        {
            case 1:
                GUI.Box(m_DrawingRect, m_Content);
                break;

            case 2:
                GUI.Box(m_DrawingRect, m_Text);
                break;

            case 3:
                GUI.Box(m_DrawingRect, m_Image);
                break;

            case 4:
                GUI.Box(m_DrawingRect, m_Content,m_Style);
                break;

            case 5:
                GUI.Box(m_DrawingRect, m_Text,m_Style);
                break;

            case 6:
                GUI.Box(m_DrawingRect, m_Image,m_Style);
                break;
        }
    }

    public void SetDrawingStyle(GUIContent content)
    {
        drawingStyle = 1;

        m_Content = content;
    }

    public void SetDrawingStyle(string text)
    {
        drawingStyle = 2;

        m_Text = text;
    }

    public void SetDrawingStyle(Texture image)
    {
        drawingStyle = 3;

        m_Image = image;
    }

    public void SetDrawingStyle(GUIContent content,GUIStyle style)
    {
        drawingStyle = 4;

        m_Content = content;
        m_Style = style;
    }

    public void SetDrawingStyle(string text, GUIStyle style)
    {
        drawingStyle = 5;

        m_Text = text;
        m_Style = style;
    }

    public void SetDrawingStyle(Texture image, GUIStyle style)
    {
        drawingStyle = 6;

        m_Image = image;
        m_Style = style;
    }

    public string Text
    {
        get
        {
            return m_Text;
        }
        set
        {
            m_Text = value;
        }
    }
}