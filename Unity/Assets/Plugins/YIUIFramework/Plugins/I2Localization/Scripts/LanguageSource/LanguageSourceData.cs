using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I2.Loc
{
    public interface ILanguageSource
    {
        LanguageSourceData SourceData { get; set; }
    }

    [ExecuteInEditMode]
    [Serializable]
	public partial class LanguageSourceData
    {
        #region Variables

        [NonSerialized] public ILanguageSource owner;
        public Object ownerObject { get { return owner as Object; } }

        public bool UserAgreesToHaveItOnTheScene;
		public bool UserAgreesToHaveItInsideThePluginsFolder;
        public bool GoogleLiveSyncIsUptoDate = true;

        [NonSerialized] public bool mIsGlobalSource;

        #endregion

        #region Variables : Terms

        public List<TermData> mTerms = new List<TermData>();

        public bool CaseInsensitiveTerms;

        //This is used to overcome the issue with Unity not serializing Dictionaries
        [NonSerialized] public Dictionary<string, TermData> mDictionary = new Dictionary<string, TermData>(StringComparer.Ordinal);

        public enum MissingTranslationAction { Empty, Fallback, ShowWarning, ShowTerm }
        public MissingTranslationAction OnMissingTranslation = MissingTranslationAction.Fallback;

        public string mTerm_AppName;

        #endregion

        #region Variables : Languages

        public List<LanguageData> mLanguages = new List<LanguageData>();

        public bool IgnoreDeviceLanguage; // If false, it will use the Device's language as the initial Language, otherwise it will use the first language in the source.

        public enum eAllowUnloadLanguages { Never, OnlyInDevice, EditorAndDevice }
        public eAllowUnloadLanguages _AllowUnloadingLanguages = eAllowUnloadLanguages.Never;

        #endregion

        #region Variables : Google

        public string Google_WebServiceURL;
        public string Google_SpreadsheetKey;
        public string Google_SpreadsheetName;
        public string Google_LastUpdatedVersion;

#if UNITY_EDITOR
        public string Google_Password = "change_this";
#endif

        public enum eGoogleUpdateFrequency { Always, Never, Daily, Weekly, Monthly, OnlyOnce, EveryOtherDay }
        public eGoogleUpdateFrequency GoogleUpdateFrequency = eGoogleUpdateFrequency.Weekly;
        public eGoogleUpdateFrequency GoogleInEditorCheckFrequency = eGoogleUpdateFrequency.Daily;

        // When Manual, the user has to call LocalizationManager.ApplyDownloadedDataFromGoogle() during a loading screen or similar
        public enum eGoogleUpdateSynchronization { Manual, OnSceneLoaded, AsSoonAsDownloaded }
        public eGoogleUpdateSynchronization GoogleUpdateSynchronization = eGoogleUpdateSynchronization.OnSceneLoaded;

        public float GoogleUpdateDelay; // How many second to delay downloading data from google (to avoid lag on the startup)

        public event LanguageSource.fnOnSourceUpdated Event_OnSourceUpdateFromGoogle;    // (LanguageSource, bool ReceivedNewData, string errorMsg)

        #endregion

        #region Variables : Assets

        public List<Object> Assets = new List<Object>();	// References to Fonts, Atlasses and other objects the localization may need

        //This is used to overcome the issue with Unity not serializing Dictionaries
        [NonSerialized] public Dictionary<string, Object> mAssetDictionary = new Dictionary<string, Object>(StringComparer.Ordinal);

        #endregion

        #region EditorVariables
#if UNITY_EDITOR

        public string Spreadsheet_LocalFileName;
		public string Spreadsheet_LocalCSVSeparator = ",";
        public string Spreadsheet_LocalCSVEncoding = "utf-8";
        public bool Spreadsheet_SpecializationAsRows = true;

#endif
        #endregion

        #region Language

        public void Awake()
		{
			LocalizationManager.AddSource (this);
			UpdateDictionary();
            UpdateAssetDictionary();
            LocalizationManager.LocalizeAll(true);
        }

        public void OnDestroy()
        {
            LocalizationManager.RemoveSource(this);
        }
 


		public bool IsEqualTo( LanguageSourceData Source )
		{
			if (Source.mLanguages.Count != mLanguages.Count)
				return false;

			for (int i=0, imax=mLanguages.Count; i<imax; ++i)
				if (Source.GetLanguageIndex( mLanguages[i].Name ) < 0)
					return false;

			if (Source.mTerms.Count != mTerms.Count)
				return false;

			for (int i=0; i<mTerms.Count; ++i)
				if (Source.GetTermData(mTerms[i].Term)==null)
					return false;

			return true;
		}

		internal bool ManagerHasASimilarSource()
		{
			for (int i=0, imax=LocalizationManager.Sources.Count; i<imax; ++i)
			{
				LanguageSourceData source = LocalizationManager.Sources[i];
				if (source!=null && source.IsEqualTo(this) && source!=this)
					return true;
			}
			return false;
		}

		public void ClearAllData()
		{
			mTerms.Clear ();
			mLanguages.Clear ();
			mDictionary.Clear();
            mAssetDictionary.Clear();
		}

        public bool IsGlobalSource()
        {
            return mIsGlobalSource;
        }

        #endregion

        public void Editor_SetDirty()
        {
            #if UNITY_EDITOR
                if (ownerObject != null)
                {
                    EditorUtility.SetDirty(ownerObject);
                }
            #endif
        }

    }
}