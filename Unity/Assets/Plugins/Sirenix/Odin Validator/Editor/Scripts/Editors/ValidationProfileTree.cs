//-----------------------------------------------------------------------
// <copyright file="ValidationProfileTree.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class ValidationProfileTree : OdinMenuTree
    {
        private Dictionary<object, OdinMenuItem> childMenuItemLookup;
        public static Color red = new Color(0.787f, 0.133f, 0.133f, 1f);
        public static Color orange = new Color(0.934f, 0.66f, 0.172f, 1f);
        public static Color green = new Color(0, 0.5f, 0, 1f);

        public OdinMenuItem GetMenuItemForObject(object obj)
        {
            OdinMenuItem result;
            this.childMenuItemLookup.TryGetValue(obj, out result);
            return result;
        }

        public ValidationProfileTree()
        {
            this.Config.DrawSearchToolbar = true;
            this.Config.AutoHandleKeyboardNavigation = true;
            this.Config.UseCachedExpandedStates = false;

            this.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
            this.childMenuItemLookup = new Dictionary<object, OdinMenuItem>();

            this.Selection.SelectionConfirmed += (x) =>
            {
                var sel = x.FirstOrDefault();
                if (sel != null && sel.Value is ValidationProfileResult)
                {
                    var result = sel.Value as ValidationProfileResult;
                    if (result != null)
                    {
                        var source = result.GetSource() as UnityEngine.Object;
                        GUIHelper.SelectObject(source);
                    }
                }
            };
        }

        public void AddProfileRecursive(IValidationProfile profile, OdinMenuItem menuItem = null)
        {
            menuItem = menuItem ?? this.RootMenuItem;

            var newMenuItem = new OdinMenuItem(this, profile.Name, profile)
            {
                Icon = profile.GetProfileIcon()
            };

            this.childMenuItemLookup[profile] = newMenuItem;

            if (profile is ValidationProfileAsset)
            {
                var wrappedProfile = (profile as ValidationProfileAsset).GetWrappedProfile();
                this.childMenuItemLookup[wrappedProfile] = newMenuItem;
            }

            menuItem.ChildMenuItems.Add(newMenuItem);

            foreach (var childProfile in profile.GetNestedValidationProfiles())
            {
                this.AddProfileRecursive(childProfile, newMenuItem);
            }

            if (menuItem == this.RootMenuItem)
            {
                this.EnumerateTree().ForEach(x => x.Toggled = true);
                this.UpdateMenuTree();
            }
        }

        public void BuildTree(object obj)
        {
            var selected = this.Selection.FirstOrDefault();
            if (selected != null && selected.Value != obj)
            {
                var menuItem = this.EnumerateTree().FirstOrDefault(n => n.Value == obj);
                if (menuItem != null)
                {
                    menuItem.Select();
                }
            }
        }

        public void Draw()
        {
            this.DrawMenuTree();
        }

        public void AddResultToTree(ValidationProfileResult result)
        {
            if (result.Results == null)
            {
                return;
            }

            if (result.Results.Any(x => x.ResultType != ValidationResultType.Valid))
            {
                var menuItem = new OdinMenuItem(this, result.Name, result);

                var scene = default(Scene);
                if (result.Source as UnityEngine.Object)
                {
                    var component = result.Source as Component;
                    var go = result.Source as GameObject;
                    if (component)
                    {
                        go = component.gameObject;
                    }

                    if (go)
                    {
                        scene = go.scene;
                    }
                }

                this.childMenuItemLookup[result] = menuItem;

                if (result.Profile != null && scene.IsValid() && !this.childMenuItemLookup.ContainsKey(scene.path) && this.childMenuItemLookup.ContainsKey(result.Profile))
                {
                    var sceneItem = new OdinMenuItem(this, scene.name, scene.path);
                    sceneItem.IconGetter = () => EditorIcons.UnityLogo;
                    sceneItem.Toggled = true;
                    this.childMenuItemLookup.Add(scene.path, sceneItem);
                    this.childMenuItemLookup[result.Profile].ChildMenuItems.Add(sceneItem);
                }

                if (scene.IsValid() && this.childMenuItemLookup.ContainsKey(scene.path))
                {
                    this.childMenuItemLookup[scene.path].ChildMenuItems.Add(menuItem);
                }
                else if (result.Profile == null || !this.childMenuItemLookup.ContainsKey(result.Profile))
                {
                    this.MenuItems.Add(menuItem);
                }
                else
                {
                    this.childMenuItemLookup[result.Profile].ChildMenuItems.Add(menuItem);
                }

                if (result.Source != null)
                {
                    var component = result.Source as UnityEngine.Component;
                    if (component)
                    {
                        menuItem.Icon = GUIHelper.GetAssetThumbnail(component.gameObject, null, false);
                    }
                    else
                    {
                        menuItem.Icon = GUIHelper.GetAssetThumbnail(result.Source as UnityEngine.Object, result.Source.GetType(), false);
                    }
                }
                else
                {
                    menuItem.Icon = EditorIcons.Transparent.Active;
                }
            }
        }

        public void CleanProfile(IValidationProfile profile)
        {
            OdinMenuItem menuItem;
            if (this.childMenuItemLookup.TryGetValue(profile, out menuItem))
            {
                var allProfileMenuItems = menuItem.GetChildMenuItemsRecursive(true).Where(x => x.Value is IValidationProfile).ToList();

                foreach (var pi in allProfileMenuItems)
                {
                    pi.ChildMenuItems.RemoveAll(x => !(x.Value is IValidationProfile));
                }
            }

            this.MarkDirty();
            this.RebuildChildMenuItemLookup();
        }

        private void RebuildChildMenuItemLookup()
        {
            this.childMenuItemLookup.Clear();

            foreach (var item in this.EnumerateTree())
            {
                this.childMenuItemLookup[item.Value] = item;

                if (item.Value is ValidationProfileAsset)
                {
                    var wrappedProfile = (item.Value as ValidationProfileAsset).GetWrappedProfile();
                    this.childMenuItemLookup[wrappedProfile] = item;
                }
            }
        }

        public void AddErrorAndWarningIcons()
        {
            var errorCount = new Dictionary<OdinMenuItem, int>();
            var warningCount = new Dictionary<OdinMenuItem, int>();
            var maxECount = 0;
            var maxWCount = 0;

            foreach (var mi in this.EnumerateTree())
            {
                var result = mi.Value as ValidationProfileResult;
                if (result == null || result.Results == null || result.Results.Count == 0) continue;

                var ec = result.Results.Count(x => x.ResultType == ValidationResultType.Error);
                var wc = result.Results.Count(x => x.ResultType == ValidationResultType.Warning);

                foreach (var mm in mi.GetParentMenuItemsRecursive(true))
                {
                    if (!errorCount.ContainsKey(mm)) errorCount[mm] = 0;
                    if (!warningCount.ContainsKey(mm)) warningCount[mm] = 0;
                    maxECount = Math.Max(ec, errorCount[mm] += ec);
                    maxWCount = Math.Max(wc, warningCount[mm] += wc);
                }
            }

            var eCountWidth = SirenixGUIStyles.CenteredWhiteMiniLabel.CalcSize(new GUIContent(maxECount + " ")).x;
            var wCountWidth = SirenixGUIStyles.CenteredWhiteMiniLabel.CalcSize(new GUIContent(maxWCount + " ")).x;

            wCountWidth = eCountWidth = Mathf.Max(eCountWidth, wCountWidth);

            foreach (var mi in this.EnumerateTree())
            {
                if (!errorCount.ContainsKey(mi)) errorCount[mi] = 0;
                if (!warningCount.ContainsKey(mi)) warningCount[mi] = 0;

                var ec = errorCount[mi];
                var wc = warningCount[mi];
                var ecl = new GUIContent(ec + "");
                var wcl = new GUIContent(wc + "");
                var ncl = new GUIContent("0");
                //var result = mi.Value as ValidationProfileResult;

                mi.OnDrawItem = (m) =>
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        var rect = m.Rect.Padding(10, 5);
                        rect.height += 1;
                        var errorRect = rect.AlignRight(eCountWidth);
                        var warningRect = errorRect.SubX(wCountWidth - 1);
                        warningRect.width = wCountWidth;

                        var hasErrors = ec > 0;
                        if (hasErrors)
                        {
                            //red.a = result == null ? 0.2f : 1;
                            SirenixEditorGUI.DrawSolidRect(errorRect, red);
                            errorRect.y -= 1;
                            errorRect.x += 1;
                            GUI.Label(errorRect, ecl, SirenixGUIStyles.CenteredWhiteMiniLabel);
                        }

                        var hasWarnings = wc > 0;
                        if (hasWarnings)
                        {
                            //orange.a = result == null ? 0.2f : 1;
                            warningRect.x -= 2;
                            SirenixEditorGUI.DrawSolidRect(warningRect, orange);
                            warningRect.y -= 1;
                            warningRect.x += 1;
                            GUI.Label(warningRect, wcl, SirenixGUIStyles.CenteredWhiteMiniLabel);
                        }

                        if (!hasErrors && !hasWarnings)
                        {
                            SirenixEditorGUI.DrawSolidRect(errorRect, green);
                            errorRect.y -= 1;
                            errorRect.x += 1;
                            GUI.Label(errorRect, ncl, SirenixGUIStyles.CenteredWhiteMiniLabel);
                        }
                    }
                };
            }
        }
    }
}
