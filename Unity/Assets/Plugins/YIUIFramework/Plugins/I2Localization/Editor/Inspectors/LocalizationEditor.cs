using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	public abstract partial class LocalizationEditor : Editor
	{
		#region Variables
		
		SerializedProperty 	mProp_Assets, mProp_Languages, 
							mProp_Google_WebServiceURL, mProp_GoogleUpdateFrequency, mProp_GoogleUpdateDelay, mProp_Google_SpreadsheetKey, mProp_Google_SpreadsheetName, mProp_Google_Password,
                            mProp_Spreadsheet_LocalFileName, mProp_Spreadsheet_LocalCSVSeparator, mProp_CaseInsensitiveTerms, mProp_Spreadsheet_LocalCSVEncoding,
							mProp_OnMissingTranslation, mProp_AppNameTerm, mProp_IgnoreDeviceLanguage, mProp_Spreadsheet_SpecializationAsRows, mProp_GoogleInEditorCheckFrequency,
                            mProp_HighlightLocalizedTargets, mProp_GoogleLiveSyncIsUptoDate, mProp_AllowUnloadingLanguages, mProp_GoogleUpdateSynchronization;

		public static LanguageSourceData mLanguageSource;
        public static Object mLanguageSourceObject;
        public static LocalizationEditor mLanguageSourceEditor;
        public static Editor mCurrentInspector;

        static bool mIsParsing;  // This is true when the editor is opening several scenes to avoid reparsing objects

		#endregion
		
		#region Variables GUI
		
		GUIStyle Style_ToolBar_Big, Style_ToolBarButton_Big;

 		
		public GUISkin CustomSkin;

		static Vector3 mScrollPos_Languages;
		public static string mLanguages_NewLanguage = "";

		#endregion

        #region Styles

        public static GUIStyle Style_WrapTextField {
            get{ 
                if (mStyle_WrapTextField==null)
                {
                    mStyle_WrapTextField = new GUIStyle(EditorStyles.textArea);
                    mStyle_WrapTextField.wordWrap = true;
                    mStyle_WrapTextField.fixedHeight = 0;
                }
                return mStyle_WrapTextField;
            }
        }
        static GUIStyle mStyle_WrapTextField;

        #endregion

		#region Inspector

		public void Custom_OnEnable( LanguageSourceData sourceData, SerializedProperty propSource)
		{
			bool ForceParse = mLanguageSource != sourceData;

            mLanguageSource = sourceData;
            mLanguageSourceEditor = this;
            mCurrentInspector = this;

            if (!LocalizationManager.Sources.Contains(mLanguageSource))
				LocalizationManager.UpdateSources();

            mProp_Assets                           = propSource.FindPropertyRelative("Assets");
            mProp_Languages                        = propSource.FindPropertyRelative("mLanguages");
            mProp_Google_WebServiceURL             = propSource.FindPropertyRelative("Google_WebServiceURL");
            mProp_GoogleUpdateFrequency            = propSource.FindPropertyRelative("GoogleUpdateFrequency");
            mProp_GoogleUpdateSynchronization      = propSource.FindPropertyRelative("GoogleUpdateSynchronization");
            mProp_GoogleInEditorCheckFrequency     = propSource.FindPropertyRelative("GoogleInEditorCheckFrequency");
            mProp_GoogleUpdateDelay                = propSource.FindPropertyRelative("GoogleUpdateDelay");
            mProp_Google_SpreadsheetKey            = propSource.FindPropertyRelative("Google_SpreadsheetKey");
            mProp_Google_SpreadsheetName           = propSource.FindPropertyRelative("Google_SpreadsheetName");
            mProp_Google_Password                  = propSource.FindPropertyRelative("Google_Password");            
            mProp_CaseInsensitiveTerms             = propSource.FindPropertyRelative("CaseInsensitiveTerms");
            mProp_Spreadsheet_LocalFileName        = propSource.FindPropertyRelative("Spreadsheet_LocalFileName");
            mProp_Spreadsheet_LocalCSVSeparator    = propSource.FindPropertyRelative("Spreadsheet_LocalCSVSeparator");
            mProp_Spreadsheet_LocalCSVEncoding     = propSource.FindPropertyRelative("Spreadsheet_LocalCSVEncoding");
            mProp_Spreadsheet_SpecializationAsRows = propSource.FindPropertyRelative("Spreadsheet_SpecializationAsRows");
            mProp_OnMissingTranslation             = propSource.FindPropertyRelative("OnMissingTranslation");
			mProp_AppNameTerm					   = propSource.FindPropertyRelative("mTerm_AppName");
			mProp_IgnoreDeviceLanguage			   = propSource.FindPropertyRelative("IgnoreDeviceLanguage");
            mProp_GoogleLiveSyncIsUptoDate         = propSource.FindPropertyRelative("GoogleLiveSyncIsUptoDate");
            mProp_AllowUnloadingLanguages          = propSource.FindPropertyRelative("_AllowUnloadingLanguages");

            if (!mIsParsing)
			{
				if (string.IsNullOrEmpty(mLanguageSource.Google_SpreadsheetKey))
					mSpreadsheetMode = eSpreadsheetMode.Local;
				else
					mSpreadsheetMode = eSpreadsheetMode.Google;

				mCurrentViewMode = mLanguageSource.mLanguages.Count>0 ? eViewMode.Keys : eViewMode.Languages;

				UpdateSelectedKeys();

                if (ForceParse || mParsedTerms.Count < mLanguageSource.mTerms.Count)
                {
                    mSelectedCategories.Clear();
                    ParseTerms(true, false, true);
                }
			}
            ScheduleUpdateTermsToShowInList();
			LoadSelectedCategories();
            //UpgradeManager.EnablePlugins();
        }

		void OnDisable()
		{
			//LocalizationManager.LocalizeAll();
			SaveSelectedCategories();
            mLanguageSourceEditor = null;
            if (mCurrentInspector==this) mCurrentInspector = null;
        }


        void UpdateSelectedKeys()
		{
			// Remove all keys that are not in this source
			string trans;
			for (int i=mSelectedKeys.Count-1; i>=0; --i)
				if (!mLanguageSource.TryGetTranslation(mSelectedKeys[i], out trans))
					mSelectedKeys.RemoveAt(i);

			// Remove all Categories that are not in this source
			/*var mCateg = mLanguageSource.GetCategories();
			for (int i=mSelectedCategories.Count-1; i>=0; --i)
				if (!mCateg.Contains(mSelectedCategories[i]))
					mSelectedCategories.RemoveAt(i);
			if (mSelectedCategories.Count==0)
				mSelectedCategories = mCateg;*/

			if (mSelectedScenes.Count==0)
				mSelectedScenes.Add (Editor_GetCurrentScene());
        }

        public override void OnInspectorGUI()
		{
			// Load Test:
			/*if (mLanguageSource.mTerms.Count<40000)
			{
				mLanguageSource.mTerms.Clear();
				for (int i=0; i<40020; ++i)
					mLanguageSource.AddTerm("ahh"+i.ToString("00000"), eTermType.Text, false);
				mLanguageSource.UpdateDictionary();
			}*/
            //Profiler.maxNumberOfSamplesPerFrame = -1;    // REMOVE ---------------------------------------------------

			mIsParsing = false;

			//#if UNITY_5_6_OR_NEWER
			//	serializedObject.UpdateIfRequiredOrScript();
			//#else
			//	serializedObject.UpdateIfDirtyOrScript();
			//#endif

			if (mLanguageSource.mTerms.Count<1000)
				Undo.RecordObject(target, "LanguageSource");

			//GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
			//GUILayout.BeginVertical(LocalizeInspector.GUIStyle_Background);
			//GUI.backgroundColor = Color.white;
			
			if (GUILayout.Button("Language Source", LocalizeInspector.GUIStyle_Header))
			{
				Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
			}

			InitializeStyles();

			GUILayout.Space(10);

            //GUI.backgroundColor = Color.Lerp(GUITools.LightGray, Color.white, 0.5f);
            //GUILayout.BeginVertical(LocalizeInspector.GUIStyle_Background);
            //GUI.backgroundColor = Color.white;
            OnGUI_Main();
            //GUILayout.EndVertical();

            GUILayout.Space (10);
			GUILayout.FlexibleSpace();

            GUITools.OnGUI_Footer("I2 Localization", LocalizationManager.GetVersion(), LocalizeInspector.HelpURL_forum, LocalizeInspector.HelpURL_Documentation, LocalizeInspector.HelpURL_AssetStore);

			//GUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
            if (Event.current.type == EventType.Repaint)
            {
                mTestAction = eTest_ActionType.None;
                mTestActionArg = null;
                mTestActionArg2 = null;
            }
        }

		#endregion
	}
}