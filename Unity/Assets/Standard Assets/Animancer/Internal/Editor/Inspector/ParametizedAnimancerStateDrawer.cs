// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Draws the Inspector GUI for an <see cref="AnimancerState"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/ParametizedAnimancerStateDrawer_1
    /// 
    public abstract class ParametizedAnimancerStateDrawer<T> : AnimancerStateDrawer<T> where T : AnimancerState
    {
        /************************************************************************************************************************/

        /// <summary>The number of parameters being managed by the target state.</summary>
        public virtual int ParameterCount => 0;

        /// <summary>Returns the name of a parameter being managed by the target state.</summary>
        /// <exception cref="NotSupportedException">The target state doesn't manage any parameters.</exception>
        public virtual string GetParameterName(int index) => throw new NotSupportedException();

        /// <summary>Returns the type of a parameter being managed by the target state.</summary>
        /// <exception cref="NotSupportedException">The target state doesn't manage any parameters.</exception>
        public virtual AnimatorControllerParameterType GetParameterType(int index) => throw new NotSupportedException();

        /// <summary>Returns the value of a parameter being managed by the target state.</summary>
        /// <exception cref="NotSupportedException">The target state doesn't manage any parameters.</exception>
        public virtual object GetParameterValue(int index) => throw new NotSupportedException();

        /// <summary>Sets the value of a parameter being managed by the target state.</summary>
        /// <exception cref="NotSupportedException">The target state doesn't manage any parameters.</exception>
        public virtual void SetParameterValue(int index, object value) => throw new NotSupportedException();

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="ParametizedAnimancerStateDrawer{T}"/> to manage the Inspector GUI for the `state`.
        /// </summary>
        protected ParametizedAnimancerStateDrawer(T state) : base(state) { }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void DoDetailsGUI()
        {
            base.DoDetailsGUI();

            if (!IsExpanded)
                return;

            var count = ParameterCount;
            if (count <= 0)
                return;

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth -= AnimancerGUI.IndentSize;

            for (int i = 0; i < count; i++)
            {
                var type = GetParameterType(i);
                if (type == 0)
                    continue;

                var name = GetParameterName(i);
                var value = GetParameterValue(i);

                EditorGUI.BeginChangeCheck();

                var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.Before);
                area = EditorGUI.IndentedRect(area);

                switch (type)
                {
                    case AnimatorControllerParameterType.Float:
                        value = EditorGUI.FloatField(area, name, (float)value);
                        break;

                    case AnimatorControllerParameterType.Int:
                        value = EditorGUI.IntField(area, name, (int)value);
                        break;

                    case AnimatorControllerParameterType.Bool:
                        value = EditorGUI.Toggle(area, name, (bool)value);
                        break;

                    case AnimatorControllerParameterType.Trigger:
                        value = EditorGUI.Toggle(area, name, (bool)value, EditorStyles.radioButton);
                        break;

                    default:
                        EditorGUI.LabelField(area, name, "Unsupported Type: " + type);
                        break;
                }

                if (EditorGUI.EndChangeCheck())
                    SetParameterValue(i, value);
            }

            EditorGUIUtility.labelWidth = labelWidth;
        }

        /************************************************************************************************************************/
    }
}

#endif

