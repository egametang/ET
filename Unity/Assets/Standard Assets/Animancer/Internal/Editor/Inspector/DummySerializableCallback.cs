// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Animancer.Editor
{
    /// <summary>
    /// An object that holds a serialized callback (a <see cref="UnityEvent"/> by default) so that empty ones can be
    /// drawn in the GUI without allocating array space for them until they actually contain something.
    /// </summary>
    internal sealed class DummySerializableCallback : ScriptableObject
    {
        /************************************************************************************************************************/

        [SerializeField] private SerializableCallbackHolder _Holder;

        /************************************************************************************************************************/

        private static SerializedProperty _CallbackProperty;

        private static SerializedProperty CallbackProperty
        {
            get
            {
                if (_CallbackProperty == null)
                {
                    var instance = CreateInstance<DummySerializableCallback>();

                    instance.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                    var serializedObject = new SerializedObject(instance);
                    _CallbackProperty = serializedObject.FindProperty(
                        $"{nameof(_Holder)}.{SerializableCallbackHolder.CallbackField}");

                    AssemblyReloadEvents.beforeAssemblyReload += () =>
                    {
                        serializedObject.Dispose();
                        DestroyImmediate(instance);
                    };
                }

                return _CallbackProperty;
            }
        }

        /************************************************************************************************************************/

        public static float Height => EditorGUI.GetPropertyHeight(CallbackProperty);

        /************************************************************************************************************************/

        public static bool DoCallbackGUI(ref Rect area, GUIContent label, SerializedProperty property,
            out object callback)
        {
            var callbackProperty = CallbackProperty;
            callbackProperty.serializedObject.Update();
            callbackProperty.prefabOverride = property.prefabOverride;

            area.height = Height;

            EditorGUI.BeginChangeCheck();
            label = EditorGUI.BeginProperty(area, label, property);

            // UnityEvents ignore the proper indentation which makes them look terrible in a list.
            // So we force the area to be indented.
            var indentedArea = EditorGUI.IndentedRect(area);
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.PropertyField(indentedArea, callbackProperty, label, false);

            EditorGUI.indentLevel = indentLevel;

            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                callbackProperty.serializedObject.ApplyModifiedProperties();
                callback = callbackProperty.GetValue();

                callbackProperty.SetValue(null);
                callbackProperty.serializedObject.Update();

                if (AnimancerEvent.Sequence.Serializable.HasPersistentCalls(callback))
                    return true;
            }

            callback = null;
            return false;
        }

        /************************************************************************************************************************/
    }
}

#endif
