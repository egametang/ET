using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal class AssemblyNameProvider : IAssemblyNameProvider
  {
    private readonly Dictionary<string, PackageInfo> m_PackageInfoCache = new Dictionary<string, PackageInfo>();
    private readonly Dictionary<string, ResponseFileData> m_ResponseFilesCache = new Dictionary<string, ResponseFileData>();

    ProjectGenerationFlag m_ProjectGenerationFlag = (ProjectGenerationFlag)EditorPrefs.GetInt("unity_project_generation_flag", 3);

    public string[] ProjectSupportedExtensions => EditorSettings.projectGenerationUserExtensions;

    public string ProjectGenerationRootNamespace => EditorSettings.projectGenerationRootNamespace;

    private Assembly[] m_AllEditorAssemblies;
    private Assembly[] m_AllPlayerAssemblies;
    private Assembly[] m_AllAssemblies;

    public ProjectGenerationFlag ProjectGenerationFlag
    {
      get => m_ProjectGenerationFlag;
      private set
      {
        EditorPrefs.SetInt("unity_project_generation_flag", (int)value);
        m_ProjectGenerationFlag = value;
      }
    }

    public string GetAssemblyNameFromScriptPath(string path)
    {
      return CompilationPipeline.GetAssemblyNameFromScriptPath(path);
    }

    public Assembly[] GetAllAssemblies()
    {
      if (m_AllEditorAssemblies == null)
      {
        m_AllEditorAssemblies = GetAssembliesByType(AssembliesType.Editor);
        m_AllAssemblies = m_AllEditorAssemblies;
      }

      if (ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.PlayerAssemblies))
      {
        if (m_AllPlayerAssemblies == null)
        {
          m_AllPlayerAssemblies = GetAssembliesByType(AssembliesType.Player);
          m_AllAssemblies = new Assembly[m_AllEditorAssemblies.Length + m_AllPlayerAssemblies.Length];
          Array.Copy(m_AllEditorAssemblies, m_AllAssemblies, m_AllEditorAssemblies.Length);
          Array.Copy(m_AllPlayerAssemblies, 0, m_AllAssemblies, m_AllEditorAssemblies.Length, m_AllPlayerAssemblies.Length);
        }
      }


      return m_AllAssemblies;
    }

    private static Assembly[] GetAssembliesByType(AssembliesType type)
    {
      // This is a very expensive Unity call...
      var compilationPipelineAssemblies = CompilationPipeline.GetAssemblies(type);
      var assemblies = new Assembly[compilationPipelineAssemblies.Length];
      var i = 0;
      foreach (var compilationPipelineAssembly in compilationPipelineAssemblies)
      {
        // The CompilationPipeline's assemblies have an output path of Libraries/ScriptAssemblies
        // TODO: It might be worth using the app's copy of Assembly and updating output path when we need it
        // But that requires tracking editor and player assemblies separately
        var outputPath = type == AssembliesType.Editor
          ? $@"Temp\Bin\Debug\{compilationPipelineAssembly.name}\"
          : $@"Temp\Bin\Debug\{compilationPipelineAssembly.name}\Player\";
        assemblies[i] = new Assembly(
          compilationPipelineAssembly.name,
          outputPath,
          compilationPipelineAssembly.sourceFiles,
          compilationPipelineAssembly.defines,
          compilationPipelineAssembly.assemblyReferences,
          compilationPipelineAssembly.compiledAssemblyReferences,
          compilationPipelineAssembly.flags,
          compilationPipelineAssembly.compilerOptions
#if UNITY_2020_2_OR_NEWER
          , compilationPipelineAssembly.rootNamespace
#endif
        );
        i++;
      }

      return assemblies;
    }

    public Assembly GetNamedAssembly(string name)
    {
      foreach (var assembly in GetAllAssemblies())
      {
        if (assembly.name == name)
          return assembly;
      }

      return null;
    }

    public string GetProjectName(string name, string[] defines)
    {
      if (!ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.PlayerAssemblies))
        return name;
      return !defines.Contains("UNITY_EDITOR") ? name + ".Player" : name;
    }

    public IEnumerable<string> GetAllAssetPaths()
    {
      return AssetDatabase.GetAllAssetPaths();
    }

    private static string GetPackageRootDirectoryName(string assetPath)
    {
      const string packagesPrefix = "packages/";
      if (!assetPath.StartsWith(packagesPrefix, StringComparison.OrdinalIgnoreCase))
      {
        return null;
      }

      var followupSeparator = assetPath.IndexOf('/', packagesPrefix.Length);
      // Note that we return the first path segment without modifying/normalising case!
      return followupSeparator == -1 ? assetPath : assetPath.Substring(0, followupSeparator);
    }

    public PackageInfo GetPackageInfoForAssetPath(string assetPath)
    {
      var packageName = GetPackageRootDirectoryName(assetPath);
      if (packageName == null)
      {
        return null;
      }

      // Assume the package name casing is consistent. If it's not, we'll fall back to an uppercase variant that's
      // saved in the same dictionary. This gives us cheaper case sensitive matching, with a fallback if our assumption
      // is incorrect
      if (m_PackageInfoCache.TryGetValue(packageName, out var cachedPackageInfo))
        return cachedPackageInfo;

      var packageNameUpper = packageName.ToUpperInvariant();
      if (m_PackageInfoCache.TryGetValue(packageNameUpper, out cachedPackageInfo))
        return cachedPackageInfo;

      var result = PackageInfo.FindForAssetPath(packageName);
      m_PackageInfoCache[packageName] = result;
      m_PackageInfoCache[packageNameUpper] = result;
      return result;
    }

    public void ResetCaches()
    {
      m_PackageInfoCache.Clear();
      m_ResponseFilesCache.Clear();
      m_AllEditorAssemblies = null;
      m_AllPlayerAssemblies = null;
    }

    public bool IsInternalizedPackagePath(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        return false;
      }

      var packageInfo = GetPackageInfoForAssetPath(path);
      if (packageInfo == null)
      {
        return false;
      }

      var packageSource = packageInfo.source;
      switch (packageSource)
      {
        case PackageSource.Embedded:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Embedded);
        case PackageSource.Registry:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Registry);
        case PackageSource.BuiltIn:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.BuiltIn);
        case PackageSource.Unknown:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Unknown);
        case PackageSource.Local:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Local);
        case PackageSource.Git:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.Git);
