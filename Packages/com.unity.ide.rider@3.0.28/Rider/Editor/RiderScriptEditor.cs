using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Rider.PathLocator;
using Packages.Rider.Editor.ProjectGeneration;
using Packages.Rider.Editor.Util;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using OperatingSystemFamily = UnityEngine.OperatingSystemFamily;

namespace Packages.Rider.Editor
{
  [InitializeOnLoad]
  internal class RiderScriptEditor : IExternalCodeEditor
  {
    IDiscovery m_Discoverability;
    static IGenerator m_ProjectGeneration;
    RiderInitializer m_Initiliazer = new RiderInitializer();
    static RiderScriptEditor m_RiderScriptEditor;

    static RiderScriptEditor()
    {
      try
      {
        // todo: make ProjectGeneration lazy
        var projectGeneration = new ProjectGeneration.ProjectGeneration();
        m_RiderScriptEditor = new RiderScriptEditor(new Discovery(), projectGeneration);
        // preserve the order here, otherwise on startup, project generation Sync would happen multiple times
        CodeEditor.Register(m_RiderScriptEditor);
        InitializeInternal(CurrentEditor);
        // end of "preserve the order here"
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }
    }

    private static void ShowWarningOnUnexpectedScriptEditor(string path)
    {
      // Show warning, when Unity was started from Rider, but external editor is different https://github.com/JetBrains/resharper-unity/issues/1127
      try
      {
        var args = Environment.GetCommandLineArgs();
        var commandlineParser = new CommandLineParser(args);
        if (commandlineParser.Options.ContainsKey("-riderPath"))
        {
          var originRiderPath = commandlineParser.Options["-riderPath"];
          var originRealPath = GetEditorRealPath(originRiderPath);
          var originVersion = Discovery.RiderPathLocator.GetBuildNumber(originRealPath);
          var version = Discovery.RiderPathLocator.GetBuildNumber(path);
          if (originVersion != null && originVersion != version)
          {
            Debug.LogWarning("Unity was started by a version of Rider that is not the current default external editor. Advanced integration features cannot be enabled.");
            Debug.Log($"Unity was started by Rider {originVersion}, but external editor is set to: {path}");
          }
        }
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }
    }

    internal static string GetEditorRealPath(string path)
    {
      if (string.IsNullOrEmpty(path))
        return path;

      if (!FileSystemUtil.EditorPathExists(path))
        return path;

      if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Windows)
      {
        var realPath = FileSystemUtil.GetFinalPathName(path);

        // case of snap installation
        if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Linux)
        {
          if (new FileInfo(path).Name.ToLowerInvariant() == "rider" &&
              new FileInfo(realPath).Name.ToLowerInvariant() == "snap")
          {
            var snapInstallPath = "/snap/rider/current/bin/rider.sh";
            if (new FileInfo(snapInstallPath).Exists)
              return snapInstallPath;
          }
        }

        // in case of symlink
        return realPath;
      }

