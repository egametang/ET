using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal interface IAssemblyNameProvider
  {
    string[] ProjectSupportedExtensions { get; }
    string ProjectGenerationRootNamespace { get; }
    ProjectGenerationFlag ProjectGenerationFlag { get; }

    string GetAssemblyNameFromScriptPath(string path);
    string GetProjectName(string name, string[] defines);
    bool IsInternalizedPackagePath(string path);
    Assembly[] GetAllAssemblies();
    Assembly GetNamedAssembly(string name);
    IEnumerable<string> GetAllAssetPaths();
    UnityEditor.PackageManager.PackageInfo GetPackageInfoForAssetPath(string assetPath);
    ResponseFileData ParseResponseFile(string responseFilePath, string projectDirectory, ApiCompatibilityLevel systemReferenceDirectories);
    IEnumerable<string> GetRoslynAnalyzerPaths();
    void ToggleProjectGeneration(ProjectGenerationFlag preference);
    void ResetCaches();
  }
}