//-----------------------------------------------------------------------
// <copyright file="AssetValidationProfile.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class AssetValidationProfile : ValidationProfile
    {
        [FolderPath]
        public string[] SearchFilters = new string[] { "t:Prefab", "t:ScriptableObject" };

        [FolderPath]
        [PropertyTooltip("Add asset directoris or individual file paths.")]
        public string[] AssetPaths = new string[0];

        [Required]
        [PropertyTooltip("Add folders or files by reference.")]
        public UnityEngine.Object[] AssetReferences = new UnityEngine.Object[0];

        [FolderPath]
        [PropertyTooltip("Exclude asset directoris or individual file paths.")]
        public string[] ExcludeAssetPaths = new string[0];

        [Required]
        [PropertyTooltip("Exclude folders or files by reference.")]
        public UnityEngine.Object[] ExcludeAssetReferences = new UnityEngine.Object[0];

        public IEnumerable<string> GetAllAssetsToValidate()
        {
            if (this.AssetPaths == null) yield break;
            var allAssetPaths = this.AssetPaths.ToList();
            var allExcludeAssetPaths = this.ExcludeAssetPaths.ToList();
            foreach (var item in this.ExcludeAssetReferences.Where(x => x))
            {
                var path = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(path))
                {
                    if (Directory.Exists(path))
                    {
                        allAssetPaths.Add(path);
                    }
                    else if(File.Exists(path))
                    {
                        allExcludeAssetPaths.Add(path);
                    }
                }
            }

            var excludeMap = new HashSet<string>();

            // Exclude assets:
            var excludeDirectories = allExcludeAssetPaths.Select(x => x.Trim('/'))
                  .Where(x => !string.IsNullOrEmpty(x) && Directory.Exists(x))
                  .ToArray();

            var excludeAssetPaths = allExcludeAssetPaths.Where(x => File.Exists(x)).ToList();

            excludeMap.AddRange(excludeAssetPaths);
            if (excludeDirectories.Length > 0)
            {
                var guids = this.SearchFilters.SelectMany(x => AssetDatabase.FindAssets(x, excludeDirectories)).ToList();
                var assets = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToList();
                excludeMap.AddRange(assets);
            }

            // Add assets:
            var addDirectories = allAssetPaths.Select(x => x.Trim('/')).Where(x => !string.IsNullOrEmpty(x) && Directory.Exists(x)).ToArray();
            var addAssetPaths = allAssetPaths.Where(x => File.Exists(x)).ToList();

            if (addDirectories.Length > 0)
            {
                var guids = this.SearchFilters.SelectMany(x => AssetDatabase.FindAssets(x, addDirectories)).ToList();
                var assets = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToList();

                foreach (var asset in assets)
                {
                    if (excludeMap.Add(asset))
                    {
                        yield return asset;
                    }
                }
            }

            foreach (var asset in addAssetPaths)
            {
                if (excludeMap.Add(asset))
                {
                    yield return asset;
                }
            }
        }

        public override IEnumerable<ValidationProfileResult> Validate(ValidationRunner runner)
        {
            var assetPaths = this.GetAllAssetsToValidate().ToList();

            var step = 1f / assetPaths.Count;
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var path = assetPaths[i];
                var progress = step * i;
                var assetsAtPath = this.LoadAllAssetsAtPathProperly(path);

                foreach (var loadInfo in assetsAtPath)
                {
                    if (loadInfo.Type  == LoadedAssetInfo.AssetType.Normal)
                    {
                        List<ValidationResult> results = null;

                        runner.ValidateUnityObjectRecursively(loadInfo.Asset, ref results);

                        yield return new ValidationProfileResult()
                        {
                            Profile = this,
                            Progress = progress,
                            Name = Path.GetFileName(path),
                            Source = loadInfo.Asset,
                            Results = results,
                            SourceRecoveryData = loadInfo.Asset,
                            Path = path,
                        };
                    }
                    else if (loadInfo.Type == LoadedAssetInfo.AssetType.BrokenComponent)
                    {
                        yield return new ValidationProfileResult()
                        {
                            Profile = this,
                            Progress = progress,
                            Name = Path.GetFileName(path),
                            Source = loadInfo.OwningGO,
                            Results = new List<ValidationResult>()
                            {
                                new ValidationResult()
                                {
                                    Message = object.ReferenceEquals(loadInfo.BrokenComponent, null) ?
                                            "Broken Component: a component at index '" + loadInfo.ComponentIndex + "' is null on the GameObject '" + loadInfo.OwningGO.name + "'! A script reference is likely broken." :
                                            "Broken Component: a component of type '" + loadInfo.BrokenComponent.GetType().GetNiceName() + "' at index '" + loadInfo.ComponentIndex + "' is null on the GameObject '" + loadInfo.OwningGO.name + "'! A script reference is likely broken.",
                                    ResultType = ValidationResultType.Error,
                                }
                            },
                            SourceRecoveryData = loadInfo.OwningGO,
                            Path = path,
                        };
                    }
                    else
                    {
                        throw new NotImplementedException(loadInfo.Type.ToString());
                    }
                }
            }
        }

        private IEnumerable<LoadedAssetInfo> LoadAllAssetsAtPathProperly(string path)
        {
            if (!path.FastEndsWith(".prefab"))
            {
                // Not a prefab, we can just do it the normal way
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                var result = new LoadedAssetInfo[assets.Length];

                for (int i = 0; i < assets.Length; i++)
                {
                    result[i] = new LoadedAssetInfo(assets[i]);
                }

                return result;
            }

            // It's a prefab; we need to get fancy with it, since AssetDatabase.LoadAllAssetsAtPath often loads broken null values for prefabs instead of loading them properly
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;

            if (mainAsset == null) return new LoadedAssetInfo[0]; // Nevermind I guess

            return this.RecurseThroughPrefab(mainAsset);
        }

        private IEnumerable<LoadedAssetInfo> RecurseThroughPrefab(GameObject go)
        {
            yield return new LoadedAssetInfo(go);

            if (go != null)
            {
                Component[] components = go.GetComponents(typeof(Component));

                for (int i = 0; i < components.Length; i++)
                {
                    var component = components[i];

                    if (component != null)
                    {
                        yield return new LoadedAssetInfo(component);
                    }
                    else
                    {
                        yield return new LoadedAssetInfo(go, component, i);
                    }
                }

                var transform = go.transform;

                for (int i = 0; i < transform.childCount; i++)
                {
                    foreach (var childResult in this.RecurseThroughPrefab(transform.GetChild(i).gameObject))
                    {
                        yield return childResult;
                    }
                }
            }
        }

        private struct LoadedAssetInfo
        {
            public enum AssetType
            {
                Normal,
                BrokenComponent,
            }

            public AssetType Type;
            public UnityEngine.Object Asset;
            public GameObject OwningGO;
            public Component BrokenComponent;
            public int ComponentIndex;

            public LoadedAssetInfo(UnityEngine.Object asset)
            {
                this.Type = AssetType.Normal;
                this.Asset = asset;
                this.OwningGO = null;
                this.BrokenComponent = null;
                this.ComponentIndex = -1;
            }

            public LoadedAssetInfo(GameObject owningGo, Component brokenComponent, int componentIndex)
            {
                this.Type = AssetType.BrokenComponent;
                this.Asset = null;
                this.OwningGO = owningGo;
                this.BrokenComponent = brokenComponent;
                this.ComponentIndex = componentIndex;
            }
        }
    }
}