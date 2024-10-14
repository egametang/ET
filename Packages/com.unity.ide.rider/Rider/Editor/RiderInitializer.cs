using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Rider.Editor.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Packages.Rider.Editor
{
    internal class RiderInitializer
    {
      public void Initialize(string editorPath)
      {
        var assembly = EditorPluginInterop.EditorPluginAssembly;
        if (EditorPluginInterop.EditorPluginIsLoadedFromAssets(assembly))
        {
          Debug.LogError($"Please delete {assembly.Location}. Unity 2019.2+ loads it directly from Rider installation. To disable this, open Rider's settings, search and uncheck 'Automatically install and update Rider's Unity editor plugin'.");
          return;
        }
        
        if (assembly != null) // already loaded RIDER-92419
        {
          return;
        }
        
        // for debugging rider editor plugin
        if (RiderPathUtil.IsRiderDevEditor(editorPath))
        {
          LoadEditorPluginForDevEditor(editorPath);
        }
        else
        {
          var relPath = "../../plugins/rider-unity/EditorPlugin";
          if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
            relPath = "Contents/plugins/rider-unity/EditorPlugin";
          var baseDir = Path.Combine(editorPath, relPath);
          var dllFile = new FileInfo(Path.Combine(baseDir, $"{EditorPluginInterop.EditorPluginAssemblyName}.dll"));

          if (!dllFile.Exists)
            dllFile = new FileInfo(Path.Combine(baseDir,
              $"{EditorPluginInterop.EditorPluginAssemblyNameFallback}.dll"));
          
          if (dllFile.Exists)
          {
            var bytes = File.ReadAllBytes(dllFile.FullName); 
            assembly = AppDomain.CurrentDomain.Load(bytes); // doesn't lock assembly on disk
            if (PluginSettings.SelectedLoggingLevel >= LoggingLevel.TRACE)
              Debug.Log($"Rider EditorPlugin loaded from {dllFile.FullName}");
          
            EditorPluginInterop.InitEntryPoint(assembly);
          }
          else
          {
            Debug.Log($"Unable to find Rider EditorPlugin {dllFile.FullName} for Unity ");
          }
        }
      }

      private static void LoadEditorPluginForDevEditor(string editorPath)
      {
        var file = new FileInfo(editorPath);
        if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
          file = new FileInfo(Path.Combine(editorPath, "rider-dev.bat"));
        
        if (!file.Exists)
        {
          Debug.Log($"Unable to determine path to EditorPlugin from {file}");
          return;
        }
        
        var dllPath = File.ReadLines(file.FullName).FirstOrDefault();

        if (dllPath == null)
        {
          Debug.Log($"Unable to determine path to EditorPlugin from {file}");
          return;
        }

        var dllFile = new FileInfo(dllPath);

        if (!dllFile.Exists)
        {
          Debug.Log($"Unable to find Rider EditorPlugin {dllPath} for Unity ");
          return;
        }

        var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(dllFile.FullName));
        if (PluginSettings.SelectedLoggingLevel >= LoggingLevel.TRACE)
          Debug.Log($"Rider EditorPlugin loaded from {dllFile.FullName}");

        EditorPluginInterop.InitEntryPoint(assembly);
      }
    }
}