#if UNITY_2019_3_OR_NEWER
        case PackageSource.LocalTarball:
          return !ProjectGenerationFlag.HasFlag(ProjectGenerationFlag.LocalTarBall);
#endif
      }

      return false;
    }

    public ResponseFileData ParseResponseFile(string responseFilePath, string projectDirectory,
      ApiCompatibilityLevel apiCompatibilityLevel)
    {
      var key = responseFilePath + ":" + (int) apiCompatibilityLevel;
      if (!m_ResponseFilesCache.TryGetValue(key, out var responseFileData))
      {
        var systemReferenceDirectories =
          CompilationPipeline.GetSystemAssemblyDirectories(apiCompatibilityLevel);
        responseFileData = CompilationPipeline.ParseResponseFile(
          responseFilePath,
          projectDirectory,
          systemReferenceDirectories
        );
        m_ResponseFilesCache.Add(key, responseFileData);
      }

      return responseFileData;
    }

    public IEnumerable<string> GetRoslynAnalyzerPaths()
    {
      return PluginImporter.GetAllImporters()
        .Where(i => !i.isNativePlugin && AssetDatabase.GetLabels(i).SingleOrDefault(l => l == "RoslynAnalyzer") != null)
        .Select(i => i.assetPath);
    }

    public void ToggleProjectGeneration(ProjectGenerationFlag preference)
    {
      if (ProjectGenerationFlag.HasFlag(preference))
      {
        ProjectGenerationFlag ^= preference;
      }
      else
      {
        ProjectGenerationFlag |= preference;
      }
    }

    public void ResetProjectGenerationFlag()
    {
      ProjectGenerationFlag = ProjectGenerationFlag.None;
    }
  }
}
