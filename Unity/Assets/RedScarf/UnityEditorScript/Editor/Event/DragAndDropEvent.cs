using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UESDragAndDropEvent : UESEvent {

    public const string DRAG_PERFORM = "DragPerform";
    public const string DRAG_UPDATED = "DragUpdated";
    public const string DRAG_EXITED  = "DragExited";
    public const string DRAG_ENTER   = "DragEnter";

    public int activeControlID;
    public Object[] objectReferences;
    public string[] paths;

    protected enum DragSteps
    {
        Enter,
        Update,
        Perform,
        Exit
    }
    protected DragSteps dragStep;

    public UESDragAndDropEvent(UESObject target, string type, System.Action<UESEvent> callback) 
        : base(target, type, callback)
    {
    }

    public override void OnGUI()
    {
        var display = m_CurrentTarget as UESDisplayObject;
        if (display == null) return;

        base.OnGUI();

        activeControlID = DragAndDrop.activeControlID;
        objectReferences = DragAndDrop.objectReferences;
        paths = DragAndDrop.paths;

        if (Event.current.type == EventType.DragExited)
        {
            dragStep = DragSteps.Exit;
            if(m_Type == DRAG_EXITED) Invoke();
        }

        if (!display.GlobalHitArea.Contains(Event.current.mousePosition)) return;

        switch (Event.current.type)
        {
            case EventType.DragPerform:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                DragAndDrop.AcceptDrag();
                dragStep = DragSteps.Perform;
                if (m_Type == DRAG_PERFORM) Invoke();
                break;

            case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (m_Type == DRAG_ENTER && dragStep != DragSteps.Update)
                {
                    dragStep = DragSteps.Enter;
                    Invoke();
                }
                dragStep = DragSteps.Update;
                if (m_Type == DRAG_UPDATED&& dragStep == DragSteps.Update) Invoke();
                break;
        }
    }
}
