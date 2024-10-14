using UnityEditor;
using UnityEngine;

namespace Packages.Rider.Editor
{
  internal static class PluginSettings
  {
    public static LoggingLevel SelectedLoggingLevel
    {
      get => (LoggingLevel) EditorPrefs.GetInt("Rider_SelectedLoggingLevel", 0);
      private set => EditorPrefs.SetInt("Rider_SelectedLoggingLevel", (int) value);
    }

    public static bool LogEventsCollectorEnabled
    {
      get => EditorPrefs.GetBool("Rider_LogEventsCollectorEnabled", true);
      private set => EditorPrefs.SetBool("Rider_LogEventsCollectorEnabled", value);
    }

    /// <summary>
    /// Preferences menu layout
    /// </summary>
    /// <remarks>
    /// Contains all 3 toggles: Enable/Disable; Debug On/Off; Writing Launch File On/Off
    /// </remarks>
    [SettingsProvider]
    private static SettingsProvider RiderPreferencesItem()
    {
      if (!RiderScriptEditor.IsRiderOrFleetInstallation(RiderScriptEditor.CurrentEditor))
        return null;
      if (!RiderScriptEditorData.instance.shouldLoadEditorPlugin)
        return null;
      var provider = new SettingsProvider("Preferences/Rider", SettingsScope.User)
      {
        label = "Rider",
        keywords = new[] { "Rider" },
        guiHandler = (searchContext) =>
        {
          EditorGUIUtility.labelWidth = 200f;
          EditorGUILayout.BeginVertical();

          GUILayout.BeginVertical();
          LogEventsCollectorEnabled =
            EditorGUILayout.Toggle(new GUIContent("Pass Console to Rider:"), LogEventsCollectorEnabled);

          GUILayout.EndVertical();
          GUILayout.Label("");

          if (!string.IsNullOrEmpty(EditorPluginInterop.LogPath))
          {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Log file:");
            var previous = GUI.enabled;
            GUI.enabled = previous && SelectedLoggingLevel != LoggingLevel.OFF;
            var button = GUILayout.Button(new GUIContent("Open log"));
            if (button)
            {
              // would use Rider if `log` is on the list of extensions, otherwise would use editor configured in the OS
              UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(EditorPluginInterop.LogPath, 0);
            }

            GUI.enabled = previous;
            GUILayout.EndHorizontal();
          }

          var loggingMsg =
            @"Sets the amount of Rider Debug output. If you are about to report an issue, please select Verbose logging level and attach Unity console output to the issue.";
          SelectedLoggingLevel =
            (LoggingLevel) EditorGUILayout.EnumPopup(new GUIContent("Logging Level:", loggingMsg),
              SelectedLoggingLevel);


          EditorGUILayout.HelpBox(loggingMsg, MessageType.None);

          const string url = "https://github.com/JetBrains/resharper-unity";
          if (LinkButton(url))
            Application.OpenURL(url);;

          GUILayout.FlexibleSpace();
          GUILayout.BeginHorizontal();

          GUILayout.FlexibleSpace();
          var assembly = EditorPluginInterop.EditorPluginAssembly;
          if (assembly != null)
          {
            var version = assembly.GetName().Version;
            GUILayout.Label("Plugin version: " + version, new GUIStyle(GUI.skin.label)
            {
              margin = new RectOffset(4, 4, 4, 4),
            });
          }

          GUILayout.EndHorizontal();

          EditorGUILayout.EndVertical();
        }
      };
      return provider;
    }

    public static bool LinkButton(string url)
    {
      var bClicked = GUILayout.Button(url, RiderStyles.LinkLabelStyle);
      var rect = GUILayoutUtility.GetLastRect();
      rect.width = RiderStyles.LinkLabelStyle.CalcSize(new GUIContent(url)).x;
      EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

      return bClicked;
    }
  }
}