// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    partial class AnimancerToolsWindow
    {
        /// <summary>[Editor-Only] [Pro-Only] 
        /// A <see cref="SpriteModifierPanel"/> for modifying <see cref="Sprite"/> detauls.
        /// </summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools/modify-sprites">Modify Sprites</see>
        /// </remarks>
        [Serializable]
        public sealed class ModifySprites : SpriteModifierPanel
        {
            /************************************************************************************************************************/

            [SerializeField] private OffsetRectMode _RectMode;
            [SerializeField] private Rect _RectOffset;

            [SerializeField] private bool _SetPivot;
            [SerializeField] private Vector2 _Pivot;

            [SerializeField] private bool _SetAlignment;
            [SerializeField] private SpriteAlignment _Alignment;

            [SerializeField] private bool _SetBorder;
            [SerializeField] private RectOffset _Border;

            [SerializeField] private bool _ShowDetails;

            /************************************************************************************************************************/

            private enum OffsetRectMode { None, Add, Subtract }
            private static readonly string[] OffsetRectModes = { "None", "Add", "Subtract" };

            private SerializedProperty _SerializedProperty;

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override string Name => "Modify Sprites";

            /// <inheritdoc/>
            public override string HelpURL => Strings.DocsURLs.ModifySprites;

            /// <inheritdoc/>
            public override string Instructions
            {
                get
                {
                    if (Sprites.Count == 0)
                        return "Select the Sprites you want to modify.";

                    if (!IsValidModification())
                        return "The current Rect Offset would move some Sprites outside the texture bounds.";

                    return "Enter the desired modifications and click Apply.";
                }
            }

            /************************************************************************************************************************/

            public override void OnEnable(int index)
            {
                base.OnEnable(index);

                _SerializedProperty = Instance.SerializedObject.FindProperty($"{nameof(_ModifySprites)}.{nameof(_RectMode)}");
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void DoBodyGUI()
            {
                var area = AnimancerGUI.LayoutSingleLineRect();
                area.xMin += 4;
                area = EditorGUI.PrefixLabel(area, AnimancerGUI.TempContent("Offset Rects", null, false));
                BeginChangeCheck();
                var selected = (OffsetRectMode)GUI.Toolbar(area, (int)_RectMode, OffsetRectModes);
                EndChangeCheck(ref _RectMode, selected);

                using (var property = _SerializedProperty.Copy())
                {
                    property.serializedObject.Update();

                    var depth = property.depth;
                    while (property.Next(false) && property.depth >= depth)
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }

                    property.serializedObject.ApplyModifiedProperties();
                }

                GUI.enabled = false;
                for (int i = 0; i < Sprites.Count; i++)
                {
                    if (_ShowDetails)
                        GUILayout.BeginVertical(GUI.skin.box);

                    var sprite = Sprites[i] = (Sprite)EditorGUILayout.ObjectField(Sprites[i], typeof(Sprite), false);

                    if (_ShowDetails)
                    {
                        if (_RectMode != OffsetRectMode.None)
                            EditorGUILayout.RectField("Rect", sprite.rect);

                        if (_SetPivot)
                            EditorGUILayout.Vector2Field("Pivot", sprite.pivot);

                        if (_SetBorder)
                            EditorGUILayout.Vector4Field("Border", sprite.border);

                        GUILayout.EndVertical();
                    }
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    GUI.enabled = Sprites.Count > 0 && IsValidModification();

                    if (GUILayout.Button("Apply"))
                    {
                        AnimancerGUI.Deselect();
                        AskAndApply();
                    }
                }
                GUILayout.EndHorizontal();
            }

            /************************************************************************************************************************/

            private bool IsValidModification()
            {
                switch (_RectMode)
                {
                    default:
                    case OffsetRectMode.None:
                        return true;

                    case OffsetRectMode.Add:
                    case OffsetRectMode.Subtract:
                        break;
                }

                var offset = GetOffset();

                var sprites = Sprites;
                for (int i = 0; i < sprites.Count; i++)
                {
                    var sprite = sprites[i];
                    var rect = Add(sprite.rect, offset);
                    if (rect.xMin < 0 ||
                        rect.yMin < 0 ||
                        rect.xMax >= sprite.texture.width ||
                        rect.xMax >= sprite.texture.height)
                    {
                        return false;
                    }
                }

                return true;
            }

            /************************************************************************************************************************/

            private Rect GetOffset()
            {
                switch (_RectMode)
                {
                    default:
                    case OffsetRectMode.None:
                        throw new InvalidOperationException($"Can't {nameof(GetOffset)} when the mode is {_RectMode}.");

                    case OffsetRectMode.Add:
                        return _RectOffset;

                    case OffsetRectMode.Subtract:
                        return new Rect(-_RectOffset.x, -_RectOffset.y, -_RectOffset.width, -_RectOffset.height);
                }
            }

            private static Rect Add(Rect a, Rect b)
            {
                a.x += b.x;
                a.y += b.y;
                a.width += b.width;
                a.height += b.height;
                return a;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override string AreYouSure => "Are you sure you want to modify the borders of these Sprites?";

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override void Modify(ref SpriteMetaData data, Sprite sprite)
            {
                switch (_RectMode)
                {
                    default:
                    case OffsetRectMode.None:
                        break;

                    case OffsetRectMode.Add:
                    case OffsetRectMode.Subtract:
                        data.rect = Add(data.rect, GetOffset());
                        break;
                }

                if (_SetPivot)
                    data.pivot = _Pivot;

                if (_SetAlignment)
                    data.alignment = (int)_Alignment;

                if (_SetBorder)
                    data.border = new Vector4(_Border.left, _Border.bottom, _Border.right, _Border.top);
            }

            /************************************************************************************************************************/
        }
    }
}

#endif

