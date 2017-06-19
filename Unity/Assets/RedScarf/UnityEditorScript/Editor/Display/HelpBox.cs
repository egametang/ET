using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESHelpBox : UESDisplayObject {

    protected string m_Message;
    protected MessageType m_MessageType;

    protected override void Awake()
    {
        base.Awake();
        SetDrawingStyle("message info...");
        Rect = new Rect(Vector2.zero,new Vector2(INIT_WIDTH,48));
    }

    public override void OnGUI()
    {
        EditorGUI.HelpBox(m_DrawingRect, m_Message, m_MessageType);
    }

    public void SetDrawingStyle(string message,MessageType messageType=MessageType.Info)
    {
        m_Message = message;
        m_MessageType = messageType;
    }
}
