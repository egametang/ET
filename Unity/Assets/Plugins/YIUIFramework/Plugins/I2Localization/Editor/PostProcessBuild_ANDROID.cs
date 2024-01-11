#if UNITY_ANDROID
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	public class PostProcessBuild_Android
	{
        // Post Process Scene is a hack, because using PostProcessBuild will be called after the APK is generated, and so, I didn't find a way to copy the new files
        [PostProcessScene]
        public static void OnPostProcessScene()
        {
            #if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
                bool isFirstScene = (EditorBuildSettings.scenes.Length>0 && EditorBuildSettings.scenes[0].path == EditorApplication.currentScene);
            #else
                bool isFirstScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex <= 0;
            #endif

            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) &&
                isFirstScene)
            {
                string projPath = System.IO.Path.GetFullPath(Application.streamingAssetsPath + "/../../Temp/StagingArea");
                //string projPath = System.IO.Path.GetFullPath(Application.dataPath+ "/Plugins/Android");
                PostProcessAndroid(BuildTarget.Android, projPath);
            }
        }

        //[PostProcessBuild(10000)]
        public static void PostProcessAndroid(BuildTarget buildTarget, string pathToBuiltProject)
		{
			if (buildTarget!=BuildTarget.Android)
				return;

            if (LocalizationManager.Sources.Count <= 0)
				LocalizationManager.UpdateSources();

            // Get language with variants, but also add it without the variant to allow fallbacks (e.g. en-CA also adds en)
			var langCodes = LocalizationManager.GetAllLanguagesCode(false).Concat( LocalizationManager.GetAllLanguagesCode(true) ).Distinct().ToList();

			if (langCodes.Count <= 0)
				return;
			string stringXML =  "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"+
								"<resources>\n"+
								"    <string name=\"app_name\">{0}</string>\n"+
								"</resources>";

            SetStringsFile( pathToBuiltProject+"/res/values", "strings.xml", stringXML, LocalizationManager.GetAppName(langCodes[0]) );


			var list = new List<string>();
			list.Add( pathToBuiltProject + "/res/values" );
			foreach (var code in langCodes)
			{
                // Android doesn't use zh-CN or zh-TW, instead it uses: zh-rCN, zh-rTW, zh
                string fixedCode = code;
                if (fixedCode.StartsWith("zh", System.StringComparison.OrdinalIgnoreCase))
                {
                    string googleCode = GoogleLanguages.GetGoogleLanguageCode(fixedCode);
                    if (googleCode==null) googleCode = fixedCode;
                    fixedCode = (googleCode == "zh-CN") ? "zh-CN" : googleCode;
                }
				fixedCode = fixedCode.Replace("-", "-r");

                string dir = pathToBuiltProject + "/res/values-" + fixedCode;

                SetStringsFile( dir, "strings.xml", stringXML, LocalizationManager.GetAppName(code) );
			}
		}

		static void CreateFileIfNeeded ( string folder, string fileName, string text )
		{
			try
			{
				if (!System.IO.Directory.Exists( folder ))
					System.IO.Directory.CreateDirectory( folder );

				if (!System.IO.File.Exists( folder + "/"+fileName ))
					System.IO.File.WriteAllText( folder + "/"+fileName, text );
			}
			catch (System.Exception e)
			{
				Debug.Log( e );
			}
		}

        static void SetStringsFile(string folder, string fileName, string stringXML, string appName)
        {
            try
            {
                appName = appName.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "\\\"").Replace("'", "\\'");
                appName = appName.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                if (!System.IO.File.Exists(folder + "/" + fileName))
                {
                    // create the string file if it doesn't exist
                    stringXML = string.Format(stringXML, appName);
                }
                else
                {
                    stringXML = System.IO.File.ReadAllText(folder + "/" + fileName);
                    // find app_name
                    var pattern = "\"app_name\">(.*)<\\/string>";
                    var regexPattern = new System.Text.RegularExpressions.Regex(pattern);
                    if (regexPattern.IsMatch(stringXML))
                    {
                        // Override the AppName if it was found
                        stringXML = regexPattern.Replace(stringXML, string.Format("\"app_name\">{0}</string>", appName));
                    }
                    else
                    {
                        // insert the appName if it wasn't there
                        int idx = stringXML.IndexOf("<resources>");
                        if (idx > 0)
                            stringXML = stringXML.Insert(idx + "</resources>".Length, string.Format("\n    <string name=\"app_name\">{0}</string>\n", appName));
                    }
                }
                System.IO.File.WriteAllText(folder + "/" + fileName, stringXML);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
#endif