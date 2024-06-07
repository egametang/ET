using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Packages.Rider.Editor.Util;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal class ProjectGeneration : IGenerator
  {
    private enum ScriptingLanguage
    {
      None,
      CSharp
    }

    /// <summary>
    /// Map source extensions to ScriptingLanguages
    /// </summary>
    private static readonly Dictionary<string, ScriptingLanguage> k_BuiltinSupportedExtensions =
      new Dictionary<string, ScriptingLanguage>
      {
        { ".cs", ScriptingLanguage.CSharp },
        { ".uxml", ScriptingLanguage.None },
        { ".uss", ScriptingLanguage.None },
        { ".shader", ScriptingLanguage.None },
        { ".compute", ScriptingLanguage.None },
        { ".cginc", ScriptingLanguage.None },
        { ".hlsl", ScriptingLanguage.None },
        { ".glslinc", ScriptingLanguage.None },
        { ".template", ScriptingLanguage.None },
        { ".raytrace", ScriptingLanguage.None },
        { ".json", ScriptingLanguage.None},
        { ".rsp", ScriptingLanguage.None},
        { ".asmdef", ScriptingLanguage.None},
        { ".asmref", ScriptingLanguage.None},
        { ".xaml", ScriptingLanguage.None},
        { ".tt", ScriptingLanguage.None},
        { ".t4", ScriptingLanguage.None},
        { ".ttinclude", ScriptingLanguage.None}
      };

    private string[] m_ProjectSupportedExtensions = Array.Empty<string>();

    // Note that ProjectDirectory can be assumed to be the result of Path.GetFullPath
    public string ProjectDirectory { get; }
    public string ProjectDirectoryWithSlash { get; }

    private readonly string m_ProjectName;
    private readonly IAssemblyNameProvider m_AssemblyNameProvider;
    private readonly IFileIO m_FileIOProvider;
    private readonly IGUIDGenerator m_GUIDGenerator;

    private readonly Dictionary<string, string> m_ProjectGuids = new Dictionary<string, string>();

    // If we have multiple projects, the same assembly references are reused for each. Caching the normalised paths and
    // names is actually cheaper than recalculating each time, in terms of both time and memory allocations
    private readonly Dictionary<string, string> m_NormalisedPaths = new Dictionary<string, string>();
    private readonly Dictionary<string, string> m_AssemblyNames = new Dictionary<string, string>();

    internal static bool isRiderProjectGeneration; // workaround to https://github.cds.internal.unity3d.com/unity/com.unity.ide.rider/issues/28

    IAssemblyNameProvider IGenerator.AssemblyNameProvider => m_AssemblyNameProvider;

    public ProjectGeneration()
      : this(Directory.GetParent(Application.dataPath).FullName) { }

    public ProjectGeneration(string projectDirectory)
      : this(projectDirectory, new AssemblyNameProvider(), new FileIOProvider(), new GUIDProvider()) { }

    public ProjectGeneration(string projectDirectory, IAssemblyNameProvider assemblyNameProvider, IFileIO fileIoProvider, IGUIDGenerator guidGenerator)
    {
      ProjectDirectory = Path.GetFullPath(projectDirectory.NormalizePath());
      ProjectDirectoryWithSlash = ProjectDirectory + Path.DirectorySeparatorChar;
      m_ProjectName = Path.GetFileName(ProjectDirectory);
      m_AssemblyNameProvider = assemblyNameProvider;
      m_FileIOProvider = fileIoProvider;
      m_GUIDGenerator = guidGenerator;
    }

    /// <summary>
    /// Syncs the scripting solution if any affected files are relevant.
    /// </summary>
    /// <returns>
    /// Whether the solution was synced.
    /// </returns>
    /// <param name='affectedFiles'>
    /// A set of files whose status has changed
    /// </param>
    /// <param name="reimportedFiles">
    /// A set of files that got reimported
    /// </param>
    /// <param name="checkProjectFiles">
    /// Check if project files were changed externally
    /// </param>
    public bool SyncIfNeeded(IEnumerable<string> affectedFiles, IEnumerable<string> reimportedFiles, bool checkProjectFiles = false)
    {
      SetupSupportedExtensions();

      PackageManagerTracker.SyncIfNeeded(checkProjectFiles);

      if (HasFilesBeenModified(affectedFiles, reimportedFiles) || RiderScriptEditorData.instance.hasChanges
                                                               || RiderScriptEditorData.instance.HasChangesInCompilationDefines()
                                                               || (checkProjectFiles && LastWriteTracker.HasLastWriteTimeChanged()))
      {
        Sync();
        return true;
      }

      return false;
    }

    private bool HasFilesBeenModified(IEnumerable<string> affectedFiles, IEnumerable<string> reimportedFiles)
    {
      return affectedFiles.Any(ShouldFileBePartOfSolution) || reimportedFiles.Any(ShouldSyncOnReimportedAsset);
    }

    private static bool ShouldSyncOnReimportedAsset(string asset)
    {
      var extension = Path.GetExtension(asset);
      return extension == ".asmdef" || extension == ".asmref" || Path.GetFileName(asset) == "csc.rsp";
    }

    public void Sync()
    {
      SetupSupportedExtensions();
      var types = GetAssetPostprocessorTypes();
      isRiderProjectGeneration = true;
      var externalCodeAlreadyGeneratedProjects = OnPreGeneratingCSProjectFiles(types);
      isRiderProjectGeneration = false;
      if (!externalCodeAlreadyGeneratedProjects)
      {
        GenerateAndWriteSolutionAndProjects(types);
      }

      OnGeneratedCSProjectFiles(types);
      m_AssemblyNameProvider.ResetCaches();
      m_AssemblyNames.Clear();
      m_NormalisedPaths.Clear();
      m_ProjectGuids.Clear();
      _buffer = null;
      RiderScriptEditorData.instance.hasChanges = false;
      RiderScriptEditorData.instance.InvalidateSavedCompilationDefines();
    }

    public bool HasSolutionBeenGenerated()
    {
      return m_FileIOProvider.Exists(SolutionFile());
    }

    private void SetupSupportedExtensions()
    {
      var extensions = m_AssemblyNameProvider.ProjectSupportedExtensions;
      m_ProjectSupportedExtensions = new string[extensions.Length];
      for (var i = 0; i < extensions.Length; i++)
      {
        m_ProjectSupportedExtensions[i] = "." + extensions[i];
      }
    }

    private bool ShouldFileBePartOfSolution(string file)
    {
      // Exclude files coming from packages except if they are internalized.
      if (m_AssemblyNameProvider.IsInternalizedPackagePath(file))
      {
          return false;
      }
      return HasValidExtension(file);
    }

    public bool HasValidExtension(string file)
    {
      // Dll's are not scripts but still need to be included..
      if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
          return true;

      var extension = Path.GetExtension(file);
      return IsSupportedExtension(extension);
    }

    private bool IsSupportedExtension(string extension)
    {
      return k_BuiltinSupportedExtensions.ContainsKey(extension) || m_ProjectSupportedExtensions.Contains(extension);
    }

    private class AssemblyUsage
    {
      private readonly HashSet<string> m_ProjectAssemblies = new HashSet<string>();
      private readonly HashSet<string> m_PrecompiledAssemblies = new HashSet<string>();

      public void AddProjectAssembly(Assembly assembly)
      {
        m_ProjectAssemblies.Add(assembly.name);
      }

      public void AddPrecompiledAssembly(Assembly assembly)
      {
        m_PrecompiledAssemblies.Add(assembly.name);
      }

      public bool IsProjectAssembly(Assembly assembly) => m_ProjectAssemblies.Contains(assembly.name);
      public bool IsPrecompiledAssembly(Assembly assembly) => m_PrecompiledAssemblies.Contains(assembly.name);
    }

    private void GenerateAndWriteSolutionAndProjects(Type[] types)
    {
      // Only synchronize islands that have associated source files and ones that we actually want in the project.
      // This also filters out DLLs coming from .asmdef files in packages.

      // Get all of the assemblies that Unity will compile from source. This includes Assembly-CSharp, all user assembly
      // definitions, and all packages. Not all of the returned assemblies will require project files - by default,
      // registry, git and local tarball packages are pre-compiled by Unity and will not require a project. This can be
      // changed by the user in the External Tools settings page.
      // Each assembly instance contains source files, output path, defines, compiler options and references. There
      // will be `compiledAssemblyReferences`, which are DLLs, such as UnityEngine.dll, and assembly references, which
      // are references to other assemblies that Unity will compile from source. Again, these assemblies might be
      // projects, or pre-compiled by Unity, depending on the options selected by the user.
      var allAssemblies = m_AssemblyNameProvider.GetAllAssemblies();
      var assemblyUsage = new AssemblyUsage();
      foreach (var assembly in allAssemblies)
      {
        if (assembly.sourceFiles.Any(ShouldFileBePartOfSolution))
          assemblyUsage.AddProjectAssembly(assembly);
        else
          assemblyUsage.AddPrecompiledAssembly(assembly);
      }

      // Get additional assets (other than source files) that we want to add to the projects, e.g. shaders, asmdef, etc.
      var additionalAssetsByAssembly = GetAdditionalAssets();

      var projectParts = new List<ProjectPart>();
      var assemblyNamesWithSource = new HashSet<string>();
      foreach (var assembly in allAssemblies)
      {
        if (!assemblyUsage.IsProjectAssembly(assembly))
          continue;

        // TODO: Will this check ever be true? Player assemblies don't have the same name as editor assemblies, right?
        if (assemblyNamesWithSource.Contains(assembly.name))
          projectParts.Add(new ProjectPart(assembly.name, assembly, new List<string>())); // do not add asset project parts to both editor and player projects
        else
        {
          additionalAssetsByAssembly.TryGetValue(assembly.name, out var additionalAssetsForProject);
          projectParts.Add(new ProjectPart(assembly.name, assembly, additionalAssetsForProject));
          assemblyNamesWithSource.Add(assembly.name);
        }
      }

      // If there are any assets that should be in a separate assembly, but that assembly folder doesn't contain any
      // source files, we'll have orphaned assets. Create a project for these assemblies, with references based on the
      // Rider package assembly
      // TODO: Would this produce the same results if we removed the check for ShouldFileBePartOfSolution above?
      // I suspect the only difference would be output path and references, and potentially simplify things
      var executingAssemblyName = typeof(ProjectGeneration).Assembly.GetName().Name;
      var riderAssembly = m_AssemblyNameProvider.GetNamedAssembly(executingAssemblyName);
      string[] coreReferences = null;
      foreach (var pair in additionalAssetsByAssembly)
      {
        var assembly = pair.Key;
        var additionalAssets = pair.Value;

        if (!assemblyNamesWithSource.Contains(assembly))
        {
          if (coreReferences == null)
          {
            coreReferences = riderAssembly?.compiledAssemblyReferences.Where(a =>
              a.EndsWith("UnityEditor.dll", StringComparison.Ordinal) ||
              a.EndsWith("UnityEngine.dll", StringComparison.Ordinal) ||
              a.EndsWith("UnityEngine.CoreModule.dll", StringComparison.Ordinal)).ToArray();
          }

          projectParts.Add(AddProjectPart(assembly, riderAssembly, coreReferences, additionalAssets));
        }
      }

      var stringBuilder = new StringBuilder();
      SyncSolution(stringBuilder, projectParts, types);
      stringBuilder.Clear();

      foreach (var projectPart in projectParts)
      {
        SyncProject(stringBuilder, projectPart, assemblyUsage, types);
        stringBuilder.Clear();
      }
    }

    private static ProjectPart AddProjectPart(string assemblyName, Assembly riderAssembly, string[] coreReferences,
      List<string> additionalAssets)
    {
      Assembly assembly = null;
      if (riderAssembly != null)
      {
        // We want to add those references, so that Rider would detect Unity path and version and provide rich features for shader files
        // Note that output path will be Library/ScriptAssemblies
        assembly = new Assembly(assemblyName, riderAssembly.outputPath, Array.Empty<string>(),
          new []{"UNITY_EDITOR"},
          Array.Empty<Assembly>(),
          coreReferences,
          riderAssembly.flags);
      }
      return new ProjectPart(assemblyName, assembly, additionalAssets);
    }

    private Dictionary<string, List<string>> GetAdditionalAssets()
    {
      var assemblyDllNames = new FilePathTrie<string>();
      var interestingAssets = new List<string>();
      foreach (var assetPath in m_AssemblyNameProvider.GetAllAssetPaths())
      {
        if (m_AssemblyNameProvider.IsInternalizedPackagePath(assetPath))
          continue;

        // Find all the .asmdef and .asmref files. Then get the assembly for a file in the same folder. Anything in that
        // folder or below will be in the same assembly (unless there's another nested .asmdef, obvs)
        if (assetPath.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase)
            || assetPath.EndsWith(".asmref", StringComparison.OrdinalIgnoreCase))
        {
          // This call is very expensive when working with a very large project (e.g. called for 50,000+ assets), hence
          // the approach of working with assembly definition root folders. We don't need a real script file to get the
          // assembly DLL name
          var assemblyDllName = m_AssemblyNameProvider.GetAssemblyNameFromScriptPath(assetPath + ".cs");
          assemblyDllNames.Insert(Path.GetDirectoryName(assetPath), assemblyDllName);
        }

        interestingAssets.Add(assetPath);
      }

      const string fallbackAssemblyDllName = "Assembly-CSharp.dll";

      var assetsByAssemblyDll = new Dictionary<string, List<string>>();
      foreach (var asset in interestingAssets)
      {
        // TODO: Can we remove folders from generated projects?
        // Why do we add them? We get an asset for every folder, including intermediate folders. We add folders that
        // contain assets that we don't add to project files, so they appear empty. Adding them to the project file does
        // not give us anything special - they appear as a folder in the Solution Explorer, so we can right click and
        // add a file, but we could also "Show All Files" and do the same. Equally, Rider defaults to the Unity Explorer
        // view, which shows all files and folders by default.
        // We gain nothing by adding folders, and for very large projects, it can be very expensive to discover what
        // project they should be added to, since most paths will be _above_ asmdef files, or inside Assets (which
        // requires the full expensive check due to Editor, Resources, etc.)
        // (E.g. an example large project with 45,600 assets, 5,000 are folders and only 2,500 are useful assets)
        if (AssetDatabase.IsValidFolder(asset))
        {
          // var assemblyDllName = assemblyDllNames.FindClosestMatch(asset);
          // if (string.IsNullOrEmpty(assemblyDllName))
          // {
          //   // Can't find it in trie (Assembly-CSharp and related projects don't have .asmdef files)
          //   assemblyDllName = m_AssemblyNameProvider.GetAssemblyNameFromScriptPath($"{asset}/asset.cs");
          // }
          // if (string.IsNullOrEmpty(assemblyDllName))
          //  assemblyDllName = fallbackAssemblyDllName;
//
          // if (!stringBuilders.TryGetValue(assemblyDllName, out var projectBuilder))
          // {
          //   projectBuilder = new StringBuilder();
          //   stringBuilders[assemblyDllName] = projectBuilder;
          //}
//
          // projectBuilder.Append("     <Folder Include=\"")
          //   .Append(m_FileIOProvider.EscapedRelativePathFor(asset, ProjectDirectoryWithSlash))
          //   .Append("\" />")
          //   .AppendLine();
        }
        else
        {
          if (!asset.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && IsSupportedExtension(Path.GetExtension(asset)))
          {
            var assemblyDllName = assemblyDllNames.FindClosestMatch(asset);
            if (string.IsNullOrEmpty(assemblyDllName))
            {
              // Can't find it in trie (Assembly-CSharp and related projects don't have .asmdef files)
              assemblyDllName = m_AssemblyNameProvider.GetAssemblyNameFromScriptPath($"{asset}.cs");
            }
            if (string.IsNullOrEmpty(assemblyDllName))
              assemblyDllName = fallbackAssemblyDllName;

            if (!assetsByAssemblyDll.TryGetValue(assemblyDllName, out var assets))
            {
              assets = new List<string>();
              assetsByAssemblyDll[assemblyDllName] = assets;
            }

            assets.Add(m_FileIOProvider.EscapedRelativePathFor(asset, ProjectDirectoryWithSlash));
          }
        }
      }

      var assetsByAssemblyName = new Dictionary<string, List<string>>(assetsByAssemblyDll.Count);
      foreach (var entry in assetsByAssemblyDll)
      {
        var assemblyName = FileSystemUtil.FileNameWithoutExtension(entry.Key);
        assetsByAssemblyName[assemblyName] = entry.Value;
      }

      return assetsByAssemblyName;
    }

    private void SyncProject(StringBuilder stringBuilder, ProjectPart island, AssemblyUsage assemblyUsage, Type[] types)
    {
      SyncProjectFileIfNotChanged(
        ProjectFile(island),
        ProjectText(stringBuilder, island, assemblyUsage),
        types);
    }

    private void SyncProjectFileIfNotChanged(string path, string newContents, Type[] types)
    {
      if (Path.GetExtension(path) == ".csproj")
      {
        newContents = OnGeneratedCSProject(path, newContents, types);
      }

      SyncFileIfNotChanged(path, newContents);
    }

    private void SyncSolutionFileIfNotChanged(string path, string newContents, Type[] types)
    {
      newContents = OnGeneratedSlnSolution(path, newContents, types);

      SyncFileIfNotChanged(path, newContents);
    }

    private static void OnGeneratedCSProjectFiles(Type[] types)
    {
      foreach (var type in types)
      {
        var method = type.GetMethod("OnGeneratedCSProjectFiles",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
          System.Reflection.BindingFlags.Static);
        if (method == null)
        {
          continue;
        }

        Debug.LogWarning("OnGeneratedCSProjectFiles is not supported.");
        // RIDER-51958
        //method.Invoke(null, args);
      }
    }

    public static Type[] GetAssetPostprocessorTypes()
    {
      return TypeCache.GetTypesDerivedFrom<AssetPostprocessor>().ToArray(); // doesn't find types from EditorPlugin, which is fine
    }

    private static bool OnPreGeneratingCSProjectFiles(Type[] types)
    {
      var result = false;
      foreach (var type in types)
      {
        var args = new object[0];
        var method = type.GetMethod("OnPreGeneratingCSProjectFiles",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
          System.Reflection.BindingFlags.Static);
        if (method == null)
        {
          continue;
        }

        var returnValue = method.Invoke(null, args);
        if (method.ReturnType == typeof(bool))
        {
          result |= (bool)returnValue;
        }
      }

      return result;
    }

    private static string OnGeneratedCSProject(string path, string content, Type[] types)
    {
      foreach (var type in types)
      {
        var args = new[] { path, content };
        var method = type.GetMethod("OnGeneratedCSProject",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
          System.Reflection.BindingFlags.Static);
        if (method == null)
        {
          continue;
        }

        var returnValue = method.Invoke(null, args);
        if (method.ReturnType == typeof(string))
        {
          content = (string)returnValue;
        }
      }

      return content;
    }

    private static string OnGeneratedSlnSolution(string path, string content, Type[] types)
    {
      foreach (var type in types)
      {
        var args = new[] { path, content };
        var method = type.GetMethod("OnGeneratedSlnSolution",
          System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
          System.Reflection.BindingFlags.Static);
        if (method == null)
        {
          continue;
        }

        var returnValue = method.Invoke(null, args);
        if (method.ReturnType == typeof(string))
        {
          content = (string)returnValue;
        }
      }

      return content;
    }

    private void SyncFileIfNotChanged(string path, string newContents)
    {
      if (HasChanged(path, newContents))
        m_FileIOProvider.WriteAllText(path, newContents);
    }

    private static char[] _buffer = null;

    private bool HasChanged(string path, string newContents)
    {
      try
      {
        if (!m_FileIOProvider.Exists(path))
          return true;

        const int bufferSize = 100 * 1024; // 100kb - big enough to read most project files in a single read

        if (_buffer == null)
          _buffer = new char[bufferSize];

        using (var reader = m_FileIOProvider.GetReader(path))
        {
          int read, offset = 0;
          do
          {
            read = reader.ReadBlock(_buffer, 0, _buffer.Length);
            for (var i = 0; i < read; i++)
            {
              if (_buffer[i] != newContents[offset + i])
                return true;
            }

            offset += read;
          } while (read > 0);

          var isSame = offset == newContents.Length;
          return !isSame;
        }
      }
      catch (Exception exception)
      {
        Debug.LogException(exception);
        return true;
      }
    }

    private string ProjectText(StringBuilder projectBuilder, ProjectPart assembly, AssemblyUsage assemblyUsage)
    {
      var responseFilesData = assembly.GetResponseFileData(m_AssemblyNameProvider, ProjectDirectory);

      ProjectHeader(projectBuilder, assembly, responseFilesData);

      projectBuilder.AppendLine("  <ItemGroup>");
      foreach (var file in assembly.SourceFiles)
      {
        var fullFile = m_FileIOProvider.EscapedRelativePathFor(file, ProjectDirectory);
        projectBuilder.Append("    <Compile Include=\"").Append(fullFile).AppendLine("\" />");
      }

      foreach (var additionalAsset in (IEnumerable<string>)assembly.AdditionalAssets ?? Array.Empty<string>())
        projectBuilder.Append("    <None Include=\"").Append(additionalAsset).AppendLine("\" />");

      var binaryReferences = new HashSet<string>(assembly.CompiledAssemblyReferences);
      foreach (var responseFileData in responseFilesData)
        binaryReferences.UnionWith(responseFileData.FullPathReferences);
      foreach (var assemblyReference in assembly.AssemblyReferences)
      {
        if (assemblyUsage.IsPrecompiledAssembly(assemblyReference))
          binaryReferences.Add(assemblyReference.outputPath);
      }

      foreach (var reference in binaryReferences)
      {
        var escapedFullPath = GetNormalisedAssemblyPath(reference);
        var assemblyName = GetAssemblyNameFromPath(reference);
        projectBuilder
          .Append("    <Reference Include=\"").Append(assemblyName).AppendLine("\">")
          .Append("      <HintPath>").Append(escapedFullPath).AppendLine("</HintPath>")
          .AppendLine("    </Reference>");
      }

      if (0 < assembly.AssemblyReferences.Length)
      {
        projectBuilder
          .AppendLine("  </ItemGroup>")
          .AppendLine("  <ItemGroup>");

        foreach (var reference in assembly.AssemblyReferences)
        {
          if (assemblyUsage.IsProjectAssembly(reference))
{
            var name = m_AssemblyNameProvider.GetProjectName(reference.name, reference.defines);
            projectBuilder
              .Append("    <ProjectReference Include=\"").Append(name).AppendLine(".csproj\">")
              .Append("      <Project>{").Append(ProjectGuid(name)).AppendLine("}</Project>")
              .Append("      <Name>").Append(name).AppendLine("</Name>")
              .AppendLine("    </ProjectReference>");
          }
        }
      }

      projectBuilder
        .AppendLine("  </ItemGroup>")
        .AppendLine("  <Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />")
        .AppendLine(
          "  <!-- To modify your build process, add your task inside one of the targets below and uncomment it.")
        .AppendLine("       Other similar extension points exist, see Microsoft.Common.targets.")
        .AppendLine("  <Target Name=\"BeforeBuild\">")
        .AppendLine("  </Target>")
        .AppendLine("  <Target Name=\"AfterBuild\">")
        .AppendLine("  </Target>")
        .AppendLine("  -->")
        .AppendLine("</Project>");

      return projectBuilder.ToString();
    }

    private string ProjectFile(ProjectPart projectPart)
    {
      return Path.Combine(ProjectDirectory, $"{m_AssemblyNameProvider.GetProjectName(projectPart.Name, projectPart.Defines)}.csproj");
    }

    public string SolutionFile()
    {
      return Path.Combine(ProjectDirectory, $"Unity.sln");
    }

    private void ProjectHeader(StringBuilder stringBuilder, ProjectPart assembly, List<ResponseFileData> responseFilesData)
    {
      var responseFilesDataArgs = GetOtherArgumentsFromResponseFilesData(responseFilesData);
      stringBuilder
        .AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>")
        .AppendLine(
          "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">")
        .AppendLine("  <PropertyGroup>")
        .Append("    <LangVersion>").Append(GetLangVersion(responseFilesDataArgs["langversion"], assembly)).AppendLine("</LangVersion>")
        .AppendLine(
          "    <_TargetFrameworkDirectories>non_empty_path_generated_by_unity.rider.package</_TargetFrameworkDirectories>")
        .AppendLine(
          "    <_FullFrameworkReferenceAssemblyPaths>non_empty_path_generated_by_unity.rider.package</_FullFrameworkReferenceAssemblyPaths>")
        .AppendLine("    <DisableHandlePackageFileConflicts>true</DisableHandlePackageFileConflicts>");

      var rulesetPaths = GetRoslynAnalyzerRulesetPaths(assembly, responseFilesDataArgs);
      foreach (var path in rulesetPaths)
        stringBuilder.Append("    <CodeAnalysisRuleSet>").Append(path).AppendLine("</CodeAnalysisRuleSet>");

      stringBuilder
        .AppendLine("  </PropertyGroup>")
        .AppendLine("  <PropertyGroup>")
        .AppendLine("    <Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>")
        .AppendLine("    <Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>")
        .AppendLine("    <ProductVersion>10.0.20506</ProductVersion>")
        .AppendLine("    <SchemaVersion>2.0</SchemaVersion>")
        .Append("    <RootNamespace>").Append(assembly.RootNamespace).AppendLine("</RootNamespace>")
        .Append("    <ProjectGuid>{").Append(ProjectGuid(m_AssemblyNameProvider.GetProjectName(assembly.Name, assembly.Defines))).AppendLine("}</ProjectGuid>")
        .AppendLine(
          "    <ProjectTypeGuids>{E097FAD1-6243-4DAD-9C02-E9B9EFC3FFC1};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>")
        .AppendLine("    <OutputType>Library</OutputType>")
        .AppendLine("    <AppDesignerFolder>Properties</AppDesignerFolder>")
        .Append("    <AssemblyName>").Append(assembly.Name).AppendLine("</AssemblyName>")
        .AppendLine("    <TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>")
        .AppendLine("    <FileAlignment>512</FileAlignment>")
        .AppendLine("    <BaseDirectory>.</BaseDirectory>")
        .AppendLine("  </PropertyGroup>")
        .AppendLine("  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">")
        .AppendLine("    <DebugSymbols>true</DebugSymbols>")
        .AppendLine("    <DebugType>full</DebugType>")
        .AppendLine("    <Optimize>false</Optimize>")
        .Append("    <OutputPath>").Append(assembly.OutputPath).AppendLine("</OutputPath>");

      var defines = new HashSet<string>(assembly.Defines);
      foreach (var responseFileData in responseFilesData)
        defines.UnionWith(responseFileData.Defines);
      stringBuilder
        .Append("    <DefineConstants>").CompatibleAppendJoin(';', defines).AppendLine("</DefineConstants>")
        .AppendLine("    <ErrorReport>prompt</ErrorReport>");

      var warningLevel = responseFilesDataArgs["warn"].Concat(responseFilesDataArgs["w"]).Distinct().FirstOrDefault();
      stringBuilder
        .Append("    <WarningLevel>").Append(!string.IsNullOrWhiteSpace(warningLevel) ? warningLevel : "4").AppendLine("</WarningLevel>")
        .Append("    <NoWarn>").CompatibleAppendJoin(',', GetNoWarn(responseFilesDataArgs["nowarn"].ToList())).AppendLine("</NoWarn>")
        .Append("    <AllowUnsafeBlocks>").Append(assembly.CompilerOptions.AllowUnsafeCode | responseFilesData.Any(x => x.Unsafe)).AppendLine("</AllowUnsafeBlocks>");

      AppendWarningAsError(stringBuilder, responseFilesDataArgs["warnaserror"],
        responseFilesDataArgs["warnaserror-"], responseFilesDataArgs["warnaserror+"]);

      // TODO: Can we have multiple documentation files in a project file?
      foreach (var docFile in responseFilesDataArgs["doc"])
        stringBuilder.Append("    <DocumentationFile>").Append(docFile).AppendLine("</DocumentationFile>");

      var nullable = responseFilesDataArgs["nullable"].FirstOrDefault();
      if (!string.IsNullOrEmpty(nullable))
        stringBuilder.Append("    <Nullable>").Append(nullable).AppendLine("</Nullable>");

      stringBuilder
        .AppendLine("  </PropertyGroup>")
        .AppendLine("  <PropertyGroup>")
        .AppendLine("    <NoConfig>true</NoConfig>")
        .AppendLine("    <NoStdLib>true</NoStdLib>")
        .AppendLine("    <AddAdditionalExplicitAssemblyReferences>false</AddAdditionalExplicitAssemblyReferences>")
        .AppendLine("    <ImplicitlyExpandNETStandardFacades>false</ImplicitlyExpandNETStandardFacades>")
        .AppendLine("    <ImplicitlyExpandDesignTimeFacades>false</ImplicitlyExpandDesignTimeFacades>")
        .AppendLine("  </PropertyGroup>");

      var analyzers = GetRoslynAnalyzers(assembly, responseFilesDataArgs);
      if (analyzers.Length > 0)
      {
        stringBuilder.AppendLine("  <ItemGroup>");
        foreach (var analyzer in analyzers)
          stringBuilder.AppendLine($"    <Analyzer Include=\"{analyzer.NormalizePath()}\" />");
        stringBuilder.AppendLine("  </ItemGroup>");
      }

      var additionalFiles = GetRoslynAdditionalFiles(assembly, responseFilesDataArgs);
      if (additionalFiles.Length > 0)
      {
        stringBuilder.AppendLine("  <ItemGroup>");
        foreach (var additionalFile in additionalFiles)
          stringBuilder.AppendLine($"    <AdditionalFiles Include=\"{additionalFile}\" />");
        stringBuilder.AppendLine("  </ItemGroup>");
      }

      var configFile = GetGlobalAnalyzerConfigFile(assembly);
      if (!string.IsNullOrEmpty(configFile))
      {
        stringBuilder
          .AppendLine("  <ItemGroup>")
          .Append("    <GlobalAnalyzerConfigFiles Include=\"").Append(configFile).AppendLine("\" />")
          .AppendLine("  </ItemGroup>");
      }
    }

    private static string GetGlobalAnalyzerConfigFile(ProjectPart assembly)
    {
      var configFile = string.Empty;
#if UNITY_2021_3 // https://github.com/JetBrains/resharper-unity/issues/2401
      var type = assembly.CompilerOptions.GetType();
      var propertyInfo = type.GetProperty("AnalyzerConfigPath");
      if (propertyInfo != null && propertyInfo.GetValue(assembly.CompilerOptions) is string value)
      {
        configFile = value;
      }
#elif UNITY_2022_2_OR_NEWER
        configFile = assembly.CompilerOptions.AnalyzerConfigPath; // https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Compilation.ScriptCompilerOptions.AnalyzerConfigPath.html
#endif


      return configFile;
    }

    private static string[] GetRoslynAdditionalFiles(ProjectPart assembly, ILookup<string, string> otherResponseFilesData)
    {
      var additionalFilePathsFromCompilationPipeline = Array.Empty<string>();
#if UNITY_2021_3 // https://github.com/JetBrains/resharper-unity/issues/2401
      var type = assembly.CompilerOptions.GetType();
      var propertyInfo = type.GetProperty("RoslynAdditionalFilePaths");
      if (propertyInfo != null && propertyInfo.GetValue(assembly.CompilerOptions) is string[] value)
      {
        additionalFilePathsFromCompilationPipeline = value;
      }
#elif UNITY_2022_2_OR_NEWER // https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Compilation.ScriptCompilerOptions.RoslynAdditionalFilePaths.html
        additionalFilePathsFromCompilationPipeline = assembly.CompilerOptions.RoslynAdditionalFilePaths;
#endif
      return otherResponseFilesData["additionalfile"]
        .SelectMany(x=>x.Split(';'))
        .Concat(additionalFilePathsFromCompilationPipeline)
        .Distinct().ToArray();
    }

    string[] GetRoslynAnalyzers(ProjectPart assembly, ILookup<string, string> otherResponseFilesData)
    {
#if UNITY_2020_2_OR_NEWER
      return otherResponseFilesData["analyzer"].Concat(otherResponseFilesData["a"])
        .SelectMany(x=>x.Split(';'))
#if !ROSLYN_ANALYZER_FIX
        .Concat(m_AssemblyNameProvider.GetRoslynAnalyzerPaths())
#else
        .Concat(assembly.CompilerOptions.RoslynAnalyzerDllPaths)
#endif
        .Select(GetNormalisedAssemblyPath)
        .Distinct()
        .ToArray();
#else
      return otherResponseFilesData["analyzer"].Concat(otherResponseFilesData["a"])
        .SelectMany(x=>x.Split(';'))
        .Distinct()
        .Select(GetNormalisedAssemblyPath)
        .ToArray();
#endif
    }

    private IEnumerable<string> GetRoslynAnalyzerRulesetPaths(ProjectPart assembly, ILookup<string, string> otherResponseFilesData)
    {
      var paths = new HashSet<string>(otherResponseFilesData["ruleset"]);
#if UNITY_2020_2_OR_NEWER
      if (!string.IsNullOrEmpty(assembly.CompilerOptions.RoslynAnalyzerRulesetPath))
        paths.Add(assembly.CompilerOptions.RoslynAnalyzerRulesetPath);
#endif

      return paths.Select(GetNormalisedAssemblyPath);
    }

    private static void AppendWarningAsError(StringBuilder stringBuilder,
      IEnumerable<string> args, IEnumerable<string> argsMinus, IEnumerable<string> argsPlus)
    {
      var treatWarningsAsErrors = false;
      var warningIds = new List<string>();
      var notWarningIds = new List<string>(argsMinus);

      foreach (var s in args)
      {
        if (s == "+" || s == "") treatWarningsAsErrors = true;
        else if (s == "-") treatWarningsAsErrors = false;
        else warningIds.Add(s);
      }

      warningIds.AddRange(argsPlus);

      stringBuilder.Append("    <TreatWarningsAsErrors>").Append(treatWarningsAsErrors) .AppendLine("</TreatWarningsAsErrors>");
      if (warningIds.Count > 0)
        stringBuilder.Append("    <WarningsAsErrors>").CompatibleAppendJoin(';', warningIds).AppendLine("</WarningsAsErrors>");
      if (notWarningIds.Count > 0)
        stringBuilder.Append("    <WarningsNotAsErrors>").CompatibleAppendJoin(';', notWarningIds) .AppendLine("</WarningsNotAsErrors>");
    }

    private void SyncSolution(StringBuilder stringBuilder, List<ProjectPart> islands, Type[] types)
    {
      SyncSolutionFileIfNotChanged(SolutionFile(), SolutionText(stringBuilder, islands), types);
    }

    private string SolutionText(StringBuilder stringBuilder, List<ProjectPart> islands)
    {
      stringBuilder
        .AppendLine()
        .AppendLine("Microsoft Visual Studio Solution File, Format Version 11.00")
        .AppendLine("# Visual Studio 2010");
      foreach (var island in islands)
      {
        var projectName = m_AssemblyNameProvider.GetProjectName(island.Name, island.Defines);

        // GUID is for C# class libraries
        stringBuilder
          .Append("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"")
          .Append(island.Name)
          .Append("\", \"")
          .Append(projectName)
          .Append(".csproj\", \"{")
          .Append(ProjectGuid(projectName))
          .AppendLine("}\"")
          .AppendLine("EndProject");
      }

      stringBuilder.AppendLine("Global")
        .AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution")
        .AppendLine("\t\tDebug|Any CPU = Debug|Any CPU")
        .AppendLine("\tEndGlobalSection")
        .AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

      foreach (var island in islands)
      {
        var projectGuid = ProjectGuid(m_AssemblyNameProvider.GetProjectName(island.Name, island.Defines));

        stringBuilder
          .Append("\t\t{").Append(projectGuid).AppendLine("}.Debug|Any CPU.ActiveCfg = Debug|Any CPU")
          .Append("\t\t{").Append(projectGuid).AppendLine("}.Debug|Any CPU.Build.0 = Debug|Any CPU");
      }

      stringBuilder.AppendLine("\tEndGlobalSection")
        .AppendLine("\tGlobalSection(SolutionProperties) = preSolution")
        .AppendLine("\t\tHideSolutionNode = FALSE")
        .AppendLine("\tEndGlobalSection")
        .AppendLine("EndGlobal");

      return stringBuilder.ToString();
    }

    private static ILookup<string, string> GetOtherArgumentsFromResponseFilesData(List<ResponseFileData> responseFilesData)
    {
      var paths = responseFilesData.SelectMany(x =>
        {
          return x.OtherArguments
            .Where(a => a.StartsWith("/", StringComparison.Ordinal) || a.StartsWith("-", StringComparison.Ordinal))
            .Select(b =>
            {
              var index = b.IndexOf(":", StringComparison.Ordinal);
              if (index > 0 && b.Length > index)
              {
                var key = b.Substring(1, index - 1);
                return new KeyValuePair<string, string>(key, b.Substring(index + 1));
              }

              const string warnaserror = "warnaserror";
              if (b.Substring(1).StartsWith(warnaserror, StringComparison.Ordinal))
              {
                return new KeyValuePair<string, string>(warnaserror, b.Substring(warnaserror.Length + 1));
              }
              const string nullable = "nullable";
              if (b.Substring(1).StartsWith(nullable, StringComparison.Ordinal))
              {
                var res = b.Substring(nullable.Length + 1);
                if (string.IsNullOrWhiteSpace(res) || res.Equals("+"))
                  res = "enable";
                else if (res.Equals("-"))
                  res = "disable";
                return new KeyValuePair<string, string>(nullable, res);
              }

              return default;
            });
        })
        .Distinct()
        .ToLookup(o => o.Key, pair => pair.Value);
      return paths;
    }

    private string GetLangVersion(IEnumerable<string> langVersionList, ProjectPart assembly)
    {
      var langVersion = langVersionList.FirstOrDefault();
      if (!string.IsNullOrWhiteSpace(langVersion))
        return langVersion;
#if UNITY_2020_2_OR_NEWER
      return assembly.CompilerOptions.LanguageVersion;
#else
      return "latest";
#endif
    }

    public static IEnumerable<string> GetNoWarn(List<string> codes)
    {
#if UNITY_2019_4 || UNITY_2020_1 // RIDER-77206 Unity 2020.1.3 'PlayerSettings' does not contain a definition for 'suppressCommonWarnings'
      var type = typeof(PlayerSettings);
      var propertyInfo = type.GetProperty("suppressCommonWarnings");
      if (propertyInfo != null && propertyInfo.GetValue(null) is bool && (bool)propertyInfo.GetValue(null))
      {
        codes.AddRange(new[] {"0169", "0649"});
      }
#elif UNITY_2020_2_OR_NEWER
      if (PlayerSettings.suppressCommonWarnings)
        codes.AddRange(new[] {"0169", "0649"});
#endif

      return codes.Distinct();
    }

    private string ProjectGuid(string name)
    {
      if (!m_ProjectGuids.TryGetValue(name, out var guid))
      {
        guid = m_GUIDGenerator.ProjectGuid(m_ProjectName + name);
        m_ProjectGuids.Add(name, guid);
      }

      return guid;
    }

    private string GetNormalisedAssemblyPath(string path)
    {
      if (!m_NormalisedPaths.TryGetValue(path, out var normalisedPath))
      {
        normalisedPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
        normalisedPath = SecurityElement.Escape(normalisedPath).NormalizePath();
        m_NormalisedPaths.Add(path, normalisedPath);
      }

      return normalisedPath;
    }

    private string GetAssemblyNameFromPath(string path)
    {
      if (!m_AssemblyNames.TryGetValue(path, out var name))
      {
        name = FileSystemUtil.FileNameWithoutExtension(path);
        m_AssemblyNames.Add(path, name);
      }

      return name;
    }
  }

  internal class FilePathTrie<TData>
  {
    private static readonly char[] Separators = { '\\', '/' };

    private readonly TrieNode m_Root = new TrieNode();

    private class TrieNode
    {
      public Dictionary<string, TrieNode> Children;
      public TData Data;
    }

    public void Insert(string filePath, TData data)
    {
      var parts = filePath.Split(Separators);

      var node = m_Root;
      foreach (var part in parts)
      {
        if (node.Children == null)
          node.Children = new Dictionary<string, TrieNode>(StringComparer.OrdinalIgnoreCase);
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
        if (!node.Children.ContainsKey(part))
          node.Children[part] = new TrieNode();

        node = node.Children[part];
      }

      node.Data = data;
    }

    public TData FindClosestMatch(string filePath)
    {
      var parts = filePath.Split(Separators);

      var node = m_Root;
      foreach (var part in parts)
      {
        if (node.Children != null && node.Children.TryGetValue(part, out var next))
          node = next;
        else
          break;
      }

      return node.Data;
    }
  }
}
