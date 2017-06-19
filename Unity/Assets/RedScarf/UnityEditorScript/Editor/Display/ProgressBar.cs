using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

public class UESProgressBar : UESDisplayObject
{
    protected float m_Value;
    protected string m_Text;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("ProgressBar");
        Value=0.3f;
    }

    public override void OnGUI()
    {
        EditorGUI.ProgressBar(m_DrawingRect, m_Value, m_Text);
    }

    public void SetDrawingStyle(string text)
    {
        m_Text = text;
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

