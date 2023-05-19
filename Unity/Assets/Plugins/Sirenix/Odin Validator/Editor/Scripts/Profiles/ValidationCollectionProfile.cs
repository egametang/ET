//-----------------------------------------------------------------------
// <copyright file="ValidationCollectionProfile.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector.Editor.Validation;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class ValidationCollectionProfile : ValidationProfile
    {
        public ValidationProfileAsset[] Profiles;

        public override IEnumerable<IValidationProfile> GetNestedValidationProfiles()
        {
            return this.Profiles;
        }

        public override IEnumerable<ValidationProfileResult> Validate(ValidationRunner runner)
        {
            var partialProgress = 0f;
            var partialProgressStepSize = 1f / this.Profiles.Length;
            for (int i = 0; i < this.Profiles.Length; i++)
            {
                IValidationProfile profile = this.Profiles[i];
                foreach (var result in profile.Validate(runner))
                {
                    result.Progress = result.Progress * partialProgressStepSize + partialProgress;
                    yield return result;
                }

                partialProgress += partialProgressStepSize;
            }
        }
    }
}