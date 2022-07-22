using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BUI
{
    [CustomEditor(typeof(BUIMember))]
    public class BUIMemberCustomInspector : Editor
    {
        private BUIMember _member;
        private void OnEnable()
        {
            _member = (BUIMember)target;
            if (string.IsNullOrEmpty(_member.Name))
            {
                _member.Name = _member.name;
            }
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("名字:", GUILayout.Width(40));
            _member.Name = GUILayout.TextField(_member.Name, GUILayout.Width(200));
            if (GUILayout.Button("同步"))
            {
                _member.name = _member.Name;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("类型:", GUILayout.Width(40));
            _member.SolveType();
            _member.Type = (BUIType)EditorGUILayout.EnumPopup(_member.Type);
            GUILayout.EndHorizontal();
        }

        [MenuItem("BUI/AttachBUIMember &1")]
        public static void AttachBUIMember()
        {
            GameObject gob = Selection.activeGameObject;
            if (gob != null)
            {
                if (!gob.GetComponent<BUIMember>())
                {
                    gob.AddComponent<BUIMember>();
                }
            }
        }
        [MenuItem("BUI/AttachBUIRoot &2")]
        public static void AttachBUIRoot()
        {
            GameObject gob = Selection.activeGameObject;
            if (gob != null)
            {
                if (!gob.GetComponent<BUIMember>())
                {
                    gob.AddComponent<BUIRoot>();
                }
            }
        }
    }
}
