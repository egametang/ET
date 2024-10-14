using Unity.CodeEditor;
using UnityEditor;

namespace Packages.Rider.Editor.PostProcessors
{
  internal class RiderAssetPostprocessor: AssetPostprocessor
  {
    public static bool OnPreGeneratingCSProjectFiles()
    {
      var path = RiderScriptEditor.GetEditorRealPath(CodeEditor.CurrentEditorInstallation);
      if (RiderScriptEditor.IsRiderOrFleetInstallation(path))
        return !ProjectGeneration.ProjectGeneration.isRiderProjectGeneration;
      return false;
    }
  }
}