using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables
		private List<string> mTranslationTerms = new List<string>();
		private Dictionary<string, TranslationQuery> mTranslationRequests = new Dictionary<string, TranslationQuery> ();
        private bool mAppNameTerm_Expanded;

        private List<string> mLanguageCodePopupList;

		#endregion

		void OnGUI_Languages()
		{
			//GUILayout.Space(5);

			OnGUI_ShowMsg();

			OnGUI_LanguageList();

            OnGUI_StoreIntegration();

            GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("论翻译缺失 On Missing Translation:", "当一个术语还没有翻译成当前语言时，游戏中应该发生什么\nWhat should happen IN-GAME when a term is not yet translated to the current language?"), EditorStyles.boldLabel, GUILayout.Width(200));
                GUILayout.BeginVertical();
                    GUILayout.Space(7);
                    EditorGUILayout.PropertyField(mProp_OnMissingTranslation, GUITools.EmptyContent, GUILayout.Width(165));
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("在运行时卸载语言 Unload Languages At Runtime:", "当玩游戏时，插件将卸载所有未使用的语言，只在需要时加载它们\nWhen playing the game, the plugin will unload all unused languages and only load them when needed"), EditorStyles.boldLabel, GUILayout.Width(200));
                GUILayout.BeginVertical();
                    GUILayout.Space(7);
                    EditorGUILayout.PropertyField(mProp_AllowUnloadingLanguages, GUITools.EmptyContent, GUILayout.Width(165));
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();


            

            string firstLanguage = "";
			if (mLanguageSource.mLanguages.Count > 0)
				firstLanguage = " (" + mLanguageSource.mLanguages [0].Name + ")";
			
			GUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("默认语言:Default Language:", "当游戏开始时，这是将被使用的语言，直到玩家手动选择语言\nWhen the game starts this is the language that will be used until the player manually selects a language"), EditorStyles.boldLabel, GUILayout.Width(160));
				GUILayout.BeginVertical();
					GUILayout.Space(7);

            mProp_IgnoreDeviceLanguage.boolValue = EditorGUILayout.Popup(mProp_IgnoreDeviceLanguage.boolValue?1:0, new[]{"Device Language", "First in List"+firstLanguage}, GUILayout.ExpandWidth(true))==1;
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
        }

        #region GUI Languages

        void OnGUI_LanguageList()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.FlexibleSpace();
				GUILayout.Label ("Languages:", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				GUILayout.Label ("Code:", EditorStyles.miniLabel);
                GUILayout.Space(170);
			GUILayout.EndHorizontal();
			
			//--[ Language List ]--------------------------

			int IndexLanguageToDelete = -1;
			int LanguageToMoveUp = -1;
			int LanguageToMoveDown = -1;
            GUI.backgroundColor = Color.Lerp(GUITools.LightGray, Color.white, 0.5f);
            mScrollPos_Languages = GUILayout.BeginScrollView( mScrollPos_Languages, LocalizeInspector.GUIStyle_OldTextArea, GUILayout.MinHeight (200), /*GUILayout.MaxHeight(Screen.height),*/ GUILayout.ExpandHeight(false));
            GUI.backgroundColor = Color.white;

            if (mLanguageCodePopupList == null || mLanguageCodePopupList.Count==0)
            {
                mLanguageCodePopupList = GoogleLanguages.GetLanguagesForDropdown("", "");
                mLanguageCodePopupList.Sort();
                mLanguageCodePopupList.Insert(0, string.Empty);
            }

			for (int i=0, imax=mProp_Languages.arraySize; i<imax; ++i)
			{
				SerializedProperty Prop_Lang = mProp_Languages.GetArrayElementAtIndex(i);
				SerializedProperty Prop_LangName = Prop_Lang.FindPropertyRelative("Name");
                SerializedProperty Prop_LangCode = Prop_Lang.FindPropertyRelative("Code");
                SerializedProperty Prop_Flags    = Prop_Lang.FindPropertyRelative("Flags");
                bool isLanguageEnabled = (Prop_Flags.intValue & (int)eLanguageDataFlags.DISABLED)==0;

                GUI.color = isLanguageEnabled ? Color.white : new Color(1, 1, 1, 0.3f);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button ("X", "toolbarbutton", GUILayout.ExpandWidth(false)))
				{
					IndexLanguageToDelete = i;
				}
				
				GUILayout.BeginHorizontal(EditorStyles.toolbar);

				EditorGUI.BeginChangeCheck();
				string LanName = EditorGUILayout.TextField(Prop_LangName.stringValue, GUILayout.ExpandWidth(true));
				if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(LanName))
				{
					Prop_LangName.stringValue = LanName;
				}

                var currentCode = "[" + Prop_LangCode.stringValue + "]";

                if (isLanguageEnabled)
                {
                    int Index = Mathf.Max(0, mLanguageCodePopupList.FindIndex(c => c.Contains(currentCode)));
                    EditorGUI.BeginChangeCheck();
                    Index = EditorGUILayout.Popup(Index, mLanguageCodePopupList.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(60));
                    if (EditorGUI.EndChangeCheck() && Index >= 0)
                    {
                        currentCode = mLanguageCodePopupList[Index];
                        int i0 = currentCode.IndexOf("[");
                        int i1 = currentCode.IndexOf("]");
                        if (i0 >= 0 && i1 > i0)
                            Prop_LangCode.stringValue = currentCode.Substring(i0 + 1, i1 - i0 - 1);
                        else
                            Prop_LangCode.stringValue = string.Empty;
                    }
                    var rect = GUILayoutUtility.GetLastRect();
                    GUI.Label(rect, Prop_LangCode.stringValue, EditorStyles.toolbarPopup);
                }
                else
                {
                    GUILayout.Label(Prop_LangCode.stringValue, EditorStyles.toolbarPopup, GUILayout.Width(60));
                }

                GUILayout.EndHorizontal();

				GUI.enabled = i<imax-1;
				if (GUILayout.Button( "\u25BC", EditorStyles.toolbarButton, GUILayout.Width(18))) LanguageToMoveDown = i;
				GUI.enabled = i>0;
				if (GUILayout.Button( "\u25B2", EditorStyles.toolbarButton, GUILayout.Width(18))) LanguageToMoveUp = i;

                GUI.enabled = true;
                if (GUILayout.Button( new GUIContent("预览 Show", "预览该语言的所有本地化\nPreview all localizations into this language"), EditorStyles.toolbarButton, GUILayout.Width(35))) 
				{
					LocalizationManager.SetLanguageAndCode( LanName, Prop_LangCode.stringValue, false, true);
				}

				if (TestButtonArg( eTest_ActionType.Button_Languages_TranslateAll, i, new GUIContent("翻译 Translate", "翻译所有空词\nTranslate all empty terms"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) 
				{
                    GUITools.DelayedCall( () => TranslateAllToLanguage(LanName));
				}
				GUI.enabled = true;
                GUI.color = Color.white;

                EditorGUI.BeginChangeCheck();
				isLanguageEnabled = EditorGUILayout.Toggle(isLanguageEnabled, GUILayout.Width(15));

				var r = GUILayoutUtility.GetLastRect();
				GUI.Label(r, new GUIContent("", "启用/禁用语言\nEnable/Disable the language.\n禁用语言可用于存储数据值或避免显示仍在开发中的语言\nDisabled languages can be used to store data values or to avoid showing Languages that are stil under development"));

                if (EditorGUI.EndChangeCheck())
                {
                    Prop_Flags.intValue = (Prop_Flags.intValue & ~(int)eLanguageDataFlags.DISABLED) | (isLanguageEnabled ? 0 : (int)eLanguageDataFlags.DISABLED);
                }

                GUILayout.EndHorizontal();
			}
			
			GUILayout.EndScrollView();
			
			OnGUI_AddLanguage( mProp_Languages );

			if (mConnection_WWW!=null || mConnection_Text.Contains("Translating"))
			{
				// Connection Status Bar
				int time = (int)(Time.realtimeSinceStartup % 2 * 2.5);
				string Loading = mConnection_Text + ".....".Substring(0, time);
				GUI.color = Color.gray;
				GUILayout.BeginHorizontal(LocalizeInspector.GUIStyle_OldTextArea);
				GUILayout.Label (Loading, EditorStyles.miniLabel);
				GUI.color = Color.white;
                if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                {
					GoogleTranslation.CancelCurrentGoogleTranslations ();
                    StopConnectionWWW();
                }
				GUILayout.EndHorizontal();
				Repaint();
			}
			
			if (IndexLanguageToDelete>=0)
			{
				if (EditorUtility.DisplayDialog ("确认删除 Confirm delete", "您确定要删除所选语言吗\nAre you sure you want to delete the selected language", "Yes", "Cancel")) 
				{
					mLanguageSource.RemoveLanguage (mLanguageSource.mLanguages [IndexLanguageToDelete].Name);
					serializedObject.Update ();
					ParseTerms (true, false, false);
				}
			}

			if (LanguageToMoveUp>=0)   SwapLanguages( LanguageToMoveUp, LanguageToMoveUp-1 );
			if (LanguageToMoveDown>=0) SwapLanguages( LanguageToMoveDown, LanguageToMoveDown+1 );
		}

		void SwapLanguages( int iFirst, int iSecond )
		{
			serializedObject.ApplyModifiedProperties();
			LanguageSourceData Source = mLanguageSource;

			SwapValues( Source.mLanguages, iFirst, iSecond );
			foreach (TermData termData in Source.mTerms)
			{
				SwapValues ( termData.Languages, iFirst, iSecond );
				SwapValues ( termData.Flags, iFirst, iSecond );
			}
			serializedObject.Update();
		}

		void SwapValues( List<LanguageData> mList, int Index1, int Index2 )
		{
			LanguageData temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}
		void SwapValues( string[] mList, int Index1, int Index2 )
		{
			string temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}
		void SwapValues( byte[] mList, int Index1, int Index2 )
		{
			byte temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}

		
		void OnGUI_AddLanguage( SerializedProperty Prop_Languages)
		{
            //--[ Add Language Upper Toolbar ]-----------------

            GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			mLanguages_NewLanguage = EditorGUILayout.TextField("", mLanguages_NewLanguage, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUI.enabled = !string.IsNullOrEmpty (mLanguages_NewLanguage);
			if (TestButton(eTest_ActionType.Button_AddLanguageManual,"Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Prop_Languages.serializedObject.ApplyModifiedProperties();
				mLanguageSource.AddLanguage( mLanguages_NewLanguage, GoogleLanguages.GetLanguageCode(mLanguages_NewLanguage) );
				Prop_Languages.serializedObject.Update();
				mLanguages_NewLanguage = "";
                GUI.FocusControl(string.Empty);
            }
            GUI.enabled = true;
			
			GUILayout.EndHorizontal();


            //--[ Add Language Bottom Toolbar ]-----------------

			GUILayout.BeginHorizontal();
			
			//-- Language Dropdown -----------------
			string CodesToExclude = string.Empty;
			foreach (var LanData in mLanguageSource.mLanguages)
				CodesToExclude = string.Concat(CodesToExclude, "[", LanData.Code, "]");

			List<string> Languages = GoogleLanguages.GetLanguagesForDropdown(mLanguages_NewLanguage, CodesToExclude);

			GUI.changed = false;
			int index = EditorGUILayout.Popup(0, Languages.ToArray(), EditorStyles.toolbarDropDown);

			if (GUI.changed && index>=0)
			{
				mLanguages_NewLanguage = GoogleLanguages.GetFormatedLanguageName( Languages[index] );
			}
			
			
			if (TestButton(eTest_ActionType.Button_AddLanguageFromPopup, "Add", EditorStyles.toolbarButton, GUILayout.Width(50)) && index>=0)
			{
                Prop_Languages.serializedObject.ApplyModifiedProperties();
                mLanguages_NewLanguage = GoogleLanguages.GetFormatedLanguageName(Languages[index]);

                if (!string.IsNullOrEmpty(mLanguages_NewLanguage))
                    mLanguageSource.AddLanguage(mLanguages_NewLanguage, GoogleLanguages.GetLanguageCode(mLanguages_NewLanguage));
                Prop_Languages.serializedObject.Update();
                
                mLanguages_NewLanguage = "";
                GUI.FocusControl(string.Empty);
            }

            GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUI.color = Color.white;
		}


        void TranslateAllToLanguage (string lanName)
		{
			if (!GoogleTranslation.CanTranslate ()) 
			{
				ShowError ("WebService设置不正确或需要重新安装。\nWebService is not set correctly or needs to be reinstalled");
				return;
			}
            ClearErrors();
			int LanIndex = mLanguageSource.GetLanguageIndex (lanName);
			string code = mLanguageSource.mLanguages [LanIndex].Code;
            string googleCode = GoogleLanguages.GetGoogleLanguageCode(code);
            if (string.IsNullOrEmpty(googleCode))
            {
                ShowError("Language '" + code + "' 不支持谷歌翻译 is not supported by google translate");
                return;
            }
            googleCode = code;

            mTranslationTerms.Clear ();
			mTranslationRequests.Clear ();
			foreach (var termData in mLanguageSource.mTerms) 
			{
                if (termData.TermType != eTermType.Text)
                    continue;

				if (!string.IsNullOrEmpty(termData.Languages[LanIndex]))
					continue;
				
				string sourceCode, sourceText;
				FindTranslationSource( LanguageSourceData.GetKeyFromFullTerm(termData.Term), termData, code, null, out sourceText, out sourceCode );

				mTranslationTerms.Add (termData.Term);

				GoogleTranslation.CreateQueries(sourceText, sourceCode, googleCode, mTranslationRequests);   // can split plurals into several queries
			}

			if (mTranslationRequests.Count == 0) 
			{
				StopConnectionWWW ();
				return;
			}

			mConnection_WWW = null;
            mConnection_Text = "Translating"; if (mTranslationRequests.Count > 1) mConnection_Text += " (" + mTranslationRequests.Count + ")";
			mConnection_Callback = null;
			//EditorApplication.update += CheckForConnection;

			GoogleTranslation.Translate (mTranslationRequests, OnLanguageTranslated);
		}

		void OnLanguageTranslated( Dictionary<string, TranslationQuery> requests, string Error )
		{
			//Debug.Log (Result);

            //if (Result.Contains("Service invoked too many times"))
            //{
            //    TimeStartTranslation = EditorApplication.timeSinceStartup + 1;
            //    EditorApplication.update += DelayedStartTranslation;
            //    mConnection_Text = "Translating (" + mTranslationRequests.Count + ")";
            //    return;
            //}

			//if (!string.IsNullOrEmpty(Error))/* || !Result.Contains("<i2>")*/
		    //{
            //    Debug.LogError("WEB ERROR: " + Error);
			//	ShowError ("Unable to access Google or not valid request");
			//	return;
			//}

			ClearErrors();
            StopConnectionWWW();

            if (!string.IsNullOrEmpty(Error))
			{
				ShowError (Error);
                return;
			}

			if (requests.Values.Count == 0)
				return;
			
			var langCode = requests.Values.First().TargetLanguagesCode [0];
            //langCode = GoogleLanguages.GetGoogleLanguageCode(langCode);
			int langIndex = mLanguageSource.GetLanguageIndexFromCode (langCode, false);
            //if (langIndex >= 0)
            {
                foreach (var term in mTranslationTerms)
                {
                    var termData = mLanguageSource.GetTermData(term);
                    if (termData == null)
                        continue;
                    if (termData.TermType != eTermType.Text)
                        continue;
                    //if (termData.Languages.Length <= langIndex)
                      //  continue;

                    string sourceCode, sourceText;
                    FindTranslationSource(LanguageSourceData.GetKeyFromFullTerm(termData.Term), termData, langCode, null, out sourceText, out sourceCode);

                    string result = GoogleTranslation.RebuildTranslation(sourceText, mTranslationRequests, langCode);               // gets the result from google and rebuilds the text from multiple queries if its is plurals

                    termData.Languages[langIndex] = result;
                }
            }

			mTranslationTerms.Clear ();
			mTranslationRequests.Clear ();
			StopConnectionWWW ();
		}

		#endregion

        #region Store Integration

        void OnGUI_StoreIntegration()
        {
            GUIStyle lstyle = new GUIStyle (EditorStyles.label);
            lstyle.richText = true;

            GUILayout.BeginHorizontal ();
                GUILayout.Label (new GUIContent("Store Integration:", "设置商店来检测游戏的本地化，Android为每种语言添加字符串xml。Ios修改了Info列表\nSetups the stores to detect that the game has localization, Android adds strings.xml for each language. IOS modifies the Info.plist"), EditorStyles.boldLabel, GUILayout.Width(160));
				GUILayout.FlexibleSpace();

					GUILayout.Label( new GUIContent( "<color=green><size=16>\u2713</size></color>  IOS", "Setups the stores to show in iTunes and the Appstore all the languages that this app supports, also localizes the app name if available" ), lstyle, GUILayout.Width( 90 ) );
					GUILayout.Label( new GUIContent( "<color=green><size=16>\u2713</size></color>  Android", "Setups the stores to show in GooglePlay all the languages this app supports, also localizes the app name if available" ), lstyle, GUILayout.Width( 90 ) );
            GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal();
                mAppNameTerm_Expanded = GUILayout.Toggle(mAppNameTerm_Expanded, new GUIContent( "应用名称翻译:\nApp Name translations:", "根据设备的语言，游戏应该如何命名\nHow should the game be named in the devices based on their language" ), EditorStyles.foldout, GUILayout.Width( 160 ) );

                GUILayout.Label("", GUILayout.ExpandWidth(true));
                var rect = GUILayoutUtility.GetLastRect();
                TermsPopup_Drawer.ShowGUI( rect, mProp_AppNameTerm, GUITools.EmptyContent, mLanguageSource);

                if (GUILayout.Button("New Term", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    AddLocalTerm("App_Name");
                    mProp_AppNameTerm.stringValue = "App_Name";
                    mAppNameTerm_Expanded = true;
                }
			GUILayout.EndHorizontal();

            if (mAppNameTerm_Expanded)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);

                GUILayout.BeginVertical("Box");
                    var termName = mProp_AppNameTerm.stringValue;
                    if (!string.IsNullOrEmpty(termName))
                    {
                        var termData = LocalizationManager.GetTermData(termName);
                        if (termData != null)
                            OnGUI_Keys_Languages(mProp_AppNameTerm.stringValue, ref termData, null, true, mLanguageSource);
                    }
                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                        GUILayout.Label("<b>Default App Name:</b>", lstyle, GUITools.DontExpandWidth);
                        GUILayout.Label(Application.productName);
                    GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
		}

 		#endregion
	}
}