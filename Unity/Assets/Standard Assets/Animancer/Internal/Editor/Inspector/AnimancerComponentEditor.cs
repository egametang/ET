// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom Inspector for <see cref="AnimancerComponent"/>s.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/AnimancerComponentEditor
    /// 
    [CustomEditor(typeof(AnimancerComponent), true), CanEditMultipleObjects]
    public class AnimancerComponentEditor : BaseAnimancerComponentEditor
    {
        /************************************************************************************************************************/

        private bool _ShowResetOnDisableWarning;

        protected override bool DoOverridePropertyGUI(string path, SerializedProperty property, GUIContent label)
        {
            if (path == Targets[0].AnimatorFieldName)
            {
                DoAnimatorGUI(property, label);
                return true;
            }

            if (path == Targets[0].ActionOnDisableFieldName)
            {
                DoActionOnDisableGUI(property, label);
                return true;
            }

            return base.DoOverridePropertyGUI(path, property, label);
        }

        /************************************************************************************************************************/

        private void DoAnimatorGUI(SerializedProperty property, GUIContent label)
        {
            var hasAnimator = property.objectReferenceValue != null;

            var color = GUI.color;
            if (!hasAnimator)
                GUI.color = AnimancerGUI.WarningFieldColor;

            EditorGUILayout.PropertyField(property, label);

            if (!hasAnimator)
            {
                GUI.color = color;

                EditorGUILayout.HelpBox($"An {nameof(Animator)} is required in order to play animations." +
                    " Click here to search for one nearby.",
                    MessageType.Warning);

                if (AnimancerGUI.TryUseClickEventInLastRect())
                {
                    Serialization.ForEachTarget(property, (targetProperty) =>
                    {
                        var target = (IAnimancerComponent)targetProperty.serializedObject.targetObject;

                        var animator = AnimancerEditorUtilities.GetComponentInHierarchy<Animator>(target.gameObject);
                        if (animator == null)
                        {
                            Debug.Log($"No {nameof(Animator)} found on '{target.gameObject.name}' or any of its parents or children." +
                                " You must assign one manually.", target.gameObject);
                            return;
                        }

                        targetProperty.objectReferenceValue = animator;
                    });
                }
            }
            else if (property.objectReferenceValue is Animator animator)
            {
                if (animator.gameObject != Targets[0].gameObject)
                {
                    EditorGUILayout.HelpBox(
                        $"It is recommended that you keep this component on the same {nameof(GameObject)}" +
                        $" as its target {nameof(Animator)} so that they get enabled and disabled at the same time.",
                        MessageType.Info);
                }

                var initialUpdateMode = Targets[0].InitialUpdateMode;
                var updateMode = animator.updateMode;
                if (AnimancerPlayable.HasChangedToOrFromAnimatePhysics(initialUpdateMode, updateMode))
                {
                    EditorGUILayout.HelpBox(
                        $"Changing to or from {nameof(AnimatorUpdateMode.AnimatePhysics)} mode at runtime has no effect" +
                        $" when using the Playables API. It will continue using the original mode it had on startup.",
                        MessageType.Warning);

                    if (AnimancerGUI.TryUseClickEventInLastRect())
                        EditorUtility.OpenWithDefaultApp(Strings.DocsURLs.UpdateModes);
                }
            }
        }

        /************************************************************************************************************************/

        private void DoActionOnDisableGUI(SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property, label, true);

            if (property.enumValueIndex == (int)AnimancerComponent.DisableAction.Reset)
            {
                // Since getting all the components creates garbage, only do it during layout events.
                if (Event.current.type == EventType.Layout)
                {
                    _ShowResetOnDisableWarning = !AreAllResettingTargetsAboveTheirAnimator();
                }

                if (_ShowResetOnDisableWarning)
                {
                    EditorGUILayout.HelpBox("Reset only works if this component is above the Animator" +
                        " so OnDisable can perform the Reset before the Animator actually gets disabled." +
                        " Click here to fix." +
                        "\n\nOtherwise you can use Stop and call Animator.Rebind before disabling this GameObject.",
                        MessageType.Error);

                    if (AnimancerGUI.TryUseClickEventInLastRect())
                        MoveResettingTargetsAboveTheirAnimator();
                }
            }
        }

        /************************************************************************************************************************/

        private bool AreAllResettingTargetsAboveTheirAnimator()
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                var target = Targets[i];
                if (!target.ResetOnDisable)
                    continue;

                var animator = target.Animator;
                if (animator == null ||
                    target.gameObject != animator.gameObject)
                    continue;

                var targetObject = (Object)target;
                var components = target.gameObject.GetComponents<Component>();
                for (int j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (component == targetObject)
                        break;
                    else if (component == animator)
                        return false;
                }
            }

            return true;
        }

        /************************************************************************************************************************/

        private void MoveResettingTargetsAboveTheirAnimator()
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                var target = Targets[i];
                if (!target.ResetOnDisable)
                    continue;

                var animator = target.Animator;
                if (animator == null ||
                    target.gameObject != animator.gameObject)
                    continue;

                int animatorIndex = -1;

                var targetObject = (Object)target;
                var components = target.gameObject.GetComponents<Component>();
                for (int j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (component == targetObject)
                    {
                        if (animatorIndex >= 0)
                        {
                            var count = j - animatorIndex;
                            while (count-- > 0)
                                UnityEditorInternal.ComponentUtility.MoveComponentUp((Component)target);
                        }
                        break;
                    }
                    else if (component == animator)
                    {
                        animatorIndex = j;
                    }
                }
            }
        }

        /************************************************************************************************************************/
    }
}

#endif