      return new FileInfo(path).FullName;
    }

    public RiderScriptEditor(IDiscovery discovery, IGenerator projectGeneration)
    {
      m_Discoverability = discovery;
      m_ProjectGeneration = projectGeneration;
    }

    public void OnGUI()
    {
      GUILayout.BeginHorizontal();

      var style = GUI.skin.label;
      var text = "Customize handled extensions in";
      EditorGUILayout.LabelField(text, style, GUILayout.Width(style.CalcSize(new GUIContent(text)).x));

      if (PluginSettings.LinkButton("Project Settings | Editor | Additional extensions to include"))
      {
        SettingsService.OpenProjectSettings("Project/Editor"); // how do I focus "Additional extensions to include"?
      }

      GUILayout.EndHorizontal();

      EditorGUILayout.LabelField("Generate .csproj files for:");
      EditorGUI.indentLevel++;
      SettingsButton(ProjectGenerationFlag.Embedded, "Embedded packages", "");
      SettingsButton(ProjectGenerationFlag.Local, "Local packages", "");
      SettingsButton(ProjectGenerationFlag.Registry, "Registry packages", "");
      SettingsButton(ProjectGenerationFlag.Git, "Git packages", "");
      SettingsButton(ProjectGenerationFlag.BuiltIn, "Built-in packages", "");
#if UNITY_2019_3_OR_NEWER
      SettingsButton(ProjectGenerationFlag.LocalTarBall, "Local tarball", "");
#endif
      SettingsButton(ProjectGenerationFlag.Unknown, "Packages from unknown sources", "");
      SettingsButton(ProjectGenerationFlag.PlayerAssemblies, "Player projects", "For each player project generate an additional csproj with the name 'project-player.csproj'");
      RegenerateProjectFiles();
      EditorGUI.indentLevel--;
    }

    void RegenerateProjectFiles()
    {
      var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(new GUILayoutOption[] {}));
      rect.width = 252;
      if (GUI.Button(rect, "Regenerate project files"))
      {
        m_ProjectGeneration.Sync();
      }
    }

    void SettingsButton(ProjectGenerationFlag preference, string guiMessage, string toolTip)
    {
      var prevValue = m_ProjectGeneration.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(preference);
      var newValue = EditorGUILayout.Toggle(new GUIContent(guiMessage, toolTip), prevValue);
      if (newValue != prevValue)
      {
        m_ProjectGeneration.AssemblyNameProvider.ToggleProjectGeneration(preference);
      }
    }

    public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles,
      string[] importedFiles)
    {
      m_ProjectGeneration.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles),
        importedFiles);
    }
    
    public void SyncAll()
    {
      m_ProjectGeneration.Sync();
    }
    
    [UsedImplicitly]
    public static void SyncSolution() // generate-the-sln-file-via-script-or-command-line
    {
      m_ProjectGeneration.Sync();
    }
    
    [UsedImplicitly] // called from Rider EditorPlugin with reflection
    public static void SyncIfNeeded(bool checkProjectFiles)
    {
      AssetDatabase.Refresh();
      m_ProjectGeneration.SyncIfNeeded(new string[] { }, new string[] { }, checkProjectFiles);
    }
    
    [UsedImplicitly]
    public static void SyncSolutionAndOpenExternalEditor()
    {
      m_ProjectGeneration.Sync();
      CodeEditor.CurrentEditor.OpenProject();
    }

    /// <summary>
    /// In 2020.x is called each time ExternalEditor is changed
    /// In 2021.x+ is called each time ExternalEditor is changed and also on each appdomain reload
    /// </summary>
    /// <param name="editorInstallationPath"></param>
    public void Initialize(string editorInstallationPath)
    {
      var prevEditorVersion = RiderScriptEditorData.instance.prevEditorBuildNumber.ToVersion();
      
      RiderScriptEditorData.instance.Invalidate(editorInstallationPath, true);

      // previous editor did not have EditorPlugin
      // just load the EditorPlugin
      if (EditorPluginInterop.EditorPluginAssembly == null) 
      {
        InitializeInternal(editorInstallationPath);
        return;
      }
      
      // previous editor was Rider with a different version
      // need to load new Editor plugin
      if (prevEditorVersion != null && prevEditorVersion != RiderScriptEditorData.instance.editorBuildNumber.ToVersion()) // in Unity 2019.3 any change in preference causes `Initialize` call
      {
#if UNITY_2019_3_OR_NEWER
        EditorUtility.RequestScriptReload(); // EditorPlugin would get loaded
#else 
        UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
#endif
      }
    }

    private static void InitializeInternal(string currentEditorPath)
    {
      var path = GetEditorRealPath(currentEditorPath);

      if (IsRiderOrFleetInstallation(path))
      {
        var installations = new HashSet<RiderPathLocator.RiderInfo>();
        if (RiderScriptEditorData.instance.installations != null)
        {
          foreach (var info in RiderScriptEditorData.instance.installations)
          {
            installations.Add(info);
          }
        }
        
        if (!RiderScriptEditorData.instance.initializedOnce || !FileSystemUtil.EditorPathExists(path))
        {
          foreach (var item in Discovery.RiderPathLocator.GetAllRiderPaths())
          {
            installations.Add(item);
          }
          // is likely outdated
          if (installations.All(a => GetEditorRealPath(a.Path) != path))
          {
            if (Discovery.RiderPathLocator.GetIsToolbox(path)) // is toolbox 1.x - update
            {
              var toolboxInstallations = installations.Where(a => a.IsToolbox).ToArray();
              if (toolboxInstallations.Any())
              {
                var newEditor = toolboxInstallations.OrderBy(a => a.BuildNumber).Last().Path;
                CodeEditor.SetExternalScriptEditor(newEditor);
                path = newEditor;
              }
              else if (installations.Any())
              {
                var newEditor = installations.OrderBy(a => a.BuildNumber).Last().Path;
                CodeEditor.SetExternalScriptEditor(newEditor);
                path = newEditor;
              }
            }
            else if (installations.Any()) // is non toolbox 1.x
            {
              if (!FileSystemUtil.EditorPathExists(path)) // previously used rider was removed
              {
                var newEditor = installations.OrderBy(a => a.BuildNumber).Last().Path;
                CodeEditor.SetExternalScriptEditor(newEditor);
                path = newEditor;
              }
              else // notify
              {
                var newEditorName = installations.OrderBy(a => a.BuildNumber).Last().Presentation;
                Debug.LogWarning($"Consider updating External Editor in Unity to {newEditorName}.");
              }
            }
          }

          ShowWarningOnUnexpectedScriptEditor(path);
          RiderScriptEditorData.instance.initializedOnce = true;
        }

        if (FileSystemUtil.EditorPathExists(path) && installations.All(a => a.Path != path)) // custom location
        {
          var info = new RiderPathLocator.RiderInfo(Discovery.RiderPathLocator, path, Discovery.RiderPathLocator.GetIsToolbox(path));
          installations.Add(info);
        }

        RiderScriptEditorData.instance.installations = installations.ToArray();
        RiderScriptEditorData.instance.Init();

        m_RiderScriptEditor.CreateSolutionIfDoesntExist();
        if (RiderScriptEditorData.instance.shouldLoadEditorPlugin)
        {
          m_RiderScriptEditor.m_Initiliazer.Initialize(path);
        }

        // can't switch to non-deprecated api, because UnityEditor.Build.BuildPipelineInterfaces.processors is internal
#pragma warning disable 618
        EditorUserBuildSettings.activeBuildTargetChanged += () =>
#pragma warning restore 618
        {
          RiderScriptEditorData.instance.hasChanges = true;
        };
      }
    }

    public bool OpenProject(string path, int line, int column)
    {
      var projectGeneration = (ProjectGeneration.ProjectGeneration) m_ProjectGeneration;
      // Assets - Open C# Project passes empty path here
      if (path != "" && !projectGeneration.HasValidExtension(path))
      {
        return false;
      }
      
      if (!IsUnityScript(path))
      {
        m_ProjectGeneration.SyncIfNeeded(affectedFiles: new string[] { }, new string[] { });
        var fastOpenResult = EditorPluginInterop.OpenFileDllImplementation(path, line, column);
        if (fastOpenResult)
          return true;
      }
      
      var slnFile = GetSolutionFile(path);
      return Discovery.RiderFileOpener.OpenFile(CurrentEditor, slnFile, path, line, column);
    }

    private string GetSolutionFile(string path)
    {
      if (IsUnityScript(path))
      {
        return Path.Combine(GetBaseUnityDeveloperFolder(), "Projects/CSharp/Unity.CSharpProjects.gen.sln");
      }

      var solutionFile = m_ProjectGeneration.SolutionFile();
      if (File.Exists(solutionFile))
      {
        return solutionFile;
      }

      return "";
    }

    static bool IsUnityScript(string path)
    {
      if (UnityEditor.Unsupported.IsDeveloperBuild())
      {
        var baseFolder = GetBaseUnityDeveloperFolder().Replace("\\", "/");
        var lowerPath = path.ToLowerInvariant().Replace("\\", "/");

        if (lowerPath.Contains((baseFolder + "/Runtime").ToLowerInvariant())
          || lowerPath.Contains((baseFolder + "/Editor").ToLowerInvariant()))
        {
          return true;
        }
      }

      return false;
    }

    static string GetBaseUnityDeveloperFolder()
    {
      return Directory.GetParent(EditorApplication.applicationPath).Parent.Parent.FullName;
    }

    public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
    {
      installation = default;
      if (string.IsNullOrEmpty(editorPath)) return false;

      if (FileSystemUtil.EditorPathExists(editorPath) && IsRiderOrFleetInstallation(editorPath))
      {
        if (RiderScriptEditorData.instance.installations == null) // the case when other CodeEditor is set from the very Unity start
        {
          RiderScriptEditorData.instance.installations = Discovery.RiderPathLocator.GetAllRiderPaths();
        }
        
        var realPath = GetEditorRealPath(editorPath);
        var editor = RiderScriptEditorData.instance.installations.FirstOrDefault(a => GetEditorRealPath(a.Path) == realPath);
        if (editor.Path != null)
        {
          installation = new CodeEditor.Installation
          {
            Name = editor.Presentation,
            Path = editor.Path
          };

          return true;
        }
        
        installation = new CodeEditor.Installation
        {
          Name = "Rider (custom location)",
          Path = editorPath
        };

        return true;
      }

      return false;
    }

    public static bool IsRiderOrFleetInstallation(string path)
    {
      if (IsAssetImportWorkerProcess())
        return false;

#if UNITY_2021_1_OR_NEWER
      if (UnityEditor.MPE.ProcessService.level == UnityEditor.MPE.ProcessLevel.Secondary)
        return false;
#elif UNITY_2020_2_OR_NEWER
      if (UnityEditor.MPE.ProcessService.level == UnityEditor.MPE.ProcessLevel.Slave)
        return false;
#elif UNITY_2020_1_OR_NEWER
      if (Unity.MPE.ProcessService.level == Unity.MPE.ProcessLevel.UMP_SLAVE)
        return false;
#endif
      
      if (string.IsNullOrEmpty(path))
        return false;

      return ExecutableStartsWith(path, "rider") || ExecutableStartsWith(path, "fleet");
    }

    public static bool ExecutableStartsWith(string path, string input)
    {
      var fileInfo = new FileInfo(path);
      var filename = fileInfo.Name;
      return filename.StartsWith(input, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAssetImportWorkerProcess()
    {
#if UNITY_2020_2_OR_NEWER
      return UnityEditor.AssetDatabase.IsAssetImportWorkerProcess();
#elif UNITY_2019_3_OR_NEWER
      return UnityEditor.Experimental.AssetDatabaseExperimental.IsAssetImportWorkerProcess();
#else
      return false;
#endif
    }

    public static string CurrentEditor // works fast, doesn't validate if executable really exists
      => EditorPrefs.GetString("kScriptsDefaultApp");

    public CodeEditor.Installation[] Installations => m_Discoverability.PathCallback();

    private void CreateSolutionIfDoesntExist()
    {
      if (!m_ProjectGeneration.HasSolutionBeenGenerated())
      {
        m_ProjectGeneration.Sync();
      }
    }
  }
}