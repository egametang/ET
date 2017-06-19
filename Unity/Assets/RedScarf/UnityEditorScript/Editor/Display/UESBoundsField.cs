using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESBoundsField : UESDisplayObject
{
    protected Bounds bounds;
    protected int drawingStyle;
    protected GUIContent m_Label;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle(new GUIContent("BoundField"));
        Rect = new Rect(Vector2.zero, new Vector2(INIT_WIDTH, 48));
    }

    public override void OnGUI()
    {
        base.OnGUI();
        switch (drawingStyle)
        {
            case 1:
                bounds = EditorGUI.BoundsField(m_DrawingRect, bounds);
                break;

            case 2:
                bounds = EditorGUI.BoundsField(m_DrawingRect, m_Label, bounds);
                break;
        }
    }

    public void SerDrawingStyle()
    {
        drawingStyle = 1;
    }

    public void SetDrawingStyle(GUIContent label)
    {
        drawingStyle = 2;

        m_Label = label;
    }

    /// <summary>
    /// Bounds值
    /// </summary>
    public Bounds Bounds
    {
        get
        {
            return bounds;
        }
        set
        {
            bounds = value;
        }
    }
}