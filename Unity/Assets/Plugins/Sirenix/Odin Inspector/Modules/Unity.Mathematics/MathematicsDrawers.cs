//-----------------------------------------------------------------------
// <copyright file="MathematicsDrawers.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Modules.UnityMathematics.Editor
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using Unity.Mathematics;
    using UnityEditor;
    using UnityEngine;

    public sealed class MatrixFloat2x2Processor : MatrixProcessor<float2x2> { }
    public sealed class MatrixFloat3x2Processor : MatrixProcessor<float3x2> { }
    public sealed class MatrixFloat4x2Processor : MatrixProcessor<float4x2> { }
    public sealed class MatrixFloat2x3Processor : MatrixProcessor<float2x3> { }
    public sealed class MatrixFloat3x3Processor : MatrixProcessor<float3x3> { }
    public sealed class MatrixFloat4x3Processor : MatrixProcessor<float4x3> { }
    public sealed class MatrixFloat2x4Processor : MatrixProcessor<float2x4> { }
    public sealed class MatrixFloat3x4Processor : MatrixProcessor<float3x4> { }
    public sealed class MatrixFloat4x4Processor : MatrixProcessor<float4x4> { }

    public sealed class MatrixDouble2x2Processor : MatrixProcessor<double2x2> { }
    public sealed class MatrixDouble3x2Processor : MatrixProcessor<double3x2> { }
    public sealed class MatrixDouble4x2Processor : MatrixProcessor<double4x2> { }
    public sealed class MatrixDouble2x3Processor : MatrixProcessor<double2x3> { }
    public sealed class MatrixDouble3x3Processor : MatrixProcessor<double3x3> { }
    public sealed class MatrixDouble4x3Processor : MatrixProcessor<double4x3> { }
    public sealed class MatrixDouble2x4Processor : MatrixProcessor<double2x4> { }
    public sealed class MatrixDouble3x4Processor : MatrixProcessor<double3x4> { }
    public sealed class MatrixDouble4x4Processor : MatrixProcessor<double4x4> { }

    public sealed class MatrixBool2x2Processor : MatrixProcessor<bool2x2> { }
    public sealed class MatrixBool3x2Processor : MatrixProcessor<bool3x2> { }
    public sealed class MatrixBool4x2Processor : MatrixProcessor<bool4x2> { }
    public sealed class MatrixBool2x3Processor : MatrixProcessor<bool2x3> { }
    public sealed class MatrixBool3x3Processor : MatrixProcessor<bool3x3> { }
    public sealed class MatrixBool4x3Processor : MatrixProcessor<bool4x3> { }
    public sealed class MatrixBool2x4Processor : MatrixProcessor<bool2x4> { }
    public sealed class MatrixBool3x4Processor : MatrixProcessor<bool3x4> { }
    public sealed class MatrixBool4x4Processor : MatrixProcessor<bool4x4> { }

    public sealed class MatrixInt2x2Processor : MatrixProcessor<int2x2> { }
    public sealed class MatrixInt3x2Processor : MatrixProcessor<int3x2> { }
    public sealed class MatrixInt4x2Processor : MatrixProcessor<int4x2> { }
    public sealed class MatrixInt2x3Processor : MatrixProcessor<int2x3> { }
    public sealed class MatrixInt3x3Processor : MatrixProcessor<int3x3> { }
    public sealed class MatrixInt4x3Processor : MatrixProcessor<int4x3> { }
    public sealed class MatrixInt2x4Processor : MatrixProcessor<int2x4> { }
    public sealed class MatrixInt3x4Processor : MatrixProcessor<int3x4> { }
    public sealed class MatrixInt4x4Processor : MatrixProcessor<int4x4> { }

    public sealed class MatrixUInt2x2Processor : MatrixProcessor<uint2x2> { }
    public sealed class MatrixUInt3x2Processor : MatrixProcessor<uint3x2> { }
    public sealed class MatrixUInt4x2Processor : MatrixProcessor<uint4x2> { }
    public sealed class MatrixUInt2x3Processor : MatrixProcessor<uint2x3> { }
    public sealed class MatrixUInt3x3Processor : MatrixProcessor<uint3x3> { }
    public sealed class MatrixUInt4x3Processor : MatrixProcessor<uint4x3> { }
    public sealed class MatrixUInt2x4Processor : MatrixProcessor<uint2x4> { }
    public sealed class MatrixUInt3x4Processor : MatrixProcessor<uint3x4> { }
    public sealed class MatrixUInt4x4Processor : MatrixProcessor<uint4x4> { }

    public sealed class DisableUnityMatrixDrawerAttribute : Attribute { }

    public abstract class MatrixProcessor<T> : OdinAttributeProcessor<T>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.GetOrAddAttribute<InlinePropertyAttribute>();
            attributes.GetOrAddAttribute<DisableUnityMatrixDrawerAttribute>();
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new HideLabelAttribute());
            attributes.Add(new MatrixChildAttribute());
        }
    }

    public class DisableUnityMatrixDrawerAttributeDrawer : OdinAttributeDrawer<DisableUnityMatrixDrawerAttribute>
    {
        protected override void Initialize()
        {
            this.SkipWhenDrawing = true;
            var chain = this.Property.GetActiveDrawerChain().BakedDrawerArray;

            for (int i = 0; i < chain.Length; i++)
            {
                var type = chain[i].GetType();

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UnityPropertyDrawer<,>) && type.GetGenericArguments()[0].Name == "MatrixDrawer")
                {
                    chain[i].SkipWhenDrawing = true;
                    break;
                }
            }
        }
    }

    public class MatrixChildAttribute : Attribute { }

    public class Bool2Drawer : OdinValueDrawer<bool2>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 100;

                if (label != null)
                {
                    GUILayout.Space(3); // Ugh, better than nothing
                }

                var options = GUILayoutOptions.Height(EditorGUIUtility.singleLineHeight);

                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                EditorGUILayout.EndVertical();
                GUIHelper.PopLabelWidth();
            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public class Bool3Drawer : OdinValueDrawer<bool3>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 100;

                if (label != null)
                {
                    GUILayout.Space(3); // Ugh, better than nothing
                }

                var options = GUILayoutOptions.Height(EditorGUIUtility.singleLineHeight);

                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                EditorGUILayout.EndVertical();
                GUIHelper.PopLabelWidth();
            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public class Bool4Drawer : OdinValueDrawer<bool4>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 100;

                if (label != null)
                {
                    GUILayout.Space(3); // Ugh, better than nothing
                }

                var options = GUILayoutOptions.Height(EditorGUIUtility.singleLineHeight);

                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(options);
                this.ValueEntry.Property.Children[3].Draw(showLabels ? GUIHelper.TempContent("W") : null);
                EditorGUILayout.EndVertical();
                GUIHelper.PopLabelWidth();
            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public class Float2Drawer : OdinValueDrawer<float2>, IDefinesGenericMenuItems
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                // Slide rect
                {
                    var val = this.ValueEntry.SmartValue;
                    EditorGUI.BeginChangeCheck();
                    var vec = SirenixEditorFields.VectorPrefixSlideRect(labelRect, new Vector2(val.x, val.y));
                    val = new float2(vec.x, vec.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.ValueEntry.SmartValue = val;
                    }
                }

                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            float2 value = (float2)property.ValueEntry.WeakSmartValue;
            var vec = new Vector2(value.x, value.y);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(vec.magnitude, 1f), () => NormalizeEntries(property));
            genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0)"), vec == Vector2.zero, () => SetVector(property, Vector2.zero));
            genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1)"), vec == Vector2.one, () => SetVector(property, Vector2.one));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0)"), vec == Vector2.right, () => SetVector(property, Vector2.right));
            genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0)"), vec == Vector2.left, () => SetVector(property, Vector2.left));
            genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1)"), vec == Vector2.up, () => SetVector(property, Vector2.up));
            genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1)"), vec == Vector2.down, () => SetVector(property, Vector2.down));
        }

        private void SetVector(InspectorProperty property, Vector2 value)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = new float2(value.x, value.y);
                }
            });
        }

        private void NormalizeEntries(InspectorProperty property)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = math.normalizesafe((float2)property.ValueEntry.WeakValues[i]);
                }
            });
        }
    }

    public class Float3Drawer : OdinValueDrawer<float3>, IDefinesGenericMenuItems
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                // Slide rect
                {
                    var val = this.ValueEntry.SmartValue;
                    EditorGUI.BeginChangeCheck();
                    var vec = SirenixEditorFields.VectorPrefixSlideRect(labelRect, new Vector3(val.x, val.y, val.z));
                    val = new float3(vec.x, vec.y, vec.z);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.ValueEntry.SmartValue = val;
                    }
                }

                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            float3 value = (float3)property.ValueEntry.WeakSmartValue;
            var vec = new Vector3(value.x, value.y, value.z);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(vec.magnitude, 1f), () => NormalizeEntries(property));
            genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0)"), vec == Vector3.zero, () => SetVector(property, Vector3.zero));
            genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1)"), vec == Vector3.one, () => SetVector(property, Vector3.one));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0)"), vec == Vector3.right, () => SetVector(property, Vector3.right));
            genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0)"), vec == Vector3.left, () => SetVector(property, Vector3.left));
            genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0)"), vec == Vector3.up, () => SetVector(property, Vector3.up));
            genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0)"), vec == Vector3.down, () => SetVector(property, Vector3.down));
            genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1)"), vec == Vector3.forward, () => SetVector(property, Vector3.forward));
            genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1)"), vec == Vector3.back, () => SetVector(property, Vector3.back));
        }

        private void SetVector(InspectorProperty property, Vector3 value)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = new float3(value.x, value.y, value.z);
                }
            });
        }

        private void NormalizeEntries(InspectorProperty property)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = math.normalizesafe((float3)property.ValueEntry.WeakValues[i]);
                }
            });
        }
    }

    public class Float4Drawer : OdinValueDrawer<float4>, IDefinesGenericMenuItems
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                // Slide rect
                {
                    var val = this.ValueEntry.SmartValue;
                    EditorGUI.BeginChangeCheck();
                    var vec = SirenixEditorFields.VectorPrefixSlideRect(labelRect, new Vector4(val.x, val.y, val.z, val.w));
                    val = new float4(vec.x, vec.y, vec.z, vec.w);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.ValueEntry.SmartValue = val;
                    }
                }

                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                this.ValueEntry.Property.Children[3].Draw(showLabels ? GUIHelper.TempContent("W") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            float4 value = (float4)property.ValueEntry.WeakSmartValue;
            var vec = new Vector4(value.x, value.y, value.z, value.w);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(vec.magnitude, 1f), () => NormalizeEntries(property));
            genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0, 0)"), vec == Vector4.zero, () => SetVector(property, Vector3.zero));
            genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1, 1)"), vec == Vector4.one, () => SetVector(property, Vector4.one));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0, 0)"), (Vector3)vec == Vector3.right, () => SetVector(property, Vector3.right));
            genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0, 0)"), (Vector3)vec == Vector3.left, () => SetVector(property, Vector3.left));
            genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0, 0)"), (Vector3)vec == Vector3.up, () => SetVector(property, Vector3.up));
            genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0, 0)"), (Vector3)vec == Vector3.down, () => SetVector(property, Vector3.down));
            genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1, 0)"), (Vector3)vec == Vector3.forward, () => SetVector(property, Vector3.forward));
            genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1, 0)"), (Vector3)vec == Vector3.back, () => SetVector(property, Vector3.back));
        }

        private void SetVector(InspectorProperty property, Vector4 value)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = new float4(value.x, value.y, value.z, value.w);
                }
            });
        }

        private void NormalizeEntries(InspectorProperty property)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = math.normalizesafe((float4)property.ValueEntry.WeakValues[i]);
                }
            });
        }
    }


    public class Double2Drawer : OdinValueDrawer<double2>, IDefinesGenericMenuItems
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                // Slide rect
                {
                    var val = this.ValueEntry.SmartValue;
                    EditorGUI.BeginChangeCheck();
                    var vec = SirenixEditorFields.VectorPrefixSlideRect(labelRect, new Vector2((float)val.x, (float)val.y));
                    val = new double2(vec.x, vec.y);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.ValueEntry.SmartValue = val;
                    }
                }

                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            double2 value = (double2)property.ValueEntry.WeakSmartValue;
            var vec = new Vector2((float)value.x, (float)value.y);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(vec.magnitude, 1f), () => NormalizeEntries(property));
            genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0)"), vec == Vector2.zero, () => SetVector(property, Vector2.zero));
            genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1)"), vec == Vector2.one, () => SetVector(property, Vector2.one));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0)"), vec == Vector2.right, () => SetVector(property, Vector2.right));
            genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0)"), vec == Vector2.left, () => SetVector(property, Vector2.left));
            genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1)"), vec == Vector2.up, () => SetVector(property, Vector2.up));
            genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1)"), vec == Vector2.down, () => SetVector(property, Vector2.down));
        }

        private void SetVector(InspectorProperty property, Vector2 value)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = new double2(value.x, value.y);
                }
            });
        }

        private void NormalizeEntries(InspectorProperty property)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = math.normalizesafe((double2)property.ValueEntry.WeakValues[i]);
                }
            });
        }
    }

    public class Double3Drawer : OdinValueDrawer<double3>, IDefinesGenericMenuItems
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                // Slide rect
                {
                    var val = this.ValueEntry.SmartValue;
                    EditorGUI.BeginChangeCheck();
                    var vec = SirenixEditorFields.VectorPrefixSlideRect(labelRect, new Vector3((float)val.x, (float)val.y, (float)val.z));
                    val = new double3(vec.x, vec.y, vec.z);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.ValueEntry.SmartValue = val;
                    }
                }

                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            double3 value = (double3)property.ValueEntry.WeakSmartValue;
            var vec = new Vector3((float)value.x, (float)value.y, (float)value.z);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(vec.magnitude, 1f), () => NormalizeEntries(property));
            genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0)"), vec == Vector3.zero, () => SetVector(property, Vector3.zero));
            genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1)"), vec == Vector3.one, () => SetVector(property, Vector3.one));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0)"), vec == Vector3.right, () => SetVector(property, Vector3.right));
            genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0)"), vec == Vector3.left, () => SetVector(property, Vector3.left));
            genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0)"), vec == Vector3.up, () => SetVector(property, Vector3.up));
            genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0)"), vec == Vector3.down, () => SetVector(property, Vector3.down));
            genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1)"), vec == Vector3.forward, () => SetVector(property, Vector3.forward));
            genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1)"), vec == Vector3.back, () => SetVector(property, Vector3.back));
        }

        private void SetVector(InspectorProperty property, Vector3 value)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = new double3(value.x, value.y, value.z);
                }
            });
        }

        private void NormalizeEntries(InspectorProperty property)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = math.normalizesafe((double3)property.ValueEntry.WeakValues[i]);
                }
            });
        }
    }

    public class Double4Drawer : OdinValueDrawer<double4>, IDefinesGenericMenuItems
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                // Slide rect
                {
                    var val = this.ValueEntry.SmartValue;
                    EditorGUI.BeginChangeCheck();
                    var vec = SirenixEditorFields.VectorPrefixSlideRect(labelRect, new Vector4((float)val.x, (float)val.y, (float)val.z, (float)val.w));
                    val = new double4(vec.x, vec.y, vec.z, vec.w);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.ValueEntry.SmartValue = val;
                    }
                }

                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                this.ValueEntry.Property.Children[3].Draw(showLabels ? GUIHelper.TempContent("W") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            double4 value = (double4)property.ValueEntry.WeakSmartValue;
            var vec = new Vector4((float)value.x, (float)value.y, (float)value.z, (float)value.w);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(vec.magnitude, 1f), () => NormalizeEntries(property));
            genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0, 0)"), vec == Vector4.zero, () => SetVector(property, Vector3.zero));
            genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1, 1)"), vec == Vector4.one, () => SetVector(property, Vector4.one));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0, 0)"), (Vector3)vec == Vector3.right, () => SetVector(property, Vector3.right));
            genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0, 0)"), (Vector3)vec == Vector3.left, () => SetVector(property, Vector3.left));
            genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0, 0)"), (Vector3)vec == Vector3.up, () => SetVector(property, Vector3.up));
            genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0, 0)"), (Vector3)vec == Vector3.down, () => SetVector(property, Vector3.down));
            genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1, 0)"), (Vector3)vec == Vector3.forward, () => SetVector(property, Vector3.forward));
            genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1, 0)"), (Vector3)vec == Vector3.back, () => SetVector(property, Vector3.back));
        }

        private void SetVector(InspectorProperty property, Vector4 value)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = new double4(value.x, value.y, value.z, value.w);
                }
            });
        }

        private void NormalizeEntries(InspectorProperty property)
        {
            property.Tree.DelayActionUntilRepaint(() =>
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    property.ValueEntry.WeakValues[i] = math.normalizesafe((double4)property.ValueEntry.WeakValues[i]);
                }
            });
        }
    }

    public class Int2Drawer : OdinValueDrawer<int2>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public class Int3Drawer : OdinValueDrawer<int3>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public class Int4Drawer : OdinValueDrawer<int4>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                this.ValueEntry.Property.Children[3].Draw(showLabels ? GUIHelper.TempContent("W") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public class UInt2Drawer : OdinValueDrawer<uint2>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public class UInt3Drawer : OdinValueDrawer<uint3>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public class UInt4Drawer : OdinValueDrawer<uint4>
    {
        private bool isMatrixChild;

        protected override void Initialize()
        {
            this.isMatrixChild = this.Property.GetAttribute<MatrixChildAttribute>() != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            Rect contentRect = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
            {
                var showLabels = !this.isMatrixChild && SirenixEditorFields.ResponsiveVectorComponentFields && contentRect.width >= 185;
                GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
                this.ValueEntry.Property.Children[0].Draw(showLabels ? GUIHelper.TempContent("X") : null);
                this.ValueEntry.Property.Children[1].Draw(showLabels ? GUIHelper.TempContent("Y") : null);
                this.ValueEntry.Property.Children[2].Draw(showLabels ? GUIHelper.TempContent("Z") : null);
                this.ValueEntry.Property.Children[3].Draw(showLabels ? GUIHelper.TempContent("W") : null);
                GUIHelper.PopLabelWidth();

            }
            SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }
#endif
}