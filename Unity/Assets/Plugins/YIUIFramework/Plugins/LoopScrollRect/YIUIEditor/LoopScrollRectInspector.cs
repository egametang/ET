#if UNITY_EDITOR
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
        
        if (GUILayout.Button("刷新"))
        {
            scroll.RefreshCells();
        }
        
        /*if(GUILayout.Button("Clear"))
        {
            scroll.ClearCells();
        }
        if(GUILayout.Button("Refill"))
        {
            scroll.RefillCells();
        }
        if(GUILayout.Button("RefillFromEnd"))
        {
            scroll.RefillCellsFromEnd();
        }*/
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 45;
        float w = (EditorGUIUtility.currentViewWidth - 100) / 2;
        index = EditorGUILayout.IntField("    索引", index, GUILayout.Width(w));
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60;
        speed = EditorGUILayout.FloatField("    滚动速度", speed, GUILayout.Width(w));
        if(GUILayout.Button("滚动跳转", GUILayout.Width(130)))
        {
            if (scroll.totalCount <= 0)
            {
                return;
            }
            index = Mathf.Clamp(index, 0, scroll.totalCount - 1);
            scroll.ScrollToCell(index, speed);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60;
        time = EditorGUILayout.FloatField("    滚动时间", time, GUILayout.Width(w));
        if(GUILayout.Button("时间跳转", GUILayout.Width(130)))
        {
            if (scroll.totalCount <= 0)
            {
                return;
            }
            index = Mathf.Clamp(index, 0, scroll.totalCount - 1);
            scroll.ScrollToCellWithinTime(index, time);
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif