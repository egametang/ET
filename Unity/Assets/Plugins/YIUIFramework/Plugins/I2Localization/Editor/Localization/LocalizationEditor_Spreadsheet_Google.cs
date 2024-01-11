using System;
using System.Collections.Generic;
using System.Text;
using I2.Loc.SimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables

		public static Dictionary<string, string> mGoogleSpreadsheets = new Dictionary<string, string>(StringComparer.Ordinal);

		public UnityWebRequest mConnection_WWW;

        delegate void fnConnectionCallback(string Result, string Error);
        event fnConnectionCallback mConnection_Callback;
		//float mConnection_TimeOut;

		string mConnection_Text = string.Empty;

		string mWebService_Status;

		#endregion
		
		#region GUI
		
		void OnGUI_Spreadsheet_Google()
		{
			GUILayout.Space(20);

#if UNITY_WEBPLAYER
			mConnection_Text = string.Empty;
			EditorGUILayout.HelpBox("在WebPlayer模式下不支持Google同步\nGoogle Synchronization is not supported when in WebPlayer mode." + mConnection_Text, MessageType.Info);

			mProp_GoogleUpdateFrequency.enumValueIndex = mProp_GoogleUpdateFrequency.enumValueIndex;  // to avoid the warning "unused"
            mProp_GoogleUpdateSynchronization.enumValueIndex = mProp_GoogleUpdateSynchronization.enumValueIndex;
#else

            OnGUI_GoogleCredentials();
			
			OnGUI_ShowMsg();

			if (string.IsNullOrEmpty(mProp_Google_WebServiceURL.stringValue))
				return;

			if (mWebService_Status == "Offline")
				return;

			GUILayout.Space(20);

			GUI.backgroundColor = Color.Lerp(Color.gray, Color.white, 0.5f);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height (1));
			GUI.backgroundColor = Color.white;
				GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("  Password", "This should match the value of the LocalizationPassword variable in the WebService Script in your Google Drive"), GUILayout.Width(108));
                mProp_Google_Password.stringValue = EditorGUILayout.TextField(mProp_Google_Password.stringValue, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                OnGUI_GoogleSpreadsheetsInGDrive();
			GUILayout.EndVertical();

			if (mConnection_WWW!=null)
			{
				// Connection Status Bar
				int time = (int)(Time.realtimeSinceStartup % 2 * 2.5);
				string Loading = mConnection_Text + ".....".Substring(0, time);
				GUI.color = Color.gray;
				GUILayout.BeginHorizontal(LocalizeInspector.GUIStyle_OldTextArea);
				GUILayout.Label (Loading, EditorStyles.miniLabel);
				GUI.color = Color.white;
				if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
					StopConnectionWWW();
				GUILayout.EndHorizontal();
				Repaint();
			}
			//else
			//	GUILayout.Space(10);


			EditorGUI.BeginChangeCheck();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
					LanguageSourceData.eGoogleUpdateFrequency GoogleUpdateFrequency = (LanguageSourceData.eGoogleUpdateFrequency)mProp_GoogleUpdateFrequency.enumValueIndex;
                    GoogleUpdateFrequency = (LanguageSourceData.eGoogleUpdateFrequency)EditorGUILayout.EnumPopup("Auto Update Frequency", GoogleUpdateFrequency, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        mProp_GoogleUpdateFrequency.enumValueIndex = (int)GoogleUpdateFrequency;
                    }

					GUILayout.Space(10);
					GUILayout.Label("Delay:");
						mProp_GoogleUpdateDelay.floatValue = EditorGUILayout.FloatField(mProp_GoogleUpdateDelay.floatValue, GUILayout.Width(30));
					GUILayout.Label("secs");

			GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
					var GoogleInEditorCheckFrequency = (LanguageSourceData.eGoogleUpdateFrequency)mProp_GoogleInEditorCheckFrequency.enumValueIndex;
                    EditorGUI.BeginChangeCheck();
                    GoogleInEditorCheckFrequency = (LanguageSourceData.eGoogleUpdateFrequency)EditorGUILayout.EnumPopup(new GUIContent("In-Editor Check Frequency", "How often the editor will verify that the Spreadsheet is up-to-date with the LanguageSource. Having un-synchronized Spreadsheets can lead to issues when playing in the device as the download data will override the one in the build"), GoogleInEditorCheckFrequency, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        mProp_GoogleInEditorCheckFrequency.enumValueIndex = (int)GoogleInEditorCheckFrequency;
                    }
					GUILayout.Space(122);
			GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Update Synchronization", GUILayout.Width(180));
                EditorGUI.BeginChangeCheck();
                LanguageSourceData.eGoogleUpdateSynchronization GoogleUpdateSynchronization = (LanguageSourceData.eGoogleUpdateSynchronization)mProp_GoogleUpdateSynchronization.enumValueIndex;
                GoogleUpdateSynchronization = (LanguageSourceData.eGoogleUpdateSynchronization)EditorGUILayout.EnumPopup(GoogleUpdateSynchronization, GUILayout.Width(178));
                if (EditorGUI.EndChangeCheck())
                {
                    mProp_GoogleUpdateSynchronization.enumValueIndex = (int)GoogleUpdateSynchronization;
                }
            GUILayout.EndHorizontal();

			GUILayout.Space(5);

			GUI.changed = false;
			bool OpenDataSourceAfterExport = EditorPrefs.GetBool("I2Loc OpenDataSourceAfterExport", true);

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				OpenDataSourceAfterExport = GUILayout.Toggle(OpenDataSourceAfterExport, "Open Spreadsheet after Export");
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			if (GUI.changed)
			{
				GUI.changed = false;
				EditorPrefs.SetBool("I2Loc OpenDataSourceAfterExport", OpenDataSourceAfterExport);
			}

#endif

			GUILayout.Space(5);
		}
		
		void OnGUI_GoogleCredentials()
		{
			GUI.enabled = mConnection_WWW==null;

			GUI.changed = false;

			string WebServiceHelp = "The web service is a script running on the google drive where the spreadsheet you want to use is located.\nThat script allows the game to synchronize the localization even after the game is published.";

			GUILayout.BeginHorizontal();
				GUILayout.Label (new GUIContent("Web Service URL:", WebServiceHelp),  GUILayout.Width(110));

				GUI.SetNextControlName ("WebServiceURL");
				mProp_Google_WebServiceURL.stringValue = EditorGUILayout.TextField(mProp_Google_WebServiceURL.stringValue);

				if (!string.IsNullOrEmpty(mWebService_Status))
				{
					if (mWebService_Status=="Online")
					{
						GUI.color = Color.green;
						GUILayout.Label( "", GUILayout.Width(17));
						Rect r = GUILayoutUtility.GetLastRect(); r.xMin += 3; r.yMin-= 3; r.xMax+= 2; r.yMax+=2;
						GUI.Label( r, new GUIContent("\u2713", "Online"), EditorStyles.whiteLargeLabel);
						GUI.color = Color.white;
					}
					else
					if (mWebService_Status=="Offline")
					{
						GUI.color = Color.red;
						GUILayout.Label( "", GUILayout.Width(17));
						Rect r = GUILayoutUtility.GetLastRect(); r.xMin += 3; r.yMin-= 3; r.xMax+= 2; r.yMax+=2;
						GUI.Label( r, new GUIContent("\u2717", mWebService_Status), EditorStyles.whiteLargeLabel);
						GUI.color = Color.white;
					}
					else
					if (mWebService_Status=="UnsupportedVersion")
					{
						Rect rect = GUILayoutUtility.GetLastRect();
						float Width = 15;
						rect.xMin = rect.xMax+1;
						rect.xMax = rect.xMin + rect.height;
                        GUITools.DrawSkinIcon(rect, "CN EntryWarnIcon", "CN EntryWarn");
						GUI.Label(rect, new GUIContent("\u2717", "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version."));
						GUILayout.Space (Width);
					}
				}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				GUILayout.Space (118);
				if (GUILayout.Button(new GUIContent("Install", "This opens the Web Service Script and shows you steps to install and authorize it on your Google Drive"), EditorStyles.toolbarButton))
				{
    					ClearErrors();
                        Application.OpenURL("https://script.google.com/d/1zcsLSmq3Oddd8AsLuoKNDG1Y0eYBOHzyvGT7v94u1oN6igmsZb_PJzEm/newcopy");  // V5
                        //Application.OpenURL("https://goo.gl/RBCO0o");  // V4:https://script.google.com/d/1T7e5_40NcgRyind-yeg4PAkHz9TNZJ22F4RcbOvCpAs03JNf1vKNNTZB/newcopy
                        //Application.OpenURL("https://goo.gl/wFSbv2");// V3:https://script.google.com/d/1CxQDSXflsXRaH3M7xGfrIDrFwOIHWPsYTWi4mRZ_k77nyIInTgIk63Kd/newcopy");
                }
                if (GUILayout.Button("Verify", EditorStyles.toolbarButton))
				{
					ClearErrors();
					VerifyGoogleService(mProp_Google_WebServiceURL.stringValue);
					GUI.changed = false;
				}
			GUILayout.EndHorizontal();


			if (string.IsNullOrEmpty(mProp_Google_WebServiceURL.stringValue))
			{
				EditorGUILayout.HelpBox(WebServiceHelp, MessageType.Info);
			}

			if (GUI.changed)
			{
				if (string.IsNullOrEmpty(mProp_Google_WebServiceURL.stringValue))
				{
					mProp_Google_SpreadsheetKey.stringValue = string.Empty;
					mProp_Google_SpreadsheetName.stringValue = string.Empty;
				}							


				// If the web service changed then clear the cached spreadsheet keys
				mGoogleSpreadsheets.Clear();
				
				GUI.changed = false;
				ClearErrors();
			}
			GUI.enabled = true;
		}
		
		void OnGUI_GoogleSpreadsheetsInGDrive()
		{
			GUI.enabled = mConnection_WWW==null;

			string[] Spreadsheets;
			string[] SpreadsheetsKey;
			if (mGoogleSpreadsheets.Count>0 || string.IsNullOrEmpty(mProp_Google_SpreadsheetKey.stringValue))
			{
				Spreadsheets = new List<string>(mGoogleSpreadsheets.Keys).ToArray();
				SpreadsheetsKey = new List<string>(mGoogleSpreadsheets.Values).ToArray();
			}
			else
			{
				Spreadsheets = new[]{mProp_Google_SpreadsheetName.stringValue ?? string.Empty};
				SpreadsheetsKey = new[]{mProp_Google_SpreadsheetKey.stringValue ?? string.Empty};
			}
			int mSpreadsheetIndex = Array.IndexOf(SpreadsheetsKey, mProp_Google_SpreadsheetKey.stringValue);

			//--[ Spreadsheets ]------------------
			GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				GUILayout.Label ("In Google Drive:", GUILayout.Width(100));

				GUI.changed = false;
				GUI.enabled = Spreadsheets != null && Spreadsheets.Length>0;
				mSpreadsheetIndex = EditorGUILayout.Popup(mSpreadsheetIndex, Spreadsheets, EditorStyles.toolbarPopup);
				if (GUI.changed && mSpreadsheetIndex >= 0)
				{
					mProp_Google_SpreadsheetKey.stringValue = SpreadsheetsKey[mSpreadsheetIndex];
					mProp_Google_SpreadsheetName.stringValue = Spreadsheets[mSpreadsheetIndex];
					GUI.changed = false;
				}
				GUI.enabled = true;

				GUI.enabled = !string.IsNullOrEmpty(mProp_Google_SpreadsheetKey.stringValue) && mConnection_WWW==null;
				if (GUILayout.Button("X", EditorStyles.toolbarButton,GUILayout.ExpandWidth(false)))
					mProp_Google_SpreadsheetKey.stringValue = string.Empty;
				GUI.enabled = true;
				GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(2);

			//--[ Spreadsheets Operations ]------------------
			GUILayout.BeginHorizontal();
				GUILayout.Space(114);
				if (GUILayout.Button("New", EditorStyles.toolbarButton,GUILayout.ExpandWidth(true)))
					Google_NewSpreadsheet();

				GUI.enabled = !string.IsNullOrEmpty(mProp_Google_SpreadsheetKey.stringValue) && mConnection_WWW==null;
				if (GUILayout.Button("Open", EditorStyles.toolbarButton,GUILayout.ExpandWidth(true)))
					OpenGoogleSpreadsheet(mProp_Google_SpreadsheetKey.stringValue);					
				GUI.enabled = mConnection_WWW==null;

				GUILayout.Space(5);

				if (TestButton(eTest_ActionType.Button_GoogleSpreadsheet_RefreshList, "Refresh", EditorStyles.toolbarButton,GUILayout.ExpandWidth(true)))
					EditorApplication.update+=Google_FindSpreadsheets;

				GUILayout.Space(10);
			GUILayout.EndHorizontal();

			GUILayout.Space(15);

			if (!string.IsNullOrEmpty(mProp_Google_SpreadsheetKey.stringValue))
				OnGUI_GoogleButtons_ImportExport( mProp_Google_SpreadsheetKey.stringValue );

			GUI.enabled = true;
		}


        private void OnGUI_ImportButtons()
        {
            eSpreadsheetUpdateMode Mode = SynchronizationButtons("Import");
            if (Mode != eSpreadsheetUpdateMode.None || InTestAction(eTest_ActionType.Button_GoogleSpreadsheet_Import))
            {
                if (mTestAction == eTest_ActionType.Button_GoogleSpreadsheet_Import)
                    Mode = (eSpreadsheetUpdateMode)mTestActionArg;

                serializedObject.ApplyModifiedProperties();

                var modeCopy = Mode;
                GUITools.DelayedCall(() => Import_Google(modeCopy));
            }
        }

        private void OnGUI_ExportButtons()
        {
            eSpreadsheetUpdateMode Mode = SynchronizationButtons("Export");
            if (Mode != eSpreadsheetUpdateMode.None || InTestAction(eTest_ActionType.Button_GoogleSpreadsheet_Export))
            {
                if (mTestAction == eTest_ActionType.Button_GoogleSpreadsheet_Export)
                    Mode = (eSpreadsheetUpdateMode)mTestActionArg;

                serializedObject.ApplyModifiedProperties();

                var modeCopy = Mode;
                GUITools.DelayedCall(() => Export_Google(modeCopy));
            }
        }

        void OnGUI_GoogleButtons_ImportExport( string SpreadsheetKey )
		{
			GUI.enabled = !string.IsNullOrEmpty(SpreadsheetKey) && mConnection_WWW==null;

            bool vertical = EditorGUIUtility.currentViewWidth < 450;

            if (vertical)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                OnGUI_ImportButtons();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                OnGUI_ExportButtons();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    OnGUI_ImportButtons();
                    GUILayout.FlexibleSpace();
                    OnGUI_ExportButtons();
                    GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }


            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth += 10;
                EditorGUILayout.PropertyField(mProp_Spreadsheet_SpecializationAsRows, new GUIContent("Show Specializations as Rows", "true: Make each specialization a separate row (e.g. Term[VR]..., Term[Touch]....\nfalse: Merge specializations into same cell separated by [i2s_XXX]"));
                EditorGUIUtility.labelWidth -= 10;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);


            GUI.enabled = true;
		}

		eSpreadsheetUpdateMode SynchronizationButtons( string Operation, bool ForceReplace = false )
		{
			eSpreadsheetUpdateMode Result = eSpreadsheetUpdateMode.None;
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Width (1));
			GUI.backgroundColor = Color.white;

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label(Operation, EditorStyles.miniLabel);
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			
				GUILayout.BeginHorizontal();
					if (GUILayout.Button( "替换 Replace", EditorStyles.toolbarButton, GUILayout.Width(100)))
						Result = eSpreadsheetUpdateMode.Replace;

					if (ForceReplace) GUI.enabled = false;
					if (GUILayout.Button( "合并 Merge", EditorStyles.toolbarButton, GUILayout.Width(100))) 
						Result = eSpreadsheetUpdateMode.Merge;
						
					if (GUILayout.Button( "新建 Add New", EditorStyles.toolbarButton, GUILayout.Width(100)))
						Result = eSpreadsheetUpdateMode.AddNewTerms;
					GUI.enabled = mConnection_WWW==null;
					GUILayout.Space(1);
				GUILayout.EndHorizontal();

				GUILayout.Space(2);
			GUILayout.EndVertical();

            if (Result != eSpreadsheetUpdateMode.None)
                ClearErrors();

			return Result;
		}
		#endregion

		void VerifyGoogleService( string WebServiceURL )
		{
			#if UNITY_WEBPLAYER
			ShowError ("Contacting google translation is not yet supported on WebPlayer" );
			#else
			StopConnectionWWW();
			mWebService_Status = null;	
			mConnection_WWW = UnityWebRequest.Get(WebServiceURL + "?action=Ping");
            I2Utils.SendWebRequest(mConnection_WWW);
            mConnection_Callback = OnVerifyGoogleService;
			EditorApplication.update += CheckForConnection;
			mConnection_Text = "Verifying Web Service";
			//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			#endif
		}
		
		void OnVerifyGoogleService( string Result, string Error )
		{
			if (Result.Contains("Authorization is required to perform that action"))
			{
				ShowWarning("You need to authorize the webservice before using it. Check the steps 4 and 5 in the WebService Script");
				mWebService_Status = "Offline";
				return;
			}

            try
            {
                var data = JSON.Parse(Result).AsObject;
				int version = 0;
				if (!int.TryParse(data["script_version"], out version))
					version = 0;
                int requiredVersion = LocalizationManager.GetRequiredWebServiceVersion();

                if (requiredVersion == version)
                {
                    mWebService_Status = "Online";
                    ClearErrors();
                }
                else
                {
                    mWebService_Status = "UnsupportedVersion";
                    ShowError("The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version.");
                }
            }
            catch (Exception)
            {
                ShowError("Unable to access the WebService");
                mWebService_Status = "Offline";
            }
		}


		void Export_Google( eSpreadsheetUpdateMode UpdateMode )
		{
			StopConnectionWWW();
			LanguageSourceData source = GetSourceData();
			mConnection_WWW = source.Export_Google_CreateWWWcall( UpdateMode );
			if (mConnection_WWW==null)
			{
				OnExported_Google(string.Empty, "WebPlayer can't contact Google");
			}
			else
			{
				mConnection_Callback = OnExported_Google;
				EditorApplication.update += CheckForConnection;
				mConnection_Text = "Uploading spreadsheet";
				//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			}
        }

        void OnExported_Google( string Result, string Error )
		{
            // Checkf or error, but discard the "necessary data rewind wasn't possible" as thats not a real error, just a bug in Unity with POST redirects
            if (!string.IsNullOrEmpty(Error) && !Error.Contains("rewind"))
			{
				Debug.Log (Error);
				ShowError("Unable to access google");
				return;
			}

            if (EditorPrefs.GetBool("I2Loc OpenDataSourceAfterExport", true) && !string.IsNullOrEmpty(GetSourceData().Google_SpreadsheetName))
				OpenGoogleSpreadsheet(GetSourceData().Google_SpreadsheetKey );
            mProp_GoogleLiveSyncIsUptoDate.boolValue = true;
        }

		static void OpenGoogleSpreadsheet( string SpreadsheetKey )
		{
			ClearErrors();
			string SpreadsheetUrl = "https://docs.google.com/spreadsheet/ccc?key=" + SpreadsheetKey;
			Application.OpenURL(SpreadsheetUrl);
		}

        public abstract LanguageSourceData GetSourceData();


        void Import_Google( eSpreadsheetUpdateMode UpdateMode )
		{
			StopConnectionWWW();
            LanguageSourceData source = GetSourceData();
			mConnection_WWW = source.Import_Google_CreateWWWcall(true, false);
			if (mConnection_WWW==null)
			{
				OnImported_Google(string.Empty, "Unable to import from google", eSpreadsheetUpdateMode.Replace);
			}
			else
			{
				mConnection_Callback=null;
				switch (UpdateMode)
				{
					case eSpreadsheetUpdateMode.Replace : mConnection_Callback += OnImported_Google_Replace; break;
					case eSpreadsheetUpdateMode.Merge : mConnection_Callback += OnImported_Google_Merge; break;
					case eSpreadsheetUpdateMode.AddNewTerms : mConnection_Callback += OnImported_Google_AddNewTerms; break;
				}
				EditorApplication.update += CheckForConnection;
				mConnection_Text = "Downloading spreadsheet";
				//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			}
		}

		void OnImported_Google_Replace( string Result, string Error ) 	{ OnImported_Google(Result, Error, eSpreadsheetUpdateMode.Replace); }
		void OnImported_Google_Merge( string Result, string Error ) 		{ OnImported_Google(Result, Error, eSpreadsheetUpdateMode.Merge); }
		void OnImported_Google_AddNewTerms( string Result, string Error ) { OnImported_Google(Result, Error, eSpreadsheetUpdateMode.AddNewTerms); }

		void OnImported_Google( string Result, string Error, eSpreadsheetUpdateMode UpdateMode )
		{
			if (!string.IsNullOrEmpty(Error))
			{
                Debug.Log(Error);
				ShowError("Unable to access google");
				return;
			}
			LanguageSourceData source = GetSourceData();
			string ErrorMsg = source.Import_Google_Result(Result, UpdateMode);
			bool HasErrors = !string.IsNullOrEmpty(ErrorMsg);
			if (HasErrors)
				ShowError(ErrorMsg);

			serializedObject.Update();
			ParseTerms(true, false, !HasErrors);
			mSelectedKeys.Clear ();
			mSelectedCategories.Clear();
			ScheduleUpdateTermsToShowInList();
			mLanguageSource.GetCategories(false, mSelectedCategories);

			EditorUtility.SetDirty (target);
			AssetDatabase.SaveAssets();
		}

		void CheckForConnection()
		{
			if (mConnection_WWW!=null && mConnection_WWW.isDone)
			{
				fnConnectionCallback callback = mConnection_Callback;
				string Result = string.Empty;
				string Error = mConnection_WWW.error;

				if (string.IsNullOrEmpty(Error))
				{
					Result = Encoding.UTF8.GetString(mConnection_WWW.downloadHandler.data); //mConnection_WWW.text;
				}

				StopConnectionWWW();
				if (callback!=null)
					callback(Result, Error);
			}
            /*else
			if (Time.realtimeSinceStartup > mConnection_TimeOut+30)
			{
				fnConnectionCallback callback = mConnection_Callback;
				StopConnectionWWW();
				if (callback!=null)
					callback(string.Empty, "Time Out");
			}*/
        }

        void StopConnectionWWW()
		{
			EditorApplication.update -= CheckForConnection;				
			mConnection_WWW = null;
			mConnection_Callback = null;
			mConnection_Text = string.Empty;
		}
		
		#region New Spreadsheet

		void Google_NewSpreadsheet()
		{
			#if UNITY_WEBPLAYER
			ShowError ("Contacting google translation is not yet supported on WebPlayer" );
			#else

			ClearErrors();
			string SpreadsheetName;

            LanguageSourceData source = GetSourceData();
            if (source.IsGlobalSource())
				SpreadsheetName = string.Format("{0} Localization", PlayerSettings.productName);
			else
				SpreadsheetName = string.Format("{0} {1} {2}", PlayerSettings.productName, Editor_GetCurrentScene(), source.ownerObject.name);

			string query =  mProp_Google_WebServiceURL.stringValue + "?action=NewSpreadsheet&name=" + Uri.EscapeDataString(SpreadsheetName) + "&password="+ Uri.EscapeDataString(mProp_Google_Password.stringValue);

			mConnection_WWW = UnityWebRequest.Get(query);
            I2Utils.SendWebRequest(mConnection_WWW);
            mConnection_Callback = Google_OnNewSpreadsheet;
			EditorApplication.update += CheckForConnection;
			mConnection_Text = "Creating Spreadsheet";
			//mConnection_TimeOut = Time.realtimeSinceStartup + 10;
			#endif
		}

		void Google_OnNewSpreadsheet( string Result, string Error )
		{
			if (!string.IsNullOrEmpty(Error))
			{
				ShowError("Unable to access google");
				return;
			}
            if (Result=="Wrong Password")
            {
                ShowError(Result);
                return;
            }

            try
            {
				var data = JSON.Parse(Result).AsObject;

				string name = data["name"];
				string key = data["id"];

				serializedObject.Update();
				mProp_Google_SpreadsheetKey.stringValue = key;
				mProp_Google_SpreadsheetName.stringValue = name;
				serializedObject.ApplyModifiedProperties();
				mGoogleSpreadsheets[name] = key;

                LanguageSourceData source = GetSourceData();
                if (source.mTerms.Count>0 || source.mLanguages.Count>0)
					Export_Google( eSpreadsheetUpdateMode.Replace );
				else
				if (EditorPrefs.GetBool("I2Loc OpenDataSourceAfterExport", true))
					OpenGoogleSpreadsheet( key );

			}
			catch(Exception e)
			{
				ShowError (e.Message);
			}
		}

		#endregion

		#region FindSpreadsheets		

		void Google_FindSpreadsheets()
		{
            ClearErrors();
            EditorApplication.update -= Google_FindSpreadsheets;
            string query =  mProp_Google_WebServiceURL.stringValue + "?action=GetSpreadsheetList&password="+ Uri.EscapeDataString(mProp_Google_Password.stringValue);
			mConnection_WWW = UnityWebRequest.Get(query);
            I2Utils.SendWebRequest(mConnection_WWW);
            mConnection_Callback = Google_OnFindSpreadsheets;
			EditorApplication.update += CheckForConnection;
			mConnection_Text = "Accessing google";
            //mConnection_TimeOut = Time.realtimeSinceStartup + 10;
		}

		void Google_OnFindSpreadsheets( string Result, string Error)
		{
			if (!string.IsNullOrEmpty(Error))
			{
				ShowError("Unable to access google");
				return;
			}

            if (Result=="Wrong Password")
            {
                ShowError(Result);
                return;
            }

            try
			{
				mGoogleSpreadsheets.Clear();
				var data = JSON.Parse(Result).AsObject;
				foreach (KeyValuePair<string, JSONNode> element in data)
					mGoogleSpreadsheets[element.Key] = element.Value;
			}
			catch(Exception e)
			{
				ShowError (e.Message);
			}

		}

		#endregion
	}
}