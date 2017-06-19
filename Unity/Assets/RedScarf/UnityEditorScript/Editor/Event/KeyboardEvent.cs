using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UESKeyboardEvent : UESEvent {

    public const string KEY_DOWN        = "KeyDown";
    public const string KEY_UP          = "KeyUp";

    public UESKeyboardEvent(UESObject target, string type, Action<UESEvent> callback)
        : base(target, type, callback)
    {

    }

    public override void OnGUI()
    {
        m_CurrentEvent = Event.current;
        if (Event.current.isKey&&Event.current.keyCode!=KeyCode.None)
        {
            switch (Event.current.type)
            {
                case EventType.KeyDown:
                    if (m_Type == KEY_DOWN) Invoke();
                    break;

                case EventType.KeyUp:
                    if (m_Type == KEY_UP) Invoke();
                    break;
            }
        }
    }
}
