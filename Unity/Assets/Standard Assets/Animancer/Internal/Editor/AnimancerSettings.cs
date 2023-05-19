// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Persistent settings used by Animancer.</summary>
    [HelpURL(Strings.DocsURLs.APIDocumentation + "." + nameof(Editor) + "/" + nameof(AnimancerSettings))]
    internal sealed class AnimancerSettings : ScriptableObject
    {
        /************************************************************************************************************************/

        private static AnimancerSettings _Instance;

        public static AnimancerSettings Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = AnimancerEditorUtilities.FindAssetOfType<AnimancerSettings>();

                    if (_Instance == null)
                    {
                        _Instance = CreateInstance<AnimancerSettings>();
                        _Instance.name = "Animancer Settings";
                        _Instance.hideFlags = HideFlags.DontSaveInBuild;

                        var script = MonoScript.FromScriptableObject(_Instance);
                        var path = AssetDatabase.GetAssetPath(script);
                        path = Path.Combine(Path.GetDirectoryName(path), _Instance.name + ".asset");
                        AssetDatabase.CreateAsset(_Instance, path);

                        Debug.Log($"Created {nameof(AnimancerSettings)} at '{path}'" +
                            " because an existing instance was not found." +
                            " Feel free to move it (and the entire Animancer folder) anywhere in the project.", _Instance);
                    }
                }

                return _Instance;
            }
        }

        /************************************************************************************************************************/

        private SerializedObject _SerializedObject;

        /// <summary>The <see cref="SerializedProperty"/> representing the <see cref="Instance"/>.</summary>
        public static SerializedObject SerializedObject
            => Instance._SerializedObject ?? (Instance._SerializedObject = new SerializedObject(Instance));

        /************************************************************************************************************************/

        private readonly Dictionary<string, SerializedProperty>
            SerializedProperties = new Dictionary<string, SerializedProperty>();

        private static SerializedProperty GetSerializedProperty(string propertyPath)
        {
            var properties = Instance.SerializedProperties;
            if (!properties.TryGetValue(propertyPath, out var property))
            {
                property = SerializedObject.FindProperty(propertyPath);
                properties.Add(propertyPath, property);
            }

            return property;
        }

        /************************************************************************************************************************/

        /// <summary>Base class for groups of fields that can be serialized inside <see cref="AnimancerSettings"/>.</summary>
        public abstract class Group
        {
            /************************************************************************************************************************/

            private string _BasePropertyPath;

            /// <summary>[Internal] Sets the prefix for <see cref="GetSerializedProperty"/>.</summary>
            internal static void SetBasePropertyPath<T>(ref T group, string propertyPath) where T : Group, new()
            {
                AnimancerUtilities.NewIfNull(ref group);
                group._BasePropertyPath = propertyPath + ".";
            }

            /************************************************************************************************************************/

            protected SerializedProperty GetSerializedProperty(string propertyPath)
                => AnimancerSettings.GetSerializedProperty(_BasePropertyPath + propertyPath);

            /************************************************************************************************************************/

            protected SerializedProperty DoPropertyField(string propertyPath)
            {
                var property = GetSerializedProperty(propertyPath);
                EditorGUILayout.PropertyField(property, true);
                return property;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Group.SetBasePropertyPath(ref _TransitionPreviewWindow, nameof(_TransitionPreviewWindow));
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="EditorUtility.SetDirty"/> on the <see cref="Instance"/>.</summary>
        public static new void SetDirty() => EditorUtility.SetDirty(_Instance);

        /************************************************************************************************************************/

        [SerializeField]
        private TransitionPreviewWindow.Settings _TransitionPreviewWindow;

        /// <summary>Settings for the <see cref="TransitionPreviewWindow"/>.</summary>
        public static TransitionPreviewWindow.Settings TransitionPreviewWindow => Instance._TransitionPreviewWindow;

        /************************************************************************************************************************/

        [SerializeField, Range(0.01f, 1)]
        [Tooltip("The amount of time between repaint commands when 'Display Options/Repaint Constantly' is disabled")]
        private float _InspectorRepaintInterval = 0.25f;

        /// <summary>
        /// The amount of time between repaint commands when
        /// <see cref="AnimancerPlayableDrawer.RepaintConstantly"/> is disabled.
        /// </summary>
        public static float InspectorRepaintInterval => Instance._InspectorRepaintInterval;

        /************************************************************************************************************************/
    }
}

#endif

