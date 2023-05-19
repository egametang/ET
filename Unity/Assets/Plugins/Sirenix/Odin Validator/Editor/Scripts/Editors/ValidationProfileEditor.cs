//-----------------------------------------------------------------------
// <copyright file="ValidationProfileEditor.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class ValidationProfileEditor
    {
        public bool ScanProfileImmediatelyWhenOpening;
        public IValidationProfile Profile;

        [Title("$targetTitle", "$targetSubtitle"), ShowIf("selectedSourceTarget", Animate = false), DisableContextMenu, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden), SuppressInvalidAttributeError, ShowInInspector, HideReferenceObjectPicker, DontValidate]
        private object selectedSourceTarget;

#pragma warning disable 0414 // Remove unread private members
        private string targetTitle;
        private string targetSubtitle;
#pragma warning restore 0414 // Remove unread private members

        private string GetTargetTitle(out string subtitle)
        {
            var result = "Validated Object";
            subtitle = null;

            var obj = this.selectedSourceTarget as UnityEngine.Object;

            if (obj != null)
            {
                result += ": " + obj.name;

                if (obj is Component)
                {
                    subtitle = obj.GetType().Name + " (Component)";
                }
                else if (obj is ScriptableObject)
                {
                    subtitle = obj.GetType().Name + " (ScriptableObject)";
                }
                else
                {
                    subtitle = obj.GetType().Name;
                }

                if (this.selectedSourceTarget is Component)
                {
                    var com = this.selectedSourceTarget as Component;

                    if (com.gameObject.scene.IsValid())
                    {
                        subtitle += " - Scene: '" + com.gameObject.scene.name + "'";
                    }
                }
            }

            return result;
        }

        public void SetTarget(object target)
        {
            this.selectedSourceTarget = target;
            this.targetTitle = this.GetTargetTitle(out this.targetSubtitle);
        }

        public ValidationProfileEditor(IValidationProfile profile)
        {
            this.Profile = profile;
        }

    }
}
