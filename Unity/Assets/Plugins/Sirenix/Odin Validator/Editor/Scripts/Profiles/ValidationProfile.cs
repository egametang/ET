//-----------------------------------------------------------------------
// <copyright file="ValidationProfile.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public interface IValidationProfile
    {
        string Name { get; set; }
        string Description { get; set; }

        object GetSource(ValidationProfileResult entry);
        IEnumerable<IValidationProfile> GetNestedValidationProfiles();
        IEnumerable<ValidationProfileResult> Validate(ValidationRunner runner);
        Texture GetProfileIcon();
    }

    public abstract class ValidationProfile : IValidationProfile
    {
        [SerializeField]
        private string name;

        [SerializeField, TextArea(1, 5)]
        private string description;

        public string Name { get { return this.name; } set { this.name = value; } }

        public string Description { get { return this.description; } set { this.description = value; } }

        public virtual object GetSource(ValidationProfileResult entry)
        {
            return entry.Source;
        }

        public virtual IEnumerable<IValidationProfile> GetNestedValidationProfiles()
        {
            yield break;
        }

        public abstract IEnumerable<ValidationProfileResult> Validate(ValidationRunner runner);

        public virtual Texture GetProfileIcon()
        {
            return GUIHelper.GetAssetThumbnail(null, typeof(ScriptableObject), false);
        }
    }

    public class ValidationProfileResult
    {
        public IValidationProfile Profile;

        /// <summary>
        /// The progress percentage - a value between 0 and 1.
        /// </summary>
        public float Progress;
        public string Name;

        /// <summary>
        /// The object containing the following results. Please note, that this value can be null if the object was from a scene that is no longer loaded.
        /// You can retrieve the obejct by calling GetSource(), which will try and open the scene, and relocate the object.
        /// </summary>
        public object Source;
        public List<ValidationResult> Results;
        public object SourceRecoveryData;

        public string Path;

        /// <summary>
        /// In some cases the <see cref="Source"> member can be null. For instance, if the objcet was from a scene which is no longer loaded.
        /// Calling GetSource, will try and open the scene, then locate and return the object if found.
        /// </summary>
        public object GetSource()
        {
            if (this.Profile == null) return null;
            if (this.Results == null) return null;
            return this.Profile.GetSource(this);
        }
    }

    public class ValidationProfileAttributeProcessor<T> : OdinAttributeProcessor<T>
        where T : ValidationProfile
    {
        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            return true;
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            if (member.DeclaringType != typeof(ValidationProfile))
            {
                attributes.Add(new HideInTablesAttribute());
            }
        }
    }
}