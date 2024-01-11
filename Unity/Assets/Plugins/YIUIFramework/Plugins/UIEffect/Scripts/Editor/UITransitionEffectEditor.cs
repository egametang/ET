using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;

namespace Coffee.UIEffects.Editors
{
    /// <summary>
    /// UIEffect editor.
    /// </summary>
    [CustomEditor(typeof(UITransitionEffect))]
    [CanEditMultipleObjects]
    public class UITransitionEffectEditor : Editor
    {
        SerializedProperty _spEffectMode;
        SerializedProperty _spEffectFactor;
        SerializedProperty _spEffectArea;
        SerializedProperty _spKeepAspectRatio;
        SerializedProperty _spDissolveWidth;
        SerializedProperty _spDissolveSoftness;
        SerializedProperty _spDissolveColor;
        SerializedProperty _spTransitionTexture;
        SerializedProperty _spPlay;
        SerializedProperty _spLoop;
        SerializedProperty _spLoopDelay;
        SerializedProperty _spDuration;
        SerializedProperty _spInitialPlayDelay;
        SerializedProperty _spUpdateMode;
        SerializedProperty _spPassRayOnHidden;

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected void OnEnable()
        {
            _spEffectMode = serializedObject.FindProperty("m_EffectMode");
            _spEffectFactor = serializedObject.FindProperty("m_EffectFactor");
            _spEffectArea = serializedObject.FindProperty("m_EffectArea");
            _spKeepAspectRatio = serializedObject.FindProperty("m_KeepAspectRatio");
            _spDissolveWidth = serializedObject.FindProperty("m_DissolveWidth");
            _spDissolveSoftness = serializedObject.FindProperty("m_DissolveSoftness");
            _spDissolveColor = serializedObject.FindProperty("m_DissolveColor");
            _spTransitionTexture = serializedObject.FindProperty("m_TransitionTexture");
            var player = serializedObject.FindProperty("m_Player");
            _spPlay = player.FindPropertyRelative("play");
            _spDuration = player.FindPropertyRelative("duration");
            _spInitialPlayDelay = player.FindPropertyRelative("initialPlayDelay");
            _spLoop = player.FindPropertyRelative("loop");
            _spLoopDelay = player.FindPropertyRelative("loopDelay");
            _spUpdateMode = player.FindPropertyRelative("updateMode");
            _spPassRayOnHidden = serializedObject.FindProperty("m_PassRayOnHidden");
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            //================
            // Effect setting.
            //================
            using (new MaterialDirtyScope(targets))
                EditorGUILayout.PropertyField(_spEffectMode);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_spEffectFactor);
            if (_spEffectMode.intValue == (int) UITransitionEffect.EffectMode.Dissolve)
            {
                EditorGUILayout.PropertyField(_spDissolveWidth);
                EditorGUILayout.PropertyField(_spDissolveSoftness);
                EditorGUILayout.PropertyField(_spDissolveColor);
            }

            EditorGUI.indentLevel--;

            //================
            // Advanced option.
            //================
            EditorGUILayout.PropertyField(_spEffectArea);
            using (new MaterialDirtyScope(targets))
                EditorGUILayout.PropertyField(_spTransitionTexture);
            EditorGUILayout.PropertyField(_spKeepAspectRatio);
            EditorGUILayout.PropertyField(_spPassRayOnHidden);

            //================
            // Effect player.
            //================
            EditorGUILayout.PropertyField(_spPlay);
            EditorGUILayout.PropertyField(_spDuration);
            EditorGUILayout.PropertyField(_spInitialPlayDelay);
            EditorGUILayout.PropertyField(_spLoop);
            EditorGUILayout.PropertyField(_spLoopDelay);
            EditorGUILayout.PropertyField(_spUpdateMode);

            // Debug.
            using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Debug");

                if (GUILayout.Button("Show", "ButtonLeft"))
                {
                    (target as UITransitionEffect).Show();
                }

                if (GUILayout.Button("Hide", "ButtonRight"))
                {
                    (target as UITransitionEffect).Hide();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
