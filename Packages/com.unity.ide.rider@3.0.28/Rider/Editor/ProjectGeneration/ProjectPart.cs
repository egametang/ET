using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal class ProjectPart
  {
    public string Name { get; }
    public string OutputPath { get; }
    public Assembly Assembly { get; }
    public List<string> AdditionalAssets { get; }
    public string[] SourceFiles { get; }
    public string RootNamespace { get; }
    public Assembly[] AssemblyReferences { get; }
    public string[] CompiledAssemblyReferences { get; }
    public string[] Defines { get; }
    public ScriptCompilerOptions CompilerOptions { get; }

    public ProjectPart(string name, Assembly assembly, List<string> additionalAssets)
    {
      Name = name;
      Assembly = assembly;
      AdditionalAssets = additionalAssets;
      OutputPath = assembly != null ? assembly.outputPath : "Temp/Bin/Debug";
      SourceFiles = assembly != null ? assembly.sourceFiles : Array.Empty<string>();
#if UNITY_2020_2_OR_NEWER
      RootNamespace = assembly != null ? assembly.rootNamespace : string.Empty;
#else
      RootNamespace = UnityEditor.EditorSettings.projectGenerationRootNamespace;
#endif
      AssemblyReferences = assembly != null ? assembly.assemblyReferences : Array.Empty<Assembly>();
      CompiledAssemblyReferences = assembly != null ? assembly.compiledAssemblyReferences : Array.Empty<string>();
      Defines = assembly != null ? assembly.defines : Array.Empty<string>();
      CompilerOptions = assembly != null ? assembly.compilerOptions : new ScriptCompilerOptions();
    }

    public List<ResponseFileData> GetResponseFileData(IAssemblyNameProvider assemblyNameProvider, string projectDirectory)
    {
      if (Assembly == null)
        return new List<ResponseFileData>();

      var data = new List<ResponseFileData>();
      foreach (var responseFile in Assembly.compilerOptions.ResponseFiles)
      {
        var responseFileData = assemblyNameProvider.ParseResponseFile(responseFile, projectDirectory, Assembly.compilerOptions.ApiCompatibilityLevel);
        foreach (var error in responseFileData.Errors)
          Debug.Log($"{responseFile} Parse Error : {error}");
        data.Add(responseFileData);
      }

      return data;
    }
  }
}