using JetBrains.Annotations;
using Packages.Rider.Editor;
using Unity.CodeEditor;

// ReSharper disable once CheckNamespace 
namespace JetBrains.Rider.Unity.Editor
{
  /// <summary>
  /// Is called via commandline from Rider Notification after checking out from source control.
  /// </summary>
  [UsedImplicitly]
  public static class RiderMenu
  {
    /// <summary>
    /// Is called via commandline from Rider Notification after checking out from source control.
    /// </summary>
    [UsedImplicitly]
    public static void MenuOpenProject()
    {
      if (RiderScriptEditor.IsRiderOrFleetInstallation(RiderScriptEditor.CurrentEditor))
      {
        // Force the project files to be sync
        CodeEditor.CurrentEditor.SyncAll();

        // Load Project
        CodeEditor.CurrentEditor.OpenProject();
      }
    }
  }
}