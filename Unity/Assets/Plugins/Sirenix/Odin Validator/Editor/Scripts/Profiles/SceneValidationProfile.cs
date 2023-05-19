//-----------------------------------------------------------------------
// <copyright file="SceneValidationProfile.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinValidator.Editor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.Validation;
    using Sirenix.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [Serializable]
    public class SceneValidationProfile : ValidationProfile
    {
        [ToggleLeft]
        public bool IncludeScenesFromBuildOptions;

        [ToggleLeft]
        public bool IncludeOpenScenes;

        [ToggleLeft]
        public bool IncludeAssetDependencies;

        [Sirenix.OdinInspector.FilePath] // UnityEditor.FilePath was added in 2020.1 Alpha
        [ListDrawerSettings(Expanded = true)]
        [PropertyTooltip("Reference folders containing scenes or individual scenes.")]
        public string[] ScenePaths = new string[0], ExcludeScenePaths = new string[0];

        public IEnumerable<string> GetAllScenes()
        {
            var excludeMap = new HashSet<string>();

            // Exclude scenes:
            if (this.ExcludeScenePaths != null)
            {
                var excludeDirectories = this.ExcludeScenePaths.Select(x => x.Trim('/')).Where(x => !string.IsNullOrEmpty(x) && Directory.Exists(x)).ToArray();
                var excludeScenesFiles = this.ExcludeScenePaths.Where(x => File.Exists(x));
                if (excludeDirectories.Length > 0)
                {
                    excludeMap.AddRange(AssetDatabase.FindAssets("t:SceneAsset", excludeDirectories).Select(x => AssetDatabase.GUIDToAssetPath(x)));
                }
                excludeMap.AddRange(excludeScenesFiles);
            }

            // Add scenes:
            if (this.ScenePaths != null)
            {
                var addDirectories = this.ScenePaths.Select(x => x.Trim('/')).Where(x => !string.IsNullOrEmpty(x) && Directory.Exists(x)).ToArray();
                var addSceneFiles = this.ScenePaths.Where(x => File.Exists(x));

                if (addDirectories.Length > 0)
                {
                    var scenes = AssetDatabase.FindAssets("t:SceneAsset", addDirectories)
                        .Select(x => AssetDatabase.GUIDToAssetPath(x));

                    foreach (var scene in scenes)
                    {
                        if (excludeMap.Add(scene))
                        {
                            yield return scene;
                        }
                    }
                }

                foreach (var scene in addSceneFiles)
                {
                    if (excludeMap.Add(scene))
                    {
                        yield return scene;
                    }
                }
            }

            if (this.IncludeScenesFromBuildOptions)
            {
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (scene.enabled && !string.IsNullOrEmpty(scene.path) && excludeMap.Add(scene.path) && File.Exists(scene.path))
                    {
                        yield return scene.path;
                    }
                }
            }

            if (this.IncludeOpenScenes)
            {
                var setupScenes = EditorSceneManager.GetSceneManagerSetup();

                foreach (var scene in setupScenes)
                {
                    if (!string.IsNullOrEmpty(scene.path) && excludeMap.Add(scene.path))
                    {
                        yield return scene.path;
                    }
                }
            }
        }

        public override object GetSource(ValidationProfileResult entry)
        {
            if (entry.Source as UnityEngine.Object) return entry.Source;

            var address = (SceneAddress)entry.SourceRecoveryData; // This should work.

            bool openScene = true;

            foreach (var setupScene in EditorSceneManager.GetSceneManagerSetup())
            {
                if (setupScene.path == address.ScenePath)
                {
                    openScene = false;

                    if (!setupScene.isActive)
                        EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneByPath(setupScene.path));

                    break;
                }
            }

            if (openScene)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return null;

                EditorSceneManager.OpenScene(address.ScenePath, OpenSceneMode.Single);
            }

            var go = this.GetGameObjectFromIndexPath(address);

            if (go != null)
            {
                entry.Source = go;

                if (address.ComponentType != null)
                {
                    Component component = null;
                    var components = go.GetComponents(address.ComponentType);

                    if (address.ComponentIndex >= 0 && address.ComponentIndex < components.Length)
                        component = components[address.ComponentIndex];

                    if (component != null)
                        entry.Source = component;
                }
            }

            return entry.Source;
        }

        private GameObject GetGameObjectFromIndexPath(SceneAddress address)
        {
            var path = address.HierarchyIndexPath;

            if (path.Count == 0) return null;

            GameObject[] roots = SceneRoots(address.ScenePath).ToArray();

            if (path[0] >= roots.Length) return null;

            Transform curr = roots[path[0]].transform;

            for (int i = 1; i < path.Count; i++)
            {
                curr = curr.GetChild(path[i]);
                if (curr == null) return null;
            }

            return curr.gameObject;
        }

        private static readonly MethodInfo Scene_GetRootGameObjects_Method = typeof(Scene).GetMethod("GetRootGameObjects", Flags.InstancePublic, null, Type.EmptyTypes, null);

        public static IEnumerable<GameObject> SceneRoots(string scenePath)
        {
            var scene = EditorSceneManager.GetSceneByPath(scenePath);

            if (Scene_GetRootGameObjects_Method != null && scene.IsValid())
            {
                var roots = (GameObject[])Scene_GetRootGameObjects_Method.Invoke(scene, null);

                foreach (var root in roots)
                    yield return root;
            }
            else
            {
                // Fallback; only works in Unity versions without multi-scene support
                var prop = new HierarchyProperty(HierarchyType.GameObjects);
                var expanded = new int[0];

                while (prop.Next(expanded))
                {
                    yield return prop.pptrValue as GameObject;
                }
            }
        }

        public override IEnumerable<ValidationProfileResult> Validate(ValidationRunner runner)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                yield break;

            var selection = Selection.objects;
            var scenesToTest = this.GetAllScenes().ToList();
            var setup = EditorSceneManager.GetSceneManagerSetup();

            var partialProgress = 0f;
            var partialProgressStepSize = 1f / (scenesToTest.Count + (this.IncludeAssetDependencies ? 1 : 0));

            try
            {
                for (int i = 0; i < scenesToTest.Count; i++)
                {
                    var scene = scenesToTest[i];

                    EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);

                    var gameObjectsToScan = Resources.FindObjectsOfTypeAll<Transform>()
                       .Where(x => (x.gameObject.scene.IsValid() && (x.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0))
                       //.SelectMany(x => x.GetComponents(typeof(Component)).Select(c => new { go = x.gameObject, component = c }))
                       .ToList();

                    var step = 1f / gameObjectsToScan.Count;
                    for (int j = 0; j < gameObjectsToScan.Count; j++)
                    {
                        var go = gameObjectsToScan[j].gameObject;
                        var progress = j * step * partialProgressStepSize + partialProgress;

                        {
                            var results = runner.ValidateUnityObjectRecursively(go);

                            var recoveryData = this.GetRecoveryData(go, null, scene);

                            var entry = new ValidationProfileResult()
                            {
                                Name = go.name,
                                Profile = this,
                                Source = go,
                                Results = results,
                                Progress = progress,
                                SourceRecoveryData = recoveryData,
                                Path = recoveryData.HierarchyPath,
                            };

                            yield return entry;
                        }

                        var components = go.GetComponents<Component>();

                        for (int k = 0; k < components.Length; k++)
                        {
                            var component = components[k];
                            var recoveryData = this.GetRecoveryData(go, component, scene);

                            if (component == null)
                            {
                                var entry = new ValidationProfileResult()
                                {
                                    Name = go.name + " (Broken Component)",
                                    Source = go,
                                    SourceRecoveryData = recoveryData,
                                    Profile = this,
                                    Progress = progress,
                                    Results = new List<ValidationResult>() { new ValidationResult()
                                    {
                                        Message = object.ReferenceEquals(component, null) ?
                                            "Broken Component: a component at index '" + k + "' is null on the GameObject '" + go.name + "'! A script reference is likely broken." :
                                            "Broken Component: a component of type '" + component.GetType().GetNiceName() + "' at index '" + k + "' is null on the GameObject '" + go.name + "'! A script reference is likely broken.",
                                        ResultType = ValidationResultType.Error,
                                    }},
                                    Path = recoveryData.HierarchyPath,
                                };

                                yield return entry;
                            }
                            else
                            {
                                var result = runner.ValidateUnityObjectRecursively(component);
                                var entry = new ValidationProfileResult()
                                {
                                    Name = go.name + " - " + component.GetType().GetNiceName().SplitPascalCase(),
                                    Profile = this,
                                    Source = component,
                                    Results = result,
                                    Progress = progress,
                                    SourceRecoveryData = this.GetRecoveryData(go, component, scene),
                                    Path = recoveryData.HierarchyPath,
                                };

                                yield return entry;
                            }
                        }
                    }
                    partialProgress += partialProgressStepSize;
                }
            }
            finally
            {
                // Load a new empty scene that will be unloaded immediately, just to be sure we completely clear all changes made by the scan
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                if (setup.Length != 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }

            if (this.IncludeAssetDependencies)
            {
                var scenes = new HashSet<UnityEngine.Object>(scenesToTest
                    .Select(x => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(x))).ToArray();

                var dep = EditorUtility.CollectDependencies(scenes);
                var components = dep.OfType<Component>().ToList();
                var scriptableObjects = dep.OfType<ScriptableObject>().ToList();
                var allObjects = components.Cast<UnityEngine.Object>().Concat(scriptableObjects.Cast<UnityEngine.Object>())
                    .ToArray();

                

                var step = 1f / allObjects.Length;
                for (int i = 0; i < allObjects.Length; i++)
                {
                    var obj = allObjects[i];
                    var progress = i * step * partialProgressStepSize + partialProgress;
                    var result = runner.ValidateUnityObjectRecursively(obj);
                    string path = AssetDatabase.Contains(obj) ? AssetDatabase.GetAssetPath(obj) : "";

                    var entry = new ValidationProfileResult()
                    {
                        Name = obj.name,
                        Profile = this,
                        Source = obj,
                        Results = result,
                        Progress = progress,
                        Path = path,
                    };

                    yield return entry;
                }
            }

            Selection.objects = selection;
        }

        private SceneAddress GetRecoveryData(GameObject go, Component co, string scene)
        {
            var result = new SceneAddress();
            result.ScenePath = scene;
            result.HierarchyPath = GetGameObjectPath(go.transform);
            result.HierarchyIndexPath = GetGameObjectIndexPath(go.transform);

            if (co != null)
            {
                result.ComponentType = co.GetType();
                result.ComponentIndex = (go.GetComponents(co.GetType()) as IList).IndexOf(co);
            }

            return result;
        }

        private static string GetGameObjectPath(Transform transform)
        {
            var path = "";
            var curr = transform;

            while (curr)
            {
                if (path != "")
                    path = "/" + path;

                path = curr.name + path;
                curr = curr.parent;
            }

            return path;
        }

        private static List<int> GetGameObjectIndexPath(Transform transform)
        {
            var result = new List<int>();
            var curr = transform;
            while (curr)
            {
                result.Add(curr.GetSiblingIndex());
                curr = curr.parent;
            }
            result.Reverse();
            return result;
        }

        public struct SceneAddress
        {
            public string ScenePath;
            public string HierarchyPath;
            public List<int> HierarchyIndexPath;
            public Type ComponentType;
            public int ComponentIndex;
        }
    }
}