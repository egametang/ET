//-----------------------------------------------------------------------
// <copyright file="ValidationProfileManagerOverview.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [DontApplyToListElements]
    public class CustomValidationHookListAttribute : Attribute
    {

    }

    internal class CustomValidationHookListAttributeDrawer : OdinAttributeDrawer<CustomValidationHookListAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                var val = this.Property.Children[i].ValueEntry.WeakSmartValue as AutomatedValidationHook;
                this.Property.Children[i].Draw(new GUIContent(val.Name));
            }
        }
    }

    public class AutomateValidationEditor
    {
        private Vector2 scrollPos;

        [CustomValidationHookList]
        [ShowInInspector, DisableContextMenu(true, true)]
        [OnValueChanged("SetDirty", includeChildren: true)]
        public List<AutomatedValidationHook> Hooks
        {
            get { return OdinValidationConfig.Instance.Hooks; }
            set { }
        }

        public void SetDirty()
        {
            EditorUtility.SetDirty(OdinValidationConfig.Instance);
        }

        [OnInspectorGUI, PropertyOrder(-200)]
        private void OnBeginGUI()
        {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, GUIStyle.none);
            GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(20, 20, 10, 10) });
            GUILayout.Label("Automate Validation", SirenixGUIStyles.SectionHeader);
            var rect = GUILayoutUtility.GetLastRect().AlignCenterY(20).AlignRight(120);

            if (GUI.Button(rect, new GUIContent("Edit Master Profiles"), SirenixGUIStyles.MiniButton))
            {
                GUIHelper.OpenInspectorWindow(OdinValidationConfig.Instance);
            }

            SirenixEditorGUI.DrawThickHorizontalSeparator(4, 10);
        }

        [OnInspectorGUI, PropertyOrder(200)]
        private void OnEndGUI()
        {
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }

    public class ValidationProfileManagerOverview
    {
        private SlidePageNavigationHelper<object> pager;
        private Vector2 scrollPos;

        public ValidationProfileManagerOverview(SlidePageNavigationHelper<object> pager)
        {
            this.pager = pager;
        }

        [OnInspectorGUI]
        public void Draw()
        {
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, GUIStyle.none);
            GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(20, 20, 10, 10) });
            GUILayout.Label("Validation Profiles", SirenixGUIStyles.SectionHeader);
            var rect = GUILayoutUtility.GetLastRect().AlignCenterY(20).AlignRight(120);

            if (GUI.Button(rect, new GUIContent("Manage Profiles"), SirenixGUIStyles.MiniButton))
            {
                GUIHelper.OpenInspectorWindow(OdinValidationConfig.Instance);
            }

            //rect.x -= rect.width + 5;
            //if (GUI.Button(rect, new GUIContent("Create New Profile"), SirenixGUIStyles.MiniButton))
            //{
            //    ValidationProfileSOCreator.ShowDialog<ValidationProfileAsset>("test");
            //}

            rect.x -= rect.width + 5;
            if (GUI.Button(rect, new GUIContent("Automate Validation"), SirenixGUIStyles.MiniButton))
            {
                this.pager.PushPage(new AutomateValidationEditor(), "Automate Validation");
            }

            SirenixEditorGUI.DrawThickHorizontalSeparator(4, 10);

            if (OdinValidationConfig.Instance.MainValidationProfiles != null)
            {
                foreach (var item in OdinValidationConfig.Instance.MainValidationProfiles)
                {
                    if (item == null) continue;

                    this.DrawCard(item);
                    GUILayout.Space(20);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawCard(IValidationProfile profile)
        {
            GUIHelper.RequestRepaint();
            GUILayout.BeginVertical();
            var isMouseIver = GUIHelper.CurrentWindowHasFocus && GUIHelper.GetCurrentLayoutRect().Contains(Event.current.mousePosition);
            GUIHelper.PushColor(new Color(1, 1, 1, (EditorGUIUtility.isProSkin ? 0.25f : 0.45f) * (isMouseIver ? 2 : 1)));
            GUILayout.BeginHorizontal(SirenixGUIStyles.CardStyle);
            GUIHelper.PopColor();
            {
                string profileName = profile.Name;
                string profileDescription = profile.Description;

                if (string.IsNullOrEmpty(profileName) || profileName.Trim().Length == 0)
                {
                    profileName = "(No Name)";
                }

                if (string.IsNullOrEmpty(profileDescription) || profileDescription.Trim().Length == 0)
                {
                    profileDescription = "(No Description)";
                }

                GUILayout.BeginVertical();
                GUILayout.Label(profileName, SirenixGUIStyles.BoldTitle);
                GUILayout.Label(profileDescription, SirenixGUIStyles.MultiLineLabel);
                GUILayout.EndVertical();
                GUILayout.Space(40);

                var rect = GUIHelper.GetCurrentLayoutRect();
                var btnsRect = rect.Padding(20).AlignRight(20).AlignCenterY(20);
                if (SirenixEditorGUI.IconButton(btnsRect, EditorIcons.Pen))
                {
                    GUIHelper.OpenInspectorWindow(profile as UnityEngine.Object);
                }

                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    this.pager.PushPage(new ValidationProfileManagerWindow.ValidationProfileEditorWrapper(new ValidationProfileEditor(profile)), profile.Name);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
