//-----------------------------------------------------------------------
// <copyright file="AutomatedValidation.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;

    [HideReferenceObjectPicker]
    public class AutomatedValidation
    {
        public Action Actions = Action.OpenValidatorIfError | Action.OpenValidatorIfWarning;

        [ListDrawerSettings(Expanded = true)]
        [AssetSelector(Filter = "t:ValidationProfileAsset", DrawDropdownForListElements = false)]
        public List<IValidationProfile> ProfilesToRun = new List<IValidationProfile>();

        public bool HasActionFlag(Action flag)
        {
            return (this.Actions & flag) == flag;
        }

        [Flags]
        public enum Action
        {
            OpenValidatorIfError = 1 << 0,
            OpenValidatorIfWarning = 1 << 1,
            StopHookEventOnError = 1 << 2,
            StopHookEventOnWarning = 1 << 3,
            LogError = 1 << 4,
            LogWarning = 1 << 5,
        }
    }
}
