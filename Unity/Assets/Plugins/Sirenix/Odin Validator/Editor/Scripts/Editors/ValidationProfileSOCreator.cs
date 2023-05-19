//-----------------------------------------------------------------------
// <copyright file="ValidationProfileSOCreator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class ValidationProfileSOCreator
    {
        public static void ShowDialog<T>(string defaultDestinationPath, Action<T> onScriptableObjectCreated = null)
            where T : ScriptableObject
        {
            var selector = new ValidationProfileSOSelector<T>(defaultDestinationPath, onScriptableObjectCreated);

            if (selector.SelectionTree.EnumerateTree().Count() == 1)
            {
                // If there is only one scriptable object to choose from in the selector, then 
                // we'll automatically select it and confirm the selection. 
                selector.SelectionTree.EnumerateTree().First().Select();
                selector.SelectionTree.Selection.ConfirmSelection();
            }
            else
            {
                // Else, we'll open up the selector in a popup and let the user choose.
                selector.ShowInPopup(200);
            }
        }

        private class ValidationProfileSOSelector<T> : OdinSelector<Type> where T : ScriptableObject
        {
            private Action<T> onScriptableObjectCreated;
            private string defaultDestinationPath;

            public ValidationProfileSOSelector(string defaultDestinationPath, Action<T> onScriptableObjectCreated = null)
            {
                this.onScriptableObjectCreated = onScriptableObjectCreated;
                this.defaultDestinationPath = defaultDestinationPath;
                this.SelectionConfirmed += this.ShowSaveFileDialog;
            }

            protected override void BuildSelectionTree(OdinMenuTree tree)
            {
                var scriptableObjectTypes = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes)
                    .Where(x => x.IsClass && !x.IsAbstract && x.InheritsFrom(typeof(T)));

                tree.Selection.SupportsMultiSelect = false;
                tree.Config.DrawSearchToolbar = true;
                tree.AddRange(scriptableObjectTypes, x => x.GetNiceName().SplitPascalCase())
                    .AddThumbnailIcons();
            }

            private void ShowSaveFileDialog(IEnumerable<Type> selection)
            {
                var obj = ScriptableObject.CreateInstance(selection.FirstOrDefault()) as T;

                string dest = this.defaultDestinationPath.TrimEnd('/');

                if (!Directory.Exists(dest))
                {
                    Directory.CreateDirectory(dest);
                    AssetDatabase.Refresh();
                }

                dest = EditorUtility.SaveFilePanel("Save object as", dest, "New " + obj.GetType().GetNiceName().SplitPascalCase(), "asset");

                if (!string.IsNullOrEmpty(dest) && PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), dest, out dest))
                {
                    AssetDatabase.CreateAsset(obj, dest);
                    AssetDatabase.Refresh();

                    if (this.onScriptableObjectCreated != null)
                    {
                        this.onScriptableObjectCreated(obj);
                        EditorGUIUtility.PingObject(obj);
                    }
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
        }
    }
}
