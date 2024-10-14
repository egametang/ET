using System.IO;

#if UNITY_2020_1_OR_NEWER
using UnityEditor.PackageManager;
#endif

namespace Packages.Rider.Editor.ProjectGeneration
{
  internal static class PackageManagerTracker
  {
    private static bool HasManifestJsonLastWriteTimeChanged()
    {
      if (!LastWriteTracker.IsUnityCompatible()) return false;
      var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
      var manifestFile = new FileInfo(Path.Combine(directoryInfo.FullName, "Packages/manifest.json"));
      if (manifestFile.Exists)
      {
        // for the manifest.json, we store the LastWriteTime here
        var res = manifestFile.LastWriteTime > RiderScriptEditorPersistedState.instance.ManifestJsonLastWrite;
        if (res) RiderScriptEditorPersistedState.instance.ManifestJsonLastWrite = manifestFile.LastWriteTime;
        return res;
      }

      return false;
    }

    /// <summary>
    /// If the manifest.json was changed outside Unity and Rider calls Unity to Refresh, we should call PM to Refresh its state also
    /// </summary>
    /// <param name="checkProjectFiles"></param>
    internal static void SyncIfNeeded(bool checkProjectFiles)
    {
#if UNITY_2020_1_OR_NEWER
      if (checkProjectFiles && HasManifestJsonLastWriteTimeChanged())
      {
        Client.Resolve();
      }
#endif
    }
  }
}