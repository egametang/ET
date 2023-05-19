//-----------------------------------------------------------------------
// <copyright file="AutomatedValidationHook.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [Toggle("EnabledProp", CollapseOthersOnExpand = false)]
    [HideReferenceObjectPicker]
    public class AutomatedValidationHook
    {
        [ShowInInspector]
        private bool EnabledProp
        {
            get
            {
                return this.Enabled;
            }

            set
            {
                if (value != this.Enabled)
                {
                    this.Enabled = value;
                    this.OnEnabledChanged();
                }
            }
        }

        [HideInInspector]
        public bool Enabled;

        public bool FinishValidationOnFailures;

        [HideInInspector]
        public IValidationHook Hook;

        // If people ever want the ability to add multiple validations we can easily let them. But lets start out simple.
        //[ListDrawerSettings(Expanded = true, DraggableItems = false, CustomAddFunction = "CreateAutomatedValidation")]
        [HideInInspector]
        public List<AutomatedValidation> Validations = new List<AutomatedValidation>();

        [ShowInInspector]
        [HideLabel, InfoBox("Note that opening the validator window will stop the project from entering play mode", "ShowPlayModeWarning", InfoMessageType = InfoMessageType.Info)]
        private AutomatedValidation Validation
        {
            get
            {
                if (this.Validations == null || this.Validations.Count == 0)
                    this.Validations = new List<AutomatedValidation>() { new AutomatedValidation() };

                return this.Validations[0];
            }
            set
            {
                if (this.Validations == null || this.Validations.Count == 0)
                    this.Validations = new List<AutomatedValidation>() { new AutomatedValidation() };

                this.Validations[0] = value;
            }
        }

        private bool ShowPlayModeWarning()
        {
            return this.Enabled && this.Hook is OnPlayValidationHook && (this.Validation.HasActionFlag(AutomatedValidation.Action.OpenValidatorIfError) || this.Validation.HasActionFlag(AutomatedValidation.Action.OpenValidatorIfWarning));
        }

        public string Name { get { return this.Hook.Name; } }

        public AutomatedValidationHook(IValidationHook hook)
        {
            this.Hook = hook;
        }

        public void SetupHook()
        {
            this.Hook.Hook(this.OnHookExecuting);
        }

        public void Unhook()
        {
            this.Hook.Unhook(this.OnHookExecuting);
        }

        private void OnEnabledChanged()
        {
            this.Hook.Unhook(this.OnHookExecuting);

            if (this.Enabled)
            {
                this.Hook.Hook(this.OnHookExecuting);
            }
        }

        public void OnHookExecuting()
        {
            using (var runner = new ValidationRunner())
            {
                bool stopTriggeringEvent = false;

                try
                {
                    foreach (var validation in this.Validations)
                    {
                        bool openValidatorWindow = false;
                        IValidationProfile actuallyFailingProfile = null;

                        try
                        {
                            foreach (var profile in validation.ProfilesToRun)
                            {
                                foreach (var result in profile.Validate(runner))
                                {
                                    if (GUIHelper.DisplaySmartUpdatingCancellableProgressBar("Executing Validation Hook: " + this.Name + " (Profile: " + profile.Name + ")", result.Name, result.Progress))
                                    {
                                        // Cancel validation
                                        return;
                                    }

                                    foreach (var subResult in result.Results)
                                    {
                                        bool couldExitFromFailure = false;

                                        if (subResult.ResultType == ValidationResultType.Error)
                                        {
                                            if (validation.HasActionFlag(AutomatedValidation.Action.LogError))
                                            {
                                                var source = result.GetSource() as UnityEngine.Object;
                                                Debug.LogError("Validation error on object '" + source + "', path '" + subResult.Path + "': " + subResult.Message, source);
                                            }

                                            if (validation.HasActionFlag(AutomatedValidation.Action.OpenValidatorIfError))
                                            {
                                                openValidatorWindow = true;
                                                couldExitFromFailure = true;
                                            }

                                            if (validation.HasActionFlag(AutomatedValidation.Action.StopHookEventOnError))
                                            {
                                                stopTriggeringEvent = true;
                                                couldExitFromFailure = true;
                                            }
                                        }
                                        else if (subResult.ResultType == ValidationResultType.Warning)
                                        {
                                            if (validation.HasActionFlag(AutomatedValidation.Action.LogWarning))
                                            {
                                                var source = result.GetSource() as UnityEngine.Object;
                                                Debug.LogWarning("Validation warning on object '" + source + "', path '" + subResult.Path + "': " + subResult.Message, source);
                                            }

                                            if (validation.HasActionFlag(AutomatedValidation.Action.OpenValidatorIfWarning))
                                            {
                                                openValidatorWindow = true;
                                                couldExitFromFailure = true;
                                            }

                                            if (validation.HasActionFlag(AutomatedValidation.Action.StopHookEventOnWarning))
                                            {
                                                stopTriggeringEvent = true;
                                                couldExitFromFailure = true;
                                            }
                                        }

                                        if (couldExitFromFailure)
                                        {
                                            actuallyFailingProfile = profile;

                                            if (!this.FinishValidationOnFailures)
                                            {
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (openValidatorWindow)
                            {
                                if (this.Hook is OnPlayValidationHook)
                                {
                                    stopTriggeringEvent = true;
                                }

                                this.OpenValidator(validation.ProfilesToRun, actuallyFailingProfile);
                            }

                            if (stopTriggeringEvent)
                            {
                                this.Hook.StopTriggeringEvent();
                            }
                        }
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private void OpenValidator(List<IValidationProfile> profilesToRun, IValidationProfile actuallyFailingProfile)
        {
            IValidationProfile profile;

            if (profilesToRun.Count == 0)
            {
                return;
            }
            else if (profilesToRun.Count == 1)
            {
                profile = profilesToRun[0];
            }
            else if (profilesToRun.All(n => n is ValidationProfileAsset))
            {
                profile = new ValidationCollectionProfile()
                {
                    Name = "Failed '" + this.Name + "' hook profiles",
                    Description = "These are the profiles that failed when the hook was executed",
                    Profiles = profilesToRun.Cast<ValidationProfileAsset>().ToArray()
                };
            }
            else
            {
                profile = actuallyFailingProfile;
            }

            if (profile != null)
            {
                ValidationProfileManagerWindow.OpenProjectValidatorWithProfile(profile, true);
            }
        }

        public override string ToString()
        {
            return this.Hook.Name;
        }
    }
}