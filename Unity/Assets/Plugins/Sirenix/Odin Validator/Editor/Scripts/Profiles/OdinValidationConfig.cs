//-----------------------------------------------------------------------
// <copyright file="OdinValidationConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    internal class OdinValidatorConfigAttribute : GlobalConfigAttribute
    {
        public OdinValidatorConfigAttribute(string configPath) : base(SirenixAssetPaths.SirenixPluginPath + configPath) { }
    }

    [OdinValidatorConfig("Odin Validator/Editor/Config/"), ShowOdinSerializedPropertiesInInspector]
    public class OdinValidationConfig : GlobalConfig<OdinValidationConfig>, ISerializationCallbackReceiver, IOverridesSerializationFormat
    {
        [InitializeOnLoadMethod]
        private static void InitHooks()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null || UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                if (!HasInstanceLoaded)
                {
                    LoadInstanceIfAssetExists();
                }

                if (!HasInstanceLoaded)
                {
                    Debug.LogError("Odin's validation config asset could not be loaded during batch mode or headless run of the Unity Editor. No validation has taken place, whether it was configured to do so or not.");
                    return;
                }
            }

            Action action = () =>
            {
                foreach (var item in OdinValidationConfig.Instance.Hooks)
                {
                    if (item.Enabled)
                    {
                        item.SetupHook();
                    }
                }
            };

            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null || UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                action();
            }
            else
            {
                UnityEditorEventUtility.DelayAction(action);
            }
        }

        [Required]
        [HideReferenceObjectPicker]
        [AssetSelector(Filter = "t:ValidationProfileAsset", DrawDropdownForListElements = false)]
        public List<IValidationProfile> MainValidationProfiles;

        [ValueDropdown("GetAvailableHooks", DrawDropdownForListElements = false, DropdownWidth = 250), HideReferenceObjectPicker]
        [ListDrawerSettings(ListElementLabelName = "Name", Expanded = false, DraggableItems = false)]
        public List<AutomatedValidationHook> Hooks = new List<AutomatedValidationHook>();

        private IEnumerable GetAvailableHooks()
        {
            var notTheseHooks = new HashSet<Type>();

            if (this.Hooks != null)
                notTheseHooks.AddRange(this.Hooks.Where(n => n.Hook != null).Select(n => n.Hook.GetType()));

            var availableHookTypes = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes)
                .Where(type => !type.IsAbstract && !type.IsInterface && !notTheseHooks.Contains(type) && typeof(IValidationHook).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null).ToList();

            return availableHookTypes
                .Select(type => new AutomatedValidationHook((IValidationHook)Activator.CreateInstance(type)))
                .Select(x => new ValueDropdownItem(x.Hook.Name, x));
        }

        #region OdinSerialization

        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);

            if (this.Hooks == null)
            {
                this.Hooks = new List<AutomatedValidationHook>();
            }
            else
            {
                this.Hooks.RemoveAll(n => n.Hook == null);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
        }

        DataFormat IOverridesSerializationFormat.GetFormatToSerializeAs(bool isPlayer)
        {
            return DataFormat.Nodes;
        }

        #endregion

        [Button(ButtonSizes.Medium), HorizontalGroup(0.5f, Order = -2), PropertyOrder(-20)]
        private void CreateNewProfile()
        {
            ValidationProfileSOCreator.ShowDialog<ValidationProfileAsset>(SirenixAssetPaths.SirenixPluginPath + "Odin Validator/Editor/Config/", newProfile =>
            {
                if (newProfile != null)
                {
                    this.MainValidationProfiles.Add(newProfile);
                    InspectorUtilities.RegisterUnityObjectDirty(this);
                }
            });
        }

        [Button("Reset Default Profiles", ButtonSizes.Medium), HorizontalGroup(0.5f)]
        public void ResetMainProfilesToDefault()
        {
            this.ResetData(false);
        }

        protected override void OnConfigAutoCreated()
        {
            this.ResetData(true);
        }

        private TAsset GetOrCreateValidationProfileSubAsset<TAsset, TProfile>(TProfile newProfile, bool overridePreExistingProfile)
            where TAsset : ValidationProfileAsset<TProfile>
            where TProfile : IValidationProfile
        {
            var path = AssetDatabase.GetAssetPath(this);
            var asset = AssetDatabase.LoadAllAssetsAtPath(path).OfType<TAsset>().FirstOrDefault(a => a.name == newProfile.Name);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TAsset>();
                asset.Profile = newProfile;
                asset.name = newProfile.Name;
                AssetDatabase.AddObjectToAsset(asset, this);
                AssetDatabase.SaveAssets();
            }
            else if (overridePreExistingProfile)
            {
                asset.Profile = newProfile;
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }

            return asset;
        }

        private void ResetData(bool overridePreExistingProfileAssets)
        {
            var scanAllAssets = this.GetOrCreateValidationProfileSubAsset<AssetValidationProfileAsset, AssetValidationProfile>(new AssetValidationProfile()
            {
                Name = "Scan All Assets",
                Description = "Scans all prefabs and scriptable objects in the project",
                AssetPaths = new string[] { "Assets" },
                SearchFilters = new string[] { "t:Prefab", "t:ScriptableObject" }
            }, overridePreExistingProfileAssets);

            var scanAllScenes = this.GetOrCreateValidationProfileSubAsset<SceneValidationProfileAsset, SceneValidationProfile>(new SceneValidationProfile()
            {
                Name = "Scan All Scenes",
                Description = "Scans all scenes in the project",
                ScenePaths = new string[] { "Assets" }
            }, overridePreExistingProfileAssets);

            var scanEntireProject = this.GetOrCreateValidationProfileSubAsset<ValidationCollectionProfileAsset, ValidationCollectionProfile>(new ValidationCollectionProfile()
            {
                Name = "Scan Entire Project",
                Description = "Scans all prefabs, scriptable objects and scenes in the project",
                Profiles = new ValidationProfileAsset[] { scanAllAssets, scanAllScenes }
            }, overridePreExistingProfileAssets);

            var scanOpenScenes = this.GetOrCreateValidationProfileSubAsset<SceneValidationProfileAsset, SceneValidationProfile>(new SceneValidationProfile()
            {
                Name = "Scan Open Scenes",
                Description = "Scans all open scenes, without going through scene asset dependencies.",
                IncludeOpenScenes = true,
            }, overridePreExistingProfileAssets);

            var scanScenesFromBuildOptions = this.GetOrCreateValidationProfileSubAsset<SceneValidationProfileAsset, SceneValidationProfile>(new SceneValidationProfile()
            {
                Name = "Scan Scenes From Build Options",
                Description = "Scans all scenes from build options, including scene asset dependencies.",
                IncludeScenesFromBuildOptions = true,
                IncludeAssetDependencies = true,
            }, overridePreExistingProfileAssets);

            var onPlayHook = new AutomatedValidationHook(new OnPlayValidationHook())
            {
                Enabled = false,
                Validations = new List<AutomatedValidation>()
                {
                    new AutomatedValidation()
                    {
                        Actions = AutomatedValidation.Action.OpenValidatorIfError | AutomatedValidation.Action.OpenValidatorIfWarning,
                        ProfilesToRun = new List<IValidationProfile>() { scanOpenScenes }
                    }
                }
            };

            var onBuild = new AutomatedValidationHook(new OnBuildValidationHook())
            {
                Enabled = false,
                Validations = new List<AutomatedValidation>()
                {
                    new AutomatedValidation()
                    {
                        Actions = AutomatedValidation.Action.OpenValidatorIfError | AutomatedValidation.Action.OpenValidatorIfWarning,
                        ProfilesToRun = new List<IValidationProfile>() { scanScenesFromBuildOptions }
                    }
                }
            };

            var onStartup = new AutomatedValidationHook(new OnProjectStartupValidationHook())
            {
                Enabled = false,
                Validations = new List<AutomatedValidation>()
                {
                    new AutomatedValidation()
                    {
                        Actions = AutomatedValidation.Action.OpenValidatorIfError | AutomatedValidation.Action.OpenValidatorIfWarning,
                        ProfilesToRun = new List<IValidationProfile>() { scanEntireProject }
                    }
                }
            };

            this.MainValidationProfiles = new List<IValidationProfile>()
            {
                scanEntireProject,
                scanAllAssets,
                scanAllScenes,
                scanOpenScenes,
                scanScenesFromBuildOptions,
            };

            this.Hooks = new List<AutomatedValidationHook>()
            {
                onPlayHook,
                onBuild,
                onStartup
            };
        }
    }
}