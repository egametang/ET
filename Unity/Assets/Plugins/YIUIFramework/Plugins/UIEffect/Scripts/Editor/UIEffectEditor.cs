using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using System;

namespace Coffee.UIEffects.Editors
{
    /// <summary>
    /// UIEffect editor.
    /// </summary>
    [CustomEditor(typeof(UIEffect))]
    [CanEditMultipleObjects]
    public class UIEffectEditor : Editor
    {
        SerializedProperty _spEffectMode;
        SerializedProperty _spEffectFactor;
        SerializedProperty _spColorMode;
        SerializedProperty _spColorFactor;
        SerializedProperty _spBlurMode;
        SerializedProperty _spBlurFactor;
        SerializedProperty _spAdvancedBlur;

        protected void OnEnable()
        {
            _spEffectMode = serializedObject.FindProperty("m_EffectMode");
            _spEffectFactor = serializedObject.FindProperty("m_EffectFactor");
            _spColorMode = serializedObject.FindProperty("m_ColorMode");
            _spColorFactor = serializedObject.FindProperty("m_ColorFactor");
            _spBlurMode = serializedObject.FindProperty("m_BlurMode");
            _spBlurFactor = serializedObject.FindProperty("m_BlurFactor");
            _spAdvancedBlur = serializedObject.FindProperty("m_AdvancedBlur");
        }

        public override void OnInspectorGUI()
        {
            //================
            // Effect setting.
            //================
            using (new MaterialDirtyScope(targets))
                EditorGUILayout.PropertyField(_spEffectMode);

            // When effect is enable, show parameters.
            if (_spEffectMode.intValue != (int) EffectMode.None)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spEffectFactor);
                EditorGUI.indentLevel--;
            }


            //================
            // Color setting.
            //================
            using (new MaterialDirtyScope(targets))
                EditorGUILayout.PropertyField(_spColorMode);

            // When color is enable, show parameters.
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spColorFactor);
                EditorGUI.indentLevel--;
            }


            //================
            // Blur setting.
            //================
            using (new MaterialDirtyScope(targets))
                EditorGUILayout.PropertyField(_spBlurMode);

            // When blur is enable, show parameters.
            if (_spBlurMode.intValue != (int) BlurMode.None)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_spBlurFactor);

                // When you change a property, it marks the material as dirty.
                using (new MaterialDirtyScope(targets))
                    EditorGUILayout.PropertyField(_spAdvancedBlur);
                EditorGUI.indentLevel--;

                // Advanced blur requires uv2 channel.
                if (_spAdvancedBlur.boolValue)
                {
                    ShowCanvasChannelsWarning();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void ShowCanvasChannelsWarning()
        {
            var effect = target as UIEffect;
            if (effect == null || !effect.graphic) return;

            var channel = effect.uvMaskChannel;
            var canvas = effect.graphic.canvas;
            if (canvas == null || (canvas.additionalShaderChannels & channel) == channel) return;

            EditorGUILayout.BeginHorizontal();
            {
                var msg = string.Format("Enable '{0}' of Canvas.additionalShaderChannels to use 'UIEffect'.", channel);
                EditorGUILayout.HelpBox(msg, MessageType.Warning);
                if (GUILayout.Button("Fix"))
                {
                    canvas.additionalShaderChannels |= channel;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
