using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(LoopScrollRect), true)]
public class LoopScrollRectInspector : Editor
{
    int index = 0;
    float speed = 1000, time = 1;
    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        LoopScrollRect scroll = (LoopScrollRect)target;
        GUI.enabled = Application.isPlaying;

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Clear"))
        {
            scroll.ClearCells();
        }
        if (GUILayout.Button("Refresh"))
        {
            scroll.RefreshCells();
        }
        if(GUILayout.Button("Refill"))
        {
            scroll.RefillCells();
        }
        if(GUILayout.Button("RefillFromEnd"))
        {
            scroll.RefillCellsFromEnd();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 45;
        float w = (EditorGUIUtility.currentViewWidth - 100) / 2;
        index = EditorGUILayout.IntField("Index", index, GUILayout.Width(w));
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60;
        speed = EditorGUILayout.FloatField("    Speed", speed, GUILayout.Width(w+15));
        if(GUILayout.Button("Scroll With Speed", GUILayout.Width(130)))
        {
            scroll.SrollToCell(index, speed);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60;
        time = EditorGUILayout.FloatField("    Time", time, GUILayout.Width(w+15));
        if(GUILayout.Button("Scroll Within Time", GUILayout.Width(130)))
        {
            scroll.SrollToCellWithinTime(index, time);
        }
        EditorGUILayout.EndHorizontal();
    }
}