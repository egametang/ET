using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {

        #region Variables: Misc

        public static List<LanguageSourceData> Sources = new List<LanguageSourceData>();
        //public static string[] GlobalSources = { "I2Languages" };
        public static string[] GlobalSources = {}; //自定义修改 不要全局 全部自行处理

        #endregion

        #region Sources

        static LocalizationManager()
        {
	        Sources.Clear();
			#if UNITY_EDITOR
	        m_LastLanguageSourceAsset = null;
	        #endif
	        //Debug.Log("I2LocalizationManager 重置初始化");
        }
        
        public static bool UpdateSources()
		{
			//UnregisterDeletededSources();
			//RegisterSourceInResources();
			//RegisterSceneSources();
			#if UNITY_EDITOR
			if (!I2Utils.IsPlaying())
			{
				RegisterSourceInEditor();
			}
			#endif
			return Sources.Count>0;
		}

		static void UnregisterDeletededSources()
		{
			// Delete sources that were part of another scene and not longer available
			for (int i=Sources.Count-1; i>=0; --i)
				if (Sources[i] == null)
					RemoveSource( Sources[i] );
		}

#if UNITY_EDITOR
		public static void RegisterSourceInEditor()
		{
			var sourceAsset = GetEditorAsset();
			if (sourceAsset == null) return;
			
			if (sourceAsset && !Sources.Contains(sourceAsset.mSource))
			{
				if (!sourceAsset.mSource.mIsGlobalSource)
					sourceAsset.mSource.mIsGlobalSource = true;
				sourceAsset.mSource.owner = sourceAsset;
				AddSource(sourceAsset.mSource);
			}
		}
		
		private static LanguageSourceAsset m_LastLanguageSourceAsset;
		
		public static LanguageSourceAsset GetEditorAsset(bool force = false)
		{
			if (m_LastLanguageSourceAsset != null && !force)
			{
				return m_LastLanguageSourceAsset;
			}
			
			Debug.Log("I2LocalizationManager 加载编辑器资源数据");
			var sourceAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<LanguageSourceAsset>(I2LocalizeHelper.I2GlobalSourcesEditorPath);
			if (sourceAsset == null)
			{
				Debug.LogError($"错误 没有找到编辑器下的资源 {I2LocalizeHelper.I2GlobalSourcesEditorPath}");
				return null;
			}

			m_LastLanguageSourceAsset = sourceAsset;
			return sourceAsset;
		}
		
#endif
		static void RegisterSceneSources()
		{
			LanguageSource[] sceneSources = (LanguageSource[])Resources.FindObjectsOfTypeAll( typeof(LanguageSource) );
            foreach (LanguageSource source in sceneSources)
				if (!Sources.Contains(source.mSource))
				{
                    if (source.mSource.owner == null)
                        source.mSource.owner = source;
                    AddSource( source.mSource );
				}
		}		

		static void RegisterSourceInResources()
		{
			// Find the Source that its on the Resources Folder
			foreach (string SourceName in GlobalSources)
			{
				LanguageSourceAsset sourceAsset = ResourceManager.pInstance.GetAsset<LanguageSourceAsset>(SourceName);
				
				if (sourceAsset && !Sources.Contains(sourceAsset.mSource))
                {
                    if (!sourceAsset.mSource.mIsGlobalSource)
                        sourceAsset.mSource.mIsGlobalSource = true;
                    sourceAsset.mSource.owner = sourceAsset;
                    AddSource(sourceAsset.mSource);
                }
            }
		}

		public static Func<LanguageSourceData, bool> Callback_AllowSyncFromGoogle = null;
		static bool AllowSyncFromGoogle(LanguageSourceData Source)
		{
			if (Callback_AllowSyncFromGoogle == null)
				return true;
			return Callback_AllowSyncFromGoogle.Invoke(Source);
		}

		internal static void AddSource ( LanguageSourceData Source )
		{
			if (Sources.Contains (Source))
				return;

			#if UNITY_EDITOR

			Debug.Log($">>----------添加多语言数据----------<<");
			foreach (var languageName in Source.owner.SourceData.GetLanguages())
			{
				Debug.Log(languageName);
			}
			Debug.Log($">>----------添加多语言数据----------<<");
			
			#endif
			
            Sources.Add( Source );

			if (Source.HasGoogleSpreadsheet() && Source.GoogleUpdateFrequency != LanguageSourceData.eGoogleUpdateFrequency.Never && AllowSyncFromGoogle(Source))
			{
                #if !UNITY_EDITOR
                    Source.Import_Google_FromCache();
                    bool justCheck = false;
                #else
                    bool justCheck=true;
                #endif
                if (Source.GoogleUpdateDelay > 0)
						CoroutineManager.Start( Delayed_Import_Google(Source, Source.GoogleUpdateDelay, justCheck) );
				else
					Source.Import_Google(false, justCheck);
            }

            //if (force)
            {
                for (int i = 0; i < Source.mLanguages.Count; ++i)
                    Source.mLanguages[i].SetLoaded(true);
            }

            if (Source.mDictionary.Count==0)
				Source.UpdateDictionary(true);
		}

		static IEnumerator Delayed_Import_Google ( LanguageSourceData source, float delay, bool justCheck )
		{
			yield return new WaitForSeconds( delay );
            if (source != null) // handle cases where the source is already deleted
            {
                source.Import_Google(false, justCheck);
            }
		}

		public static void RemoveSource (LanguageSourceData Source )
		{
			//Debug.Log ("RemoveSource " + Source+" " + Source.GetInstanceID());
			Sources.Remove( Source );
		}

		public static bool IsGlobalSource( string SourceName )
		{
			return Array.IndexOf(GlobalSources, SourceName)>=0;
		}

		public static LanguageSourceData GetSourceContaining( string term, bool fallbackToFirst = true )
		{
			if (!string.IsNullOrEmpty(term))
			{
				for (int i=0, imax=Sources.Count; i<imax; ++i)
				{
					if (Sources[i].GetTermData(term) != null)
						return Sources[i];
				}
			}
			
			return fallbackToFirst && Sources.Count>0 ? Sources[0] :  null;
		}

		public static Object FindAsset (string value)
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				Object Obj = Sources[i].FindAsset(value);
				if (Obj)
					return Obj;
			}
			return null;
		}

        public static void ApplyDownloadedDataFromGoogle()
        {
            for (int i = 0, imax = Sources.Count; i < imax; ++i)
            {
                Sources[i].ApplyDownloadedDataFromGoogle();
            }
        }

        #endregion

    }
}
