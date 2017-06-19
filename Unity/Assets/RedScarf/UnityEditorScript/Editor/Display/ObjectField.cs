using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESObjectField : UESDisplayObject
{
    protected SerializedProperty m_Property;
    protected GUIContent m_Label;
    protected string m_LabelStr;
    protected Type m_ObjType;
    protected UnityEngine.Object m_Obj;
    protected bool m_AllowSceneObjects;
    protected int drawingStyle;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("ObjectField",Camera.main, typeof(Camera), true);
    }

    public override void OnGUI()
    {
        switch (drawingStyle)
        {
            case 1:
                //EditorGUI.ObjectField(m_DrawingRect, m_Property);
                break;

            case 2:
                EditorGUI.ObjectField(m_DrawingRect, m_Property, m_Label);
                break;

            case 3:
                //EditorGUI.ObjectField(m_DrawingRect, m_Property, m_ObjType);
                break;

            case 4:
                //EditorGUI.ObjectField(m_DrawingRect, m_Obj, m_ObjType);
                break;

            case 5:
                //EditorGUI.ObjectField(m_DrawingRect, m_Label, m_Obj, m_ObjType);
                break;

            case 6:
                EditorGUI.ObjectField(m_DrawingRect, m_Property, m_ObjType, m_Label);
                break;

            case 7:
                //EditorGUI.ObjectField(m_DrawingRect, m_LabelStr, m_Obj, m_ObjType);
                break;

            case 8:
                m_Obj=EditorGUI.ObjectField(m_DrawingRect, m_Obj, m_ObjType, m_AllowSceneObjects);
                break;

            case 9:
                m_Obj = EditorGUI.ObjectField(m_DrawingRect, m_Label, m_Obj, m_ObjType, m_AllowSceneObjects);
                break;

            case 10:
                m_Obj = EditorGUI.ObjectField(m_DrawingRect, m_LabelStr, m_Obj, m_ObjType, m_AllowSceneObjects);
                break;
        }
    }

    //public void SetDrawingStyle(SerializedProperty property)
    //{
    //    drawingStyle = 1;

    //    m_Property = property;
    //}

    public void SetDrawingStyle(SerializedProperty property,GUIContent label)
    {
        drawingStyle = 2;

        m_Property = property;
        m_Label = label;
    }

    //public void SetDrawingStyle(SerializedProperty property,Type objType)
    //{
    //    drawingStyle = 3;

    //    m_Property = property;
    //    m_ObjType = objType;
    //}

    public void SetDrawingStyle(SerializedProperty property,Type objType,GUIContent label)
    {
        drawingStyle = 6;

        m_Property = property;
        m_ObjType = objType;
        m_Label = label;
    }

    public void SetDrawingStyle(UnityEngine.Object obj,Type objType,bool allowSceneObjects)
    {
        drawingStyle = 8;

        m_Obj = obj;
        m_ObjType = objType;
        m_AllowSceneObjects = allowSceneObjects;
    }

    public void SetDrawingStyle(GUIContent label,UnityEngine.Object obj, Type objType, bool allowSceneObjects)
    {
        drawingStyle = 9;

        m_Label = label;
        m_Obj = obj;
        m_ObjType = objType;
        m_AllowSceneObjects = allowSceneObjects;
    }

    public void SetDrawingStyle(string label, UnityEngine.Object obj, Type objType, bool allowSceneObjects)
    {
        drawingStyle = 10;

        m_LabelStr = label;
        m_Obj = obj;
        m_ObjType = objType;
        m_AllowSceneObjects = allowSceneObjects;
    }

    public UnityEngine.Object Object
    {
        get
        {
            return m_Obj;
        }
        set
        {
            m_Obj = value;
        }
    }
}
