using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {

        #region Variables: Misc

        #endregion

        public static void InitializeIfNeeded()
        {
            #if UNITY_EDITOR
                #if UNITY_2017_2_OR_NEWER
                                EditorApplication.playModeStateChanged -= OnEditorPlayModeStateChanged;
                                EditorApplication.playModeStateChanged += OnEditorPlayModeStateChanged;
                #else
                            UnityEditor.EditorApplication.playmodeStateChanged -= OldOnEditorPlayModeStateChanged;
                            UnityEditor.EditorApplication.playmodeStateChanged += OldOnEditorPlayModeStateChanged;
                #endif
            #endif

            if (string.IsNullOrEmpty(mCurrentLanguage) || Sources.Count == 0)
            {
                AutoLoadGlobalParamManagers();
                UpdateSources();
                SelectStartupLanguage();
            }
        }

        public static string GetVersion()
		{
			return "2.8.20 f2";
		}

		public static int GetRequiredWebServiceVersion()
		{
			return 5;
		}

		public static string GetWebServiceURL( LanguageSourceData source = null )
		{
			if (source != null && !string.IsNullOrEmpty(source.Google_WebServiceURL))
				return source.Google_WebServiceURL;

            InitializeIfNeeded();
			for (int i = 0; i < Sources.Count; ++i)
				if (Sources[i] != null && !string.IsNullOrEmpty(Sources[i].Google_WebServiceURL))
					return Sources[i].Google_WebServiceURL;
			return string.Empty;
		}

#if UNITY_EDITOR
    #if UNITY_2017_2_OR_NEWER
        static void OnEditorPlayModeStateChanged( PlayModeStateChange stateChange )
        {
            if (stateChange != PlayModeStateChange.ExitingPlayMode)
                return;
    #else
        static void OldOnEditorPlayModeStateChanged()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                        return;
    #endif

            OnLocalizeEvent = null;

            foreach (var source in Sources)
            {
                source.LoadAllLanguages(true);
            }
            try
            {
                var tempPath = Application.temporaryCachePath;

                foreach (var file in Directory.GetFiles(tempPath).Where(f => f.Contains("LangSource_") && f.EndsWith(".loc", StringComparison.Ordinal)))
                {
                    File.Delete(file);
                }
            }
            catch(Exception)
            {
            }
        }
#endif
    }
}
