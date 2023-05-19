// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] A custom Inspector for <see cref="IAnimancerComponent"/>s.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/BaseAnimancerComponentEditor
    /// 
    public abstract class BaseAnimancerComponentEditor : UnityEditor.Editor
    {
        /************************************************************************************************************************/

        [NonSerialized]
        private IAnimancerComponent[] _Targets;
        /// <summary><see cref="UnityEditor.Editor.targets"/> casted to <see cref="IAnimancerComponent"/>.</summary>
        public IAnimancerComponent[] Targets => _Targets;

        /// <summary>The drawer for the <see cref="IAnimancerComponent.Playable"/>.</summary>
        private readonly AnimancerPlayableDrawer
            PlayableDrawer = new AnimancerPlayableDrawer();

        /************************************************************************************************************************/

        /// <summary>Initialises this <see cref="UnityEditor.Editor"/>.</summary>
        protected virtual void OnEnable()
        {
            var targets = this.targets;
            _Targets = new IAnimancerComponent[targets.Length];
            GatherTargets();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Copies the <see cref="UnityEditor.Editor.targets"/> into the <see cref="_Targets"/> array.
        /// </summary>
        private void GatherTargets()
        {
            for (int i = 0; i < _Targets.Length; i++)
                _Targets[i] = (IAnimancerComponent)targets[i];
        }

        /************************************************************************************************************************/

        /// <summary>Called by the Unity editor to draw the custom Inspector GUI elements.</summary>
        public override void OnInspectorGUI()
        {
            _LastRepaintTime = EditorApplication.timeSinceStartup;

            // Normally the targets wouldn't change after OnEnable, but the trick AnimancerComponent.Reset uses to
            // swap the type of an existing component when a new one is added causes the old target to be destroyed.
            GatherTargets();

            serializedObject.Update();

            var area = GUILayoutUtility.GetRect(0, 0);

            DoOtherFieldsGUI();
            PlayableDrawer.DoGUI(_Targets);

            area.yMax = GUILayoutUtility.GetLastRect().yMax;
            AnimancerLayerDrawer.HandleDragAndDropAnimations(area, _Targets[0], 0);

            serializedObject.ApplyModifiedProperties();
        }

        /************************************************************************************************************************/

        [NonSerialized]
        private double _LastRepaintTime = double.NegativeInfinity;

        /// <summary>
        /// If we have only one object selected and are in Play Mode, we need to constantly repaint to keep the
        /// Inspector up to date with the latest details.
        /// </summary>
        public override bool RequiresConstantRepaint()
        {
            if (_Targets.Length != 1)
                return false;

            var target = _Targets[0];
            if (!target.IsPlayableInitialised)
            {
                if (!EditorApplication.isPlaying ||
                    target.Animator == null ||
                    target.Animator.runtimeAnimatorController == null)
                    return false;
            }

            if (AnimancerPlayableDrawer.RepaintConstantly)
                return true;

            return EditorApplication.timeSinceStartup > _LastRepaintTime + AnimancerSettings.InspectorRepaintInterval;
        }

        /************************************************************************************************************************/

        /// <summary>Draws the rest of the Inspector fields after the Animator field.</summary>
        protected void DoOtherFieldsGUI()
        {
            var property = serializedObject.GetIterator();

            if (!property.NextVisible(true))
                return;

            do
            {
                using (ObjectPool.Disposable.Acquire<GUIContent>(out var label))
                {
                    var path = property.propertyPath;
                    if (path == "m_Script")
                        continue;

                    label.text = AnimancerGUI.GetNarrowText(property.displayName);
                    label.tooltip = property.tooltip;

                    // Let the target try to override.
                    if (DoOverridePropertyGUI(property.propertyPath, property, label))
                        continue;

                    // Otherwise draw the property normally.
                    EditorGUILayout.PropertyField(property, label, true);
                }
            }
            while (property.NextVisible(false));
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only]
        /// Draws any custom GUI for the `property`.
        /// The return value indicates whether the GUI should replace the regular call to
        /// <see cref="EditorGUILayout.PropertyField(SerializedProperty, GUIContent, bool, GUILayoutOption[])"/> or
        /// not. True = GUI was drawn, so don't draw the regular GUI. False = Draw the regular GUI.
        /// </summary>
        protected virtual bool DoOverridePropertyGUI(string path, SerializedProperty property, GUIContent label) => false;

        /************************************************************************************************************************/
    }
}

#endif

