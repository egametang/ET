//-----------------------------------------------------------------------
// <copyright file="ValidationProfileManagerWindow.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class ValidationProfileManagerWindow : OdinEditorWindow
    {
        private SlidePageNavigationHelper<object> pager;

        [MenuItem("Tools/Odin Project Validator")]
        public static void OpenProjectValidator()
        {
            var window = Resources.FindObjectsOfTypeAll<ValidationProfileManagerWindow>().FirstOrDefault();
            if (window)
            {
                window.Focus();
            }
            else
            {
                window = GetWindow<ValidationProfileManagerWindow>();
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(670, 700);
                window.Show();
            }

            window.pager = new SlidePageNavigationHelper<object>();
            window.pager.PushPage(new ValidationProfileManagerOverview(window.pager), "Overview");
        }

        internal class ValidationProfileEditorWrapper
        {
#pragma warning disable 0414 // Remove unread private members
            [DisableContextMenu, ShowInInspector, HideReferenceObjectPicker]
            private ValidationProfileEditor validationProfileEditor;
#pragma warning restore 0414 // Remove unread private members

            public ValidationProfileEditorWrapper(ValidationProfileEditor validationProfileEditor)
            {
                this.validationProfileEditor = validationProfileEditor;
            }
        }

        public static void OpenProjectValidatorWithProfile(IValidationProfile profile, bool scanProfileImmediately = false)
        {
            var window = Resources.FindObjectsOfTypeAll<ValidationProfileManagerWindow>().FirstOrDefault();
            if (window)
            {
                window.Focus();
            }
            else
            {
                window = GetWindow<ValidationProfileManagerWindow>();
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(670, 700);
                window.Show();
            }

            window.pager = new SlidePageNavigationHelper<object>();
            window.pager.PushPage(new ValidationProfileManagerOverview(window.pager), "Overview");

            var editor = new ValidationProfileEditor(profile);
            editor.ScanProfileImmediatelyWhenOpening = scanProfileImmediately;
            window.pager.PushPage(new ValidationProfileEditorWrapper(editor), profile.Name);
        }

        protected override void Initialize()
        {
            this.WindowPadding = new Vector4(0, 0, 0, 0);
            if (this.pager == null)
            {
                this.pager = new SlidePageNavigationHelper<object>();
                this.pager.PushPage(new ValidationProfileManagerOverview(this.pager), "Overview");
            }
        }

        protected override void DrawEditors()
        {
            SirenixEditorGUI.DrawSolidRect(new Rect(0, 0, this.position.width, this.position.height), SirenixGUIStyles.DarkEditorBackground);

            // Draw top pager:
            var rect = GUIHelper.GetCurrentLayoutRect().AlignTop(34);
            SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.EditorWindowBackgroundColor);
            SirenixEditorGUI.DrawBorders(rect, 0, 0, 0, 1);
            this.pager.DrawPageNavigation(rect.AlignCenterY(20).HorizontalPadding(10));

            // Draw pages:
            this.pager.BeginGroup();
            var i = 0;
            foreach (var page in this.pager.EnumeratePages)
            {
                if (page.BeginPage())
                {
                    GUILayout.BeginVertical(GUILayoutOptions.ExpandHeight(true));
                    GUILayout.Space(30);
                    this.DrawEditor(i);
                    GUILayout.EndVertical();
                }
                page.EndPage();
                i++;
            }
            this.pager.EndGroup();
        }

        protected override IEnumerable<object> GetTargets()
        {
            return this.pager.EnumeratePages.Select(x => x.Value);
        }
    }
}
