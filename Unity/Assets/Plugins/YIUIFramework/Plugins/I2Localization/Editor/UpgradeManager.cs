using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I2.Loc
{
	[InitializeOnLoad]
	public class UpgradeManager
	{
		static bool mAlreadyCheckedPlugins;

		static UpgradeManager()
		{
			EditorApplication.update += AutoCheckPlugins;
		}

		public static void AutoCheckPlugins()
		{
			CheckPlugins ();
		}

		public static void CheckPlugins( bool bForce = false )
		{
			EditorApplication.update -= AutoCheckPlugins;

			if (mAlreadyCheckedPlugins && !bForce)
				return;
			mAlreadyCheckedPlugins = true;
			
			EnablePlugins(bForce);
			CreateLanguageSources();
			//CreateScriptLocalization();
		}

		const string EditorPrefs_AutoEnablePlugins = "I2Loc AutoEnablePlugins";

		[MenuItem( "Tools/I2 Localization/Enable Plugins/Force Detection", false, 0 )]
		public static void ForceCheckPlugins()
		{
			CheckPlugins( true );
		}

		[MenuItem( "Tools/I2 Localization/Enable Plugins/Enable Auto Detection", false, 1 )]
		public static void EnableAutoCheckPlugins()
		{
			EditorPrefs.SetBool(EditorPrefs_AutoEnablePlugins, true);
		}
		[MenuItem( "Tools/I2 Localization/Enable Plugins/Enable Auto Detection", true)]
		public static bool ValidEnableAutoCheckPlugins()
		{
			return !EditorPrefs.GetBool(EditorPrefs_AutoEnablePlugins, true);
		}


		[MenuItem( "Tools/I2 Localization/Enable Plugins/Disable Auto Detection", false, 2 )]
		public static void DisableAutoCheckPlugins()
		{
			EditorPrefs.SetBool(EditorPrefs_AutoEnablePlugins, false);
		}
		[MenuItem( "Tools/I2 Localization/Enable Plugins/Disable Auto Detection", true)]
		public static bool ValidDisableAutoCheckPlugins()
		{
			return EditorPrefs.GetBool(EditorPrefs_AutoEnablePlugins, true);
		}


        [MenuItem("Tools/I2 Localization/Toggle Highlight Localized", false, 17)]
        public static void ToogleH()
        {
            LocalizationManager.HighlightLocalizedTargets = !LocalizationManager.HighlightLocalizedTargets;
            LocalizationManager.LocalizeAll(true);
        }


        [MenuItem("Tools/I2 Localization/Create Temp")]
        public static void CreateTemp()
        {
            LanguageSourceData source = LocalizationManager.Sources[0];
            for (int i = 0; i < 1000; ++i)
                source.AddTerm("Term " + i, eTermType.Text, false);
            source.UpdateDictionary(true);
        }




        public static void EnablePlugins( bool bForce = false )
		{
			if (!bForce)
			{
				bool AutoEnablePlugins = EditorPrefs.GetBool(EditorPrefs_AutoEnablePlugins, true);
				if (!AutoEnablePlugins)
					return;
			}
			//var tar = System.Enum.GetValues(typeof(BuildTargetGroup));
			foreach (BuildTargetGroup target in Enum.GetValues(typeof(BuildTargetGroup)))
				if (target!=BuildTargetGroup.Unknown && !target.HasAttributeOfType<ObsoleteAttribute>())
				{
					#if UNITY_5_6
						if (target == BuildTargetGroup.Switch) continue;    // some releases of 5.6 defined BuildTargetGroup.Switch but didn't handled it correctly
					#endif
					EnablePluginsOnPlatform( target );
				}

			// Force these one (iPhone has the same # than iOS and iPhone is deprecated, so iOS was been skipped)
			EnablePluginsOnPlatform(BuildTargetGroup.iOS);
		}

		static void EnablePluginsOnPlatform( BuildTargetGroup Platform )
		{
			string Settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(Platform );
			
			bool HasChanged = false;
			List<string> symbols = new List<string>( Settings.Split(';'));
			
			HasChanged |= UpdateSettings("NGUI",  "NGUIDebug",  	  			"", ref symbols);
			HasChanged |= UpdateSettings("DFGUI", "dfPanel", 	  				"", ref symbols);
			HasChanged |= UpdateSettings("TK2D",  "tk2dTextMesh", 				"", ref symbols);
			HasChanged |= UpdateSettings( "TextMeshPro", "TMPro.TMP_FontAsset", "TextMeshPro", ref symbols );
			HasChanged |= UpdateSettings( "SVG", "SVGImporter.SVGAsset",		"", ref symbols );
            
			if (HasChanged)
			{
				try
				{
					Settings = string.Empty;
					for (int i=0,imax=symbols.Count; i<imax; ++i)
					{
						if (i>0) Settings += ";";
						Settings += symbols[i];
					}
					PlayerSettings.SetScriptingDefineSymbolsForGroup(Platform, Settings );
				}
				catch (Exception)
				{
				}
			}
		}
		
		static bool UpdateSettings( string mPlugin, string mType, string AssemblyType, ref List<string> symbols)
		{
			try
			{
				bool hasPluginClass = false;

				if (!string.IsNullOrEmpty( AssemblyType ))
				{
					var rtype = AppDomain.CurrentDomain.GetAssemblies()
								.Where( assembly => assembly.FullName.Contains(AssemblyType) )
								.Select( assembly => assembly.GetType( mType, false ) )
								.Where( t => t!=null )
								.FirstOrDefault();
					if (rtype != null)
						hasPluginClass = true;
				}

				if (!hasPluginClass)
					hasPluginClass = typeof( Localize ).Assembly.GetType( mType, false )!=null;

				
				bool hasPluginDef = symbols.IndexOf(mPlugin)>=0;
				
				if (hasPluginClass != hasPluginDef)
				{
					if (hasPluginClass) symbols.Add(mPlugin);
								   else symbols.Remove(mPlugin);
					return true;
				}
			}
			catch(Exception)
			{
			}
			return false;
			
		}
		
		//[MenuItem( "Tools/I2 Localization/Create I2Languages", false, 1)]
		//方便查看与源码的区别所以保留源码之前的方法
		/*public static void CreateLanguageSources()
		{
			if (LocalizationManager.GlobalSources==null || LocalizationManager.GlobalSources.Length==0)
				return;
			
			Object GlobalSource = Resources.Load(LocalizationManager.GlobalSources[0]);
            LanguageSourceData sourceData = null;
            string sourcePath = null;

            if (GlobalSource != null)
            {
                if (GlobalSource is GameObject)
                {
                    // I2Languages was a prefab before 2018.3, it should be converted to an ScriptableObject
                    sourcePath = AssetDatabase.GetAssetPath(GlobalSource);
                    LanguageSource langSourceObj = (GlobalSource as GameObject).GetComponent<LanguageSource>();
                    sourceData = langSourceObj.mSource;
                }
                else
                {
                    return;
                }
            }

            LanguageSourceAsset asset = ScriptableObject.CreateInstance<LanguageSourceAsset>();
            if (sourceData != null)
            {
                asset.mSource = sourceData;
                AssetDatabase.DeleteAsset(sourcePath);
            }

            if (string.IsNullOrEmpty(sourcePath))
            {
                //string PluginPath = GetI2LocalizationPath();
                string ResourcesFolder = "Assets/Resources";//PluginPath.Substring(0, PluginPath.Length-"/Localization".Length) + "/Resources";

                string fullresFolder = Application.dataPath + ResourcesFolder.Replace("Assets", "");
                if (!Directory.Exists(fullresFolder))
                    Directory.CreateDirectory(fullresFolder);

                sourcePath = ResourcesFolder + "/" + LocalizationManager.GlobalSources[0] + ".asset";
            }
            else
            {
                sourcePath = sourcePath.Replace(".prefab", ".asset");
            }

            AssetDatabase.CreateAsset(asset, sourcePath);
            AssetDatabase.SaveAssets();
		    AssetDatabase.Refresh();
		}*/
		
				
        [MenuItem("Tools/I2 Localization/Help", false, 30)]
        [MenuItem("Help/I2 Localization")]
        public static void MainHelp()
        {
            Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
        }

        /*[MenuItem("Tools/I2 Localization/Open I2Languages.asset", false, 0)]
        //方便查看与源码的区别所以保留源码之前的方法
        public static void OpenGlobalSource()
        {
            CreateLanguageSources();
            LanguageSourceAsset GO = Resources.Load<LanguageSourceAsset>(LocalizationManager.GlobalSources[0]);
            if (GO == null)
                Debug.Log("Unable to find Global Language at Assets/Resources/" + LocalizationManager.GlobalSources[0] + ".asset");
            
            Selection.activeObject = GO;
        }*/

        #region 修改全局路径
		
        //源码必须吧资源放在resources下
        //根据自己的其他需求最后规划不需要 所以自行修改
        //1 全局的asset 将会放到editor下 因为只有editor才使用
        //2 平台时会自行管理  根据需求动态加载
        //LocalizationManager.GlobalSources 不能有数据 请设置为 {}

        private const string I2GlobalSourcesEditorFolderPath = "Assets/Editor/I2Localization";
        private const string I2GlobalSourcesEditorPath = "Assets/Editor/I2Localization/I2Languages.asset";
        
        [MenuItem("Tools/I2 Localization/Open I2Languages.asset", false, 0)]
        public static void OpenGlobalSource()
        {
	        var globalSourcesAsset = CreateLanguageSources();

	        if (globalSourcesAsset == null)
		        Debug.LogError($"没有找到数据源 {I2GlobalSourcesEditorPath}");
            
	        Selection.activeObject = globalSourcesAsset;
        }
        
        private static LanguageSourceAsset CreateLanguageSources()
		{

			var globalSourcesAsset = AssetDatabase.LoadAssetAtPath<LanguageSourceAsset>(I2GlobalSourcesEditorPath);
			if (globalSourcesAsset != null)
			{
				return globalSourcesAsset;
			}
			
            var asset = ScriptableObject.CreateInstance<LanguageSourceAsset>();

            var assetFolder = Application.dataPath + I2GlobalSourcesEditorFolderPath.Replace("Assets", "");
            if (!Directory.Exists(assetFolder))
	            Directory.CreateDirectory(assetFolder);
            
            AssetDatabase.CreateAsset(asset, I2GlobalSourcesEditorPath);
            AssetDatabase.SaveAssets();
		    AssetDatabase.Refresh();

		    return asset;
		}
        
        #endregion
        
        /*static void CreateScriptLocalization()
		{
			string[] assets = AssetDatabase.FindAssets("ScriptLocalization");
			if (assets.Length>0)
				return;
			
			string ScriptsFolder = "Assets";
			string ScriptText = LocalizationEditor.mScriptLocalizationHeader + "	}\n}";
			
			System.IO.File.WriteAllText(ScriptsFolder + "/ScriptLocalization.cs", ScriptText);
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}*/

        public static string GetI2LocalizationPath()
		{
			string[] assets = AssetDatabase.FindAssets("LocalizationManager");
			if (assets.Length==0)
				return string.Empty;
			
			string PluginPath = AssetDatabase.GUIDToAssetPath(assets[0]);
			PluginPath = PluginPath.Substring(0, PluginPath.Length - "/Scripts/LocalizationManager.cs".Length);
			
			return PluginPath;
		}

		public static string GetI2Path()
		{
			string pluginPath = GetI2LocalizationPath();
			return pluginPath.Substring(0, pluginPath.Length-"/Localization".Length);
		}

		public static string GetI2CommonResourcesPath()
		{
			string I2Path = GetI2Path();
			return I2Path + "/Resources";
		}
	}

	public static class UpgradeManagerHelper
	{
		public static bool HasAttributeOfType<T>(this Enum enumVal) where T:Attribute
		{
			var type = enumVal.GetType();
			var memInfo = type.GetMember(enumVal.ToString());
			var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
			return attributes.Length > 0;
		}
	}
}