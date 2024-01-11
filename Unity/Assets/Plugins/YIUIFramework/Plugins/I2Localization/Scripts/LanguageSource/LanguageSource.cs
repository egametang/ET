using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
    [AddComponentMenu("I2/Localization/Source")]
    [ExecuteInEditMode]
	public class LanguageSource : MonoBehaviour, ISerializationCallbackReceiver, ILanguageSource
    {
        public LanguageSourceData SourceData
        {
            get { return mSource; }
            set { mSource = value; }
        }
        public LanguageSourceData mSource = new LanguageSourceData();

        // Because of Unity2018.3 change in Prefabs, now all the source variables are moved into LanguageSourceData
        // But to avoid loosing previously serialized data, these vars are copied into mSource.XXXX when deserializing)
        // These are going to be removed once everyone port their projects to the new I2L version.
        #region Legacy Variables 

        // TODO: also copy         public string name;   and owner

        public int version;
        public bool NeverDestroy;  	// Keep between scenes (will call DontDestroyOnLoad )

		public bool UserAgreesToHaveItOnTheScene;
		public bool UserAgreesToHaveItInsideThePluginsFolder;
        public bool GoogleLiveSyncIsUptoDate = true;

        public List<Object> Assets = new List<Object>();	// References to Fonts, Atlasses and other objects the localization may need

        public string Google_WebServiceURL;
        public string Google_SpreadsheetKey;
        public string Google_SpreadsheetName;
        public string Google_LastUpdatedVersion;


        public LanguageSourceData.eGoogleUpdateFrequency GoogleUpdateFrequency = LanguageSourceData.eGoogleUpdateFrequency.Weekly;

        public float GoogleUpdateDelay = 5; // How many second to delay downloading data from google (to avoid lag on the startup)

        public delegate void fnOnSourceUpdated(LanguageSourceData source, bool ReceivedNewData, string errorMsg);
        public event fnOnSourceUpdated Event_OnSourceUpdateFromGoogle;

        public List<LanguageData> mLanguages = new List<LanguageData>();

        public bool IgnoreDeviceLanguage; // If false, it will use the Device's language as the initial Language, otherwise it will use the first language in the source.

        public LanguageSourceData.eAllowUnloadLanguages _AllowUnloadingLanguages = LanguageSourceData.eAllowUnloadLanguages.Never;

        public List<TermData> mTerms = new List<TermData>();

        public bool CaseInsensitiveTerms;

        public LanguageSourceData.MissingTranslationAction OnMissingTranslation = LanguageSourceData.MissingTranslationAction.Fallback;

        public string mTerm_AppName;

        #endregion

        #region EditorVariables
        #if UNITY_EDITOR

            public string Spreadsheet_LocalFileName;
		    public string Spreadsheet_LocalCSVSeparator = ",";
            public string Spreadsheet_LocalCSVEncoding = "utf-8";
            public bool Spreadsheet_SpecializationAsRows = true;

            public string Google_Password = "change_this";
            public LanguageSourceData.eGoogleUpdateFrequency GoogleInEditorCheckFrequency = LanguageSourceData.eGoogleUpdateFrequency.Daily;
#endif
        #endregion

        void Awake()
        {
            #if UNITY_EDITOR
            if (BuildPipeline.isBuildingPlayer)
                return;
            #endif
   //         NeverDestroy = false;

   //         if (NeverDestroy)
			//{
			//	if (mSource.ManagerHasASimilarSource())
			//	{
			//		Object.Destroy (this);
			//		return;
			//	}
			//	else
			//	{
			//		if (Application.isPlaying)
			//			DontDestroyOnLoad (gameObject);
			//	}
			//}
            mSource.owner = this;
            //不自动调用awake 由其他方式代替
            //mSource.Awake();
        }

        private void OnDestroy()
        {
            NeverDestroy = false;

            if (!NeverDestroy)
            {
               mSource.OnDestroy();
            }
        }

        public string GetSourceName()
        {
            string s = gameObject.name;
            Transform tr = transform.parent;
            while (tr)
            {
                s = string.Concat(tr.name, "_", s);
                tr = tr.parent;
            }
            return s;
        }

        public void OnBeforeSerialize()
        {
            version = 1;
        }

        public void OnAfterDeserialize()
        {
            if (version==0 || mSource==null)
            {
                mSource = new LanguageSourceData();
                mSource.owner = this;
                mSource.UserAgreesToHaveItOnTheScene = UserAgreesToHaveItOnTheScene;
                mSource.UserAgreesToHaveItInsideThePluginsFolder = UserAgreesToHaveItInsideThePluginsFolder;
                mSource.IgnoreDeviceLanguage = IgnoreDeviceLanguage;
                mSource._AllowUnloadingLanguages = _AllowUnloadingLanguages;
                mSource.CaseInsensitiveTerms = CaseInsensitiveTerms;
                mSource.OnMissingTranslation = OnMissingTranslation;
                mSource.mTerm_AppName = mTerm_AppName;

                mSource.GoogleLiveSyncIsUptoDate = GoogleLiveSyncIsUptoDate;
                mSource.Google_WebServiceURL = Google_WebServiceURL;
                mSource.Google_SpreadsheetKey = Google_SpreadsheetKey;
                mSource.Google_SpreadsheetName = Google_SpreadsheetName;
                mSource.Google_LastUpdatedVersion = Google_LastUpdatedVersion;
                mSource.GoogleUpdateFrequency = GoogleUpdateFrequency;
                mSource.GoogleUpdateDelay = GoogleUpdateDelay;
                
                mSource.Event_OnSourceUpdateFromGoogle += Event_OnSourceUpdateFromGoogle;

                if (mLanguages != null && mLanguages.Count>0)
                {
                    mSource.mLanguages.Clear();
                    mSource.mLanguages.AddRange(mLanguages);
                    mLanguages.Clear();
                }
                if (Assets != null && Assets.Count > 0)
                {
                    mSource.Assets.Clear();
                    mSource.Assets.AddRange(Assets);
                    Assets.Clear();
                }
                if (mTerms != null && mTerms.Count>0)
                {
                    mSource.mTerms.Clear();
                    for (int i=0; i<mTerms.Count; ++i)
                        mSource.mTerms.Add(mTerms[i]);
                    mTerms.Clear();
                }

                version = 1;

                Event_OnSourceUpdateFromGoogle = null;
            }
        }
    }
}