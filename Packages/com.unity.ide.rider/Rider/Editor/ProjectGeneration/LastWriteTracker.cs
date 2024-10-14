using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal static class LastWriteTracker
  {
    internal static bool HasLastWriteTimeChanged()
    {
      if (!IsUnityCompatible()) return false;

      // any external changes of sln/csproj should cause their regeneration
      // Directory.GetCurrentDirectory(), "*.csproj", "*.sln"
      var files = new List<FileInfo>();

      var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
      files.AddRange(directoryInfo.GetFiles("*.csproj"));
      files.Add(new FileInfo(Path.Combine(directoryInfo.FullName, directoryInfo.Name + ".sln")));

      return files.Any(a => a.LastWriteTime > RiderScriptEditorPersistedState.instance.LastWrite);
    }

    internal static void UpdateLastWriteIfNeeded(string path)
    {
      if (!IsUnityCompatible()) return;

      var fileInfo = new FileInfo(path);
      if (fileInfo.Directory == null)
        return;
      var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
      if (fileInfo.Directory.FullName.Equals(directoryInfo.FullName, StringComparison.OrdinalIgnoreCase) &&
          (fileInfo.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
           || fileInfo.Name.Equals(directoryInfo.Name + ".sln", StringComparison.OrdinalIgnoreCase)))
      {
        RiderScriptEditorPersistedState.instance.LastWrite = fileInfo.LastWriteTime;
      }
    }

    internal static bool IsUnityCompatible()
    {
#if UNITY_2020_1_OR_NEWER
      return true;
#else
      return false;
#endif
    }
  }
}