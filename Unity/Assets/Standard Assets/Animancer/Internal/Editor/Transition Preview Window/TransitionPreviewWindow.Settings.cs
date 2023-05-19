// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    internal partial class TransitionPreviewWindow
    {
        /// <summary>Persistent settings for the <see cref="TransitionPreviewWindow"/>.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#previews">Previews</see>
        /// </remarks>
        [Serializable]
        internal sealed class Settings : AnimancerSettings.Group
        {
            /************************************************************************************************************************/

            private static Settings Instance => AnimancerSettings.TransitionPreviewWindow;

            /************************************************************************************************************************/

            public static void DoInspectorGUI()
            {
                AnimancerSettings.SerializedObject.Update();

                EditorGUI.indentLevel++;

                DoMiscGUI();
                DoModelsGUI();
                DoFloorGUI();
                DoGatheringExceptionsGUI();
                _Instance._Scene.DoHierarchyGUI();

                EditorGUI.indentLevel--;

                AnimancerSettings.SerializedObject.ApplyModifiedProperties();
            }

            /************************************************************************************************************************/
            #region Misc
            /************************************************************************************************************************/

            private static void DoMiscGUI()
            {
                Instance.DoPropertyField(nameof(_AutoClose));
                Instance.DoPropertyField(nameof(_ShowTransition));

                var property = Instance.DoPropertyField(nameof(_MovementSensitivity));
                property.floatValue = Mathf.Clamp(property.floatValue, 0.01f, 100f);

                property = Instance.DoPropertyField(nameof(_RotationSensitivity));
                property.floatValue = Mathf.Clamp(property.floatValue, 0.01f, 100f);
            }

            /************************************************************************************************************************/

            [SerializeField]
            private float _InspectorWidth = 300;

            public static float InspectorWidth
            {
                get => Instance._InspectorWidth;
                set
                {
                    Instance._InspectorWidth = value;
                    AnimancerSettings.SetDirty();
                }
            }

            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("Should this window automatically close if the target object is destroyed?")]
            private bool _AutoClose = true;

            public static bool AutoClose => Instance._AutoClose;

            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("Should the transition be displayed in this window?" +
                " Otherwise you can still edit it using the regular Inspector.")]
            private bool _ShowTransition = true;

            public static bool ShowTransition => Instance._ShowTransition;

            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("Determines how fast the camera moves when you Left Click and Drag")]
            private float _MovementSensitivity = 1;

            public static float MovementSensitivity => Instance._MovementSensitivity;

            [SerializeField]
            [Tooltip("Determines how fast the camera rotates when you Right Click and Drag")]
            private float _RotationSensitivity = 1;

            public static float RotationSensitivity => Instance._RotationSensitivity;

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Models
            /************************************************************************************************************************/

            private static void DoModelsGUI()
            {
                var property = Instance.DoPropertyField(nameof(_MaxRecentModels));
                property.intValue = Math.Max(property.intValue, 0);

                property = ModelsProperty;

                var count = property.arraySize = EditorGUILayout.DelayedIntField(nameof(Models), property.arraySize);

                // Drag and Drop to add model.
                var area = GUILayoutUtility.GetLastRect();
                AnimancerGUI.HandleDragAndDrop<GameObject>(area,
                    (gameObject) =>
                    {
                        return
                            EditorUtility.IsPersistent(gameObject) &&
                            !Models.Contains(gameObject) &&
                            gameObject.GetComponentInChildren<Animator>() != null;
                    },
                    (gameObject) =>
                    {
                        var modelsProperty = ModelsProperty;// Avoid Closure.
                        modelsProperty.serializedObject.Update();

                        var i = modelsProperty.arraySize;
                        modelsProperty.arraySize = i + 1;
                        modelsProperty.GetArrayElementAtIndex(i).objectReferenceValue = gameObject;
                        modelsProperty.serializedObject.ApplyModifiedProperties();
                    });

                if (count == 0)
                    return;

                property.isExpanded = EditorGUI.Foldout(area, property.isExpanded, GUIContent.none, true);
                if (!property.isExpanded)
                    return;

                EditorGUI.indentLevel++;

                var model = property.GetArrayElementAtIndex(0);
                for (int i = 0; i < count; i++)
                {
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.ObjectField(model);

                    if (GUILayout.Button("x", AnimancerGUI.MiniButton))
                    {
                        Serialization.RemoveArrayElement(property, i);
                        property.serializedObject.ApplyModifiedProperties();

                        AnimancerGUI.Deselect();
                        GUIUtility.ExitGUI();
                        return;
                    }

                    GUILayout.Space(EditorStyles.objectField.margin.right);
                    GUILayout.EndHorizontal();
                    model.Next(false);
                }

                EditorGUI.indentLevel--;
            }

            /************************************************************************************************************************/

            [SerializeField]
            private List<GameObject> _Models;

            public static List<GameObject> Models
            {
                get
                {
                    if (AnimancerUtilities.NewIfNull(ref Instance._Models))
                        return Instance._Models;

                    var previousModels = ObjectPool.AcquireSet<Object>();
                    var modified = false;
                    for (int i = Instance._Models.Count - 1; i >= 0; i--)
                    {
                        var model = Instance._Models[i];
                        if (model == null || previousModels.Contains(model))
                        {
                            Instance._Models.RemoveAt(i);
                            modified = true;
                        }
                        else
                        {
                            previousModels.Add(model);
                        }
                    }
                    ObjectPool.Release(previousModels);
                    if (modified)
                        ModelsProperty.OnPropertyChanged();

                    return Instance._Models;
                }
            }

            private static SerializedProperty ModelsProperty => Instance.GetSerializedProperty(nameof(_Models));

            /************************************************************************************************************************/

            [SerializeField]
            private int _MaxRecentModels = 10;

            public static int MaxRecentModels => Instance._MaxRecentModels;

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Floor
            /************************************************************************************************************************/

            private static void DoFloorGUI()
            {
                AnimancerGUI.BeginVerticalBox(GUI.skin.box);

                var enabled = GUI.enabled;

                var property = Instance.DoPropertyField(nameof(_FloorEnabled));

                if (!property.boolValue)
                    GUI.enabled = false;

                EditorGUI.BeginChangeCheck();

                property = Instance.DoPropertyField(nameof(_FloorMaterial));

                if (property.objectReferenceValue == null)
                    GUI.enabled = false;

                Instance.DoPropertyField(nameof(_FloorTexturePropertyName));

                if (EditorGUI.EndChangeCheck())
                    Scene.Floor.DiscardCustomMaterial();

                if (GUILayout.Button(AnimancerGUI.TempContent("Apply Changes",
                    $"Changes to the {nameof(Material)} itself will not be automatically applied")))
                    Scene.Floor.DiscardCustomMaterial();

                GUI.enabled = enabled;

                AnimancerGUI.EndVerticalBox(GUI.skin.box);
            }

            /************************************************************************************************************************/

            [SerializeField]
            private bool _FloorEnabled = true;

            public static bool FloorEnabled => Instance._FloorEnabled;

            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("Scriptable Render Pipelines regularly break the default shader, so this field allows custom material to be" +
                " assigned if necessary. A transparent shader like 'Mobile/Particles/Alpha Blended' is recommended.")]
            private Material _FloorMaterial;

            public static Material FloorMaterial => Instance._FloorMaterial;

            /************************************************************************************************************************/

            [SerializeField]
            [Tooltip("The name of the property to assign the grid texture for the Floor Material (default '_MainTex')")]
            private string _FloorTexturePropertyName = "_MainTex";

            public static string FloorTexturePropertyName => Instance._FloorTexturePropertyName;

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
            #region Exceptions
            /************************************************************************************************************************/

            private struct ExceptionDetails
            {
                public Exception Exception { get; private set; }
                public bool isExpanded;

                public ExceptionDetails(Exception exception)
                {
                    Exception = exception;
                    isExpanded = false;
                    _ToString = null;
                }

                private string _ToString;
                public override string ToString() => _ToString ?? (_ToString = Exception.ToString());
            }

            private static List<ExceptionDetails> _ExceptionDetails;

            /************************************************************************************************************************/

            private static void DoGatheringExceptionsGUI()
            {
                var exceptions = AnimationGatherer.Exceptions;
                if (exceptions == null || exceptions.Count == 0)
                    return;

                AnimancerUtilities.NewIfNull(ref _ExceptionDetails);

                if (Event.current.type == EventType.Layout)
                {
                    while (_ExceptionDetails.Count < exceptions.Count)
                        _ExceptionDetails.Add(new ExceptionDetails(exceptions[_ExceptionDetails.Count]));
                }

                EditorGUILayout.LabelField("Gathering Exceptions", _ExceptionDetails.Count.ToString());
                EditorGUI.indentLevel++;

                if (GUILayout.Button("Log All"))
                {
                    for (int i = 0; i < _ExceptionDetails.Count; i++)
                        Debug.LogException(_ExceptionDetails[i].Exception);
                }

                if (GUILayout.Button("Clear"))
                {
                    _ExceptionDetails = null;
                    return;
                }

                for (int i = 0; i < _ExceptionDetails.Count; i++)
                {
                    var exception = _ExceptionDetails[i];
                    Rect area;

                    if (exception.isExpanded)
                    {
                        var size = GUI.skin.label.CalcSize(AnimancerGUI.TempContent(exception.ToString()));
                        area = GUILayoutUtility.GetRect(size.x + AnimancerGUI.IndentSize, size.y);
                        area.xMin += AnimancerGUI.IndentSize;
                        GUI.Label(area, exception.ToString());
                    }
                    else
                    {
                        area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.After);
                        area.xMin += AnimancerGUI.IndentSize;
                        GUI.Label(area, exception.ToString());
                    }

                    area.x = 0;
                    exception.isExpanded = EditorGUI.Foldout(area, exception.isExpanded, GUIContent.none, true);
                    _ExceptionDetails[i] = exception;
                }

                EditorGUI.indentLevel--;
            }

            /************************************************************************************************************************/
            #endregion
            /************************************************************************************************************************/
        }
    }
}

#endif

