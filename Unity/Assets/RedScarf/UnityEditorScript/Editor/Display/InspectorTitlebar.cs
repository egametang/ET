using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESInspectorTitlebar : UESDisplayObject {

    protected UnityEngine.Object[] m_TargetObjs;
    protected UnityEngine.Object m_TargetObj;
    protected bool m_Foldout;
    protected bool m_Expandable;
    protected int drawingStyle;

    protected override void Awake()
    {
        SetDrawingStyle(Camera.main, true);
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                EditorGUI.InspectorTitlebar(m_DrawingRect, m_TargetObjs);
                break;

            case 2:
                m_Foldout = EditorGUI.InspectorTitlebar(m_DrawingRect, m_Foldout, m_TargetObj, m_Expandable);
                break;

            case 3:
                m_Foldout = EditorGUI.InspectorTitlebar(m_DrawingRect, m_Foldout, m_TargetObjs, m_Expandable);
                break;
        }
    }

    public void SetDrawingStyle(UnityEngine.Object[] targetObjs)
    {
        drawingStyle = 1;

        m_TargetObjs = targetObjs;
    }

    public void SetDrawingStyle(UnityEngine.Object targetObj, bool expandable)
    {
        drawingStyle = 2;

        m_TargetObj = targetObj;
        m_Expandable = expandable;
    }

    public void SetDrawingStyle(UnityEngine.Object[] targetObjs,bool expandable)
    {
        drawingStyle = 3;

        m_TargetObjs = targetObjs;
        m_Expandable = expandable;
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
