// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    partial class AnimancerToolsWindow
    {
        /// <summary>[Editor-Only] [Pro-Only] Base class for panels in the <see cref="AnimancerToolsWindow"/>.</summary>
        /// <remarks>
        /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools">Animancer Tools</see>
        /// </remarks>
        public abstract class Panel
        {
            /************************************************************************************************************************/

            private readonly AnimBool ExpansionAnimator = new AnimBool();

            private int _Index;

            /************************************************************************************************************************/

            /// <summary>Is the body of this panel currently visible?</summary>
            public bool IsExpanded
            {
                get { return Instance._CurrentPanel == _Index; }
                set
                {
                    if (value)
                        Instance._CurrentPanel = _Index;
                    else if (IsExpanded)
                        Instance._CurrentPanel = -1;
                }
            }

            /************************************************************************************************************************/

            /// <summary>The display name of this panel.</summary>
            public abstract string Name { get; }

            /// <summary>The usage instructions to display at the top of this panel.</summary>
            public abstract string Instructions { get; }

            /// <summary>The URL for the help button in the header to open.</summary>
            public virtual string HelpURL => Strings.DocsURLs.AnimancerTools;

            /// <summary>Called whenever the <see cref="Selection"/> changes.</summary>
            public virtual void OnSelectionChanged() { }

            /************************************************************************************************************************/

            /// <summary>Sets up the initial state of this panel.</summary>
            public virtual void OnEnable(int index)
            {
                _Index = index;
                ExpansionAnimator.value = ExpansionAnimator.target = IsExpanded;
            }

            /************************************************************************************************************************/

            /// <summary>Draws the GUI for this panel.</summary>
            public virtual void DoGUI()
            {
                var enabled = GUI.enabled;

                GUILayout.BeginVertical(EditorStyles.helpBox);

                DoHeaderGUI();

                ExpansionAnimator.target = IsExpanded;

                if (EditorGUILayout.BeginFadeGroup(ExpansionAnimator.faded))
                {
                    var instructions = Instructions;
                    if (!string.IsNullOrEmpty(instructions))
                        EditorGUILayout.HelpBox(instructions, MessageType.Info);

                    DoBodyGUI();
                }
                EditorGUILayout.EndFadeGroup();

                GUILayout.EndVertical();

                if (ExpansionAnimator.isAnimating)
                    Repaint();

                GUI.enabled = enabled;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Draws the Header GUI for this panel which is displayed regardless of whether it is expanded or not.
            /// </summary>
            public virtual void DoHeaderGUI()
            {
                var area = AnimancerGUI.LayoutSingleLineRect(AnimancerGUI.SpacingMode.BeforeAndAfter);
                var click = GUI.Button(area, Name, EditorStyles.boldLabel);

                area.xMin = area.xMax - area.height;
                GUI.DrawTexture(area, HelpIcon);

                if (click)
                {
                    if (area.Contains(Event.current.mousePosition))
                    {
                        Application.OpenURL(HelpURL);
                        return;
                    }
                    else
                    {
                        IsExpanded = !IsExpanded;
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>Draws the Body GUI for this panel which is only displayed while it is expanded.</summary>
            public abstract void DoBodyGUI();

            /************************************************************************************************************************/

            /// <summary>Asks the user where they want to save a modified asset, calls `modify` on it, and saves it.</summary>
            public static bool SaveModifiedAsset<T>(string saveTitle, string saveMessage,
                T obj, Action<T> modify) where T : Object
            {
                var originalPath = AssetDatabase.GetAssetPath(obj);

                var extension = Path.GetExtension(originalPath);
                if (extension[0] == '.')
                    extension = extension.Substring(1, extension.Length - 1);

                var directory = Path.GetDirectoryName(originalPath);

                var newName = Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(originalPath));
                var savePath = EditorUtility.SaveFilePanelInProject(saveTitle, newName, extension, saveMessage, directory);
                if (string.IsNullOrEmpty(savePath))
                    return false;

                if (originalPath != savePath)
                {
                    obj = Instantiate(obj);
                    AssetDatabase.CreateAsset(obj, savePath);
                }

                modify(obj);

                AssetDatabase.SaveAssets();

                return true;
            }

            /************************************************************************************************************************/

            private static Texture _HelpIcon;

            /// <summary>The help icon image used in the panel header.</summary>
            public static Texture HelpIcon
            {
                get
                {
                    if (_HelpIcon == null)
                        _HelpIcon = EditorGUIUtility.IconContent("_Help").image;
                    return _HelpIcon;
                }
            }

            /************************************************************************************************************************/
        }
    }
}

#endif

