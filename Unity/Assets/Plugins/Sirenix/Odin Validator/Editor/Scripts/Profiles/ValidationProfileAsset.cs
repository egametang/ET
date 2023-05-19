//-----------------------------------------------------------------------
// <copyright file="ValidationProfileAsset.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.Validation;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public abstract class ValidationProfileAsset : ScriptableObject, IValidationProfile
    {
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public abstract IEnumerable<IValidationProfile> GetNestedValidationProfiles();
        public abstract Texture GetProfileIcon();
        public abstract object GetSource(ValidationProfileResult entry);
        public abstract IEnumerable<ValidationProfileResult> Validate(ValidationRunner runner);

        public abstract IValidationProfile GetWrappedProfile();
    }

    public abstract class ValidationProfileAsset<T> : ValidationProfileAsset
        where T : IValidationProfile
    {
        [HideLabel]
        public T Profile;

        public override string Name
        {
            get { return this.Profile == null ? "" : this.Profile.Name; }
            set { if (this.Profile != null) this.Profile.Name = value; }
        }

        public override string Description
        {
            get { return this.Profile == null ? "" : this.Profile.Description; }
            set { if (this.Profile != null) this.Profile.Description = value; }
        }

        public override IEnumerable<IValidationProfile> GetNestedValidationProfiles()
        {
            if (this.Profile == null) yield break;

            foreach (var profile in this.Profile.GetNestedValidationProfiles())
                yield return profile;
        }

        public override Texture GetProfileIcon()
        {
            return this.Profile == null ? null : this.Profile.GetProfileIcon();
        }

        public override object GetSource(ValidationProfileResult entry)
        {
            return this.Profile == null ? null : this.Profile.GetSource(entry);
        }

        public override IEnumerable<ValidationProfileResult> Validate(ValidationRunner runner)
        {
            if (this.Profile == null) yield break;

            foreach (var result in this.Profile.Validate(runner))
                yield return result;
        }

        public override IValidationProfile GetWrappedProfile()
        {
            return this.Profile;
        }
    }
}