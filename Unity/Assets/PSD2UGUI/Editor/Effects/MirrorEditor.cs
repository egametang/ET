using System.Collections;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///
/// name:MirrorEditor
/// author:Administrator
/// date:2017/2/4 10:18:04
/// versions:
/// introduce:
/// note:
/// 
/// </summary>
namespace UGUI.Effects
{
    [CustomEditor(typeof(Mirror), true)]
    [CanEditMultipleObjects]
    public class MirrorEditor : Editor
    {
        protected SerializedProperty m_MirrorType;

        private GUIContent m_CorrectButtonContent;
        private GUIContent m_MirrorTypeContent;

        protected virtual void OnDisable()
        {
            
        }

        protected virtual void OnEnable()
        {
            m_CorrectButtonContent = new GUIContent("Set Native Size", "Sets the size to match the content.");

            m_MirrorTypeContent = new GUIContent("Mirror Type");

            m_MirrorType = serializedObject.FindProperty("m_MirrorType");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_MirrorType, m_MirrorTypeContent);

            if (GUILayout.Button(m_CorrectButtonContent, EditorStyles.miniButton))
            {
                int len = targets.Length;

                for (int i = 0; i < len; i++)
                {
                    if (targets[i] is Mirror)
                    {
                        Mirror mirror = targets[i] as Mirror;

                        Undo.RecordObject(mirror.rectTransform, "Set Native Size");
                        mirror.SetNativeSize();
                        EditorUtility.SetDirty(mirror);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
