// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom Inspector for <see cref="NamedAnimancerComponent"/>s.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/NamedAnimancerComponentEditor
    /// 
    [CustomEditor(typeof(NamedAnimancerComponent), true), CanEditMultipleObjects]
    public class NamedAnimancerComponentEditor : AnimancerComponentEditor
    {
        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Draws any custom GUI for the `property`. The return value indicates whether the GUI should replace the
        /// regular call to <see cref="EditorGUILayout.PropertyField"/> or not.
        /// </summary>
        protected override bool DoOverridePropertyGUI(string path, SerializedProperty property, GUIContent label)
        {
            switch (path)
            {
                case "_PlayAutomatically":
                    if (ShouldShowAnimationFields())
                        DoDefaultAnimationField(property);
                    return true;

                case "_Animations":
                    if (ShouldShowAnimationFields())
                    {
                        DoAnimationsField(property);
                    }
                    return true;

                default:
                    return base.DoOverridePropertyGUI(path, property, label);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="NamedAnimancerComponent.PlayAutomatically"/> and
        /// <see cref="NamedAnimancerComponent.Animations"/> fields are only used on startup, so we don't need to show
        /// them in Play Mode after the object is already enabled.
        /// </summary>
        private bool ShouldShowAnimationFields()
        {
            if (!EditorApplication.isPlaying)
                return true;

            for (int i = 0; i < Targets.Length; i++)
            {
                if (!Targets[i].IsPlayableInitialised)
                    return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        private void DoDefaultAnimationField(SerializedProperty playAutomatically)
        {
            var area = AnimancerGUI.LayoutSingleLineRect();

            var playAutomaticallyWidth = EditorGUIUtility.labelWidth + AnimancerGUI.ToggleWidth;
            var playAutomaticallyArea = AnimancerGUI.StealFromLeft(ref area, playAutomaticallyWidth);
            var label = AnimancerGUI.TempContent(playAutomatically);
            EditorGUI.PropertyField(playAutomaticallyArea, playAutomatically, label);

            SerializedProperty firstElement;
            AnimationClip clip;

            var animations = serializedObject.FindProperty("_Animations");
            if (animations.arraySize > 0)
            {
                firstElement = animations.GetArrayElementAtIndex(0);
                clip = (AnimationClip)firstElement.objectReferenceValue;
                EditorGUI.BeginProperty(area, null, firstElement);
            }
            else
            {
                firstElement = null;
                clip = null;
                EditorGUI.BeginProperty(area, null, animations);
            }

            EditorGUI.BeginChangeCheck();

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            clip = (AnimationClip)EditorGUI.ObjectField(area, GUIContent.none, clip, typeof(AnimationClip), true);

            EditorGUI.indentLevel = indentLevel;

            if (EditorGUI.EndChangeCheck())
            {
                if (clip != null)
                {
                    if (firstElement == null)
                    {
                        animations.arraySize = 1;
                        firstElement = animations.GetArrayElementAtIndex(0);
                    }

                    firstElement.objectReferenceValue = clip;
                }
                else
                {
                    if (firstElement == null || animations.arraySize == 1)
                        animations.arraySize = 0;
                    else
                        firstElement.objectReferenceValue = clip;
                }
            }

            EditorGUI.EndProperty();
        }

        /************************************************************************************************************************/

        private ReorderableList _Animations;
        private static int _RemoveAnimationIndex;

        private void DoAnimationsField(SerializedProperty property)
        {
            GUILayout.Space(AnimancerGUI.StandardSpacing - 1);

            if (_Animations == null)
            {
                _Animations = new ReorderableList(property.serializedObject, property.Copy())
                {
                    drawHeaderCallback = DrawAnimationsHeader,
                    drawElementCallback = DrawAnimationElement,
                    elementHeight = AnimancerGUI.LineHeight,
                    onRemoveCallback = RemoveSelectedElement,
                };
            }

            _RemoveAnimationIndex = -1;

            GUILayout.BeginVertical();
            _Animations.DoLayoutList();
            GUILayout.EndVertical();

            if (_RemoveAnimationIndex >= 0)
            {
                property.DeleteArrayElementAtIndex(_RemoveAnimationIndex);
            }

            AnimancerGUI.HandleDragAndDropAnimations(GUILayoutUtility.GetLastRect(), (clip) =>
            {
                var index = property.arraySize;
                property.arraySize = index + 1;
                var element = property.GetArrayElementAtIndex(index);
                element.objectReferenceValue = clip;
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        /************************************************************************************************************************/

        private SerializedProperty _AnimationsArraySize;

        private void DrawAnimationsHeader(Rect area)
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth -= 6;

            area.width += 5;

            var property = _Animations.serializedProperty;
            var label = EditorGUI.BeginProperty(area, AnimancerGUI.TempContent(property), property);

            if (_AnimationsArraySize == null)
            {
                _AnimationsArraySize = property.Copy();
                _AnimationsArraySize.Next(true);
                _AnimationsArraySize.Next(true);
            }

            EditorGUI.PropertyField(area, _AnimationsArraySize, label);

            EditorGUI.EndProperty();

            EditorGUIUtility.labelWidth = labelWidth;
        }

        /************************************************************************************************************************/

        private static readonly HashSet<Object>
            PreviousAnimations = new HashSet<Object>();

        private void DrawAnimationElement(Rect area, int index, bool isActive, bool isFocused)
        {
            if (index == 0)
                PreviousAnimations.Clear();

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth -= 20;

            var element = _Animations.serializedProperty.GetArrayElementAtIndex(index);

            var color = GUI.color;
            var animation = element.objectReferenceValue;
            if (animation == null || PreviousAnimations.Contains(animation))
                GUI.color = AnimancerGUI.WarningFieldColor;
            else
                PreviousAnimations.Add(animation);

            EditorGUI.BeginChangeCheck();
            EditorGUI.ObjectField(area, element, GUIContent.none);

            if (EditorGUI.EndChangeCheck() && element.objectReferenceValue == null)
                _RemoveAnimationIndex = index;

            GUI.color = color;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        /************************************************************************************************************************/

        private static void RemoveSelectedElement(ReorderableList list)
        {
            var property = list.serializedProperty;
            var element = property.GetArrayElementAtIndex(list.index);

            // Deleting a non-null element sets it to null, so we make sure it's null to actually remove it.
            if (element.objectReferenceValue != null)
                element.objectReferenceValue = null;

            property.DeleteArrayElementAtIndex(list.index);

            if (list.index >= property.arraySize - 1)
                list.index = property.arraySize - 1;
        }

        /************************************************************************************************************************/
    }
}

#endif

