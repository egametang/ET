using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{

		#region Variables

		public enum eViewMode { ImportExport, Keys, Languages, Tools, References }
		public static eViewMode mCurrentViewMode = eViewMode.Keys;
		
		public enum eSpreadsheetMode { Local, Google }
		public eSpreadsheetMode mSpreadsheetMode = eSpreadsheetMode.Google;


		public static string mLocalizationMsg = "";
		public static MessageType mLocalizationMessageType = MessageType.None;

        // These variables are for executing action from Unity Tests
        public enum eTest_ActionType { None, Button_AddLanguageFromPopup, Button_AddLanguageManual,
                                        Button_AddTerm_InTermsList, Button_AddSelectedTerms,
                                        Button_RemoveSelectedTerms, Button_DeleteTerm,
                                        Button_SelectTerms_All, Button_SelectTerms_None, Button_SelectTerms_Used, Button_SelectTerms_Missing,
                                        Button_Term_Translate, Button_Term_TranslateAll, Button_Languages_TranslateAll,
                                        Button_Assets_Add, Button_Assets_Replace, Button_Assets_Delete,
                                        Button_GoogleSpreadsheet_RefreshList, Button_GoogleSpreadsheet_Export, Button_GoogleSpreadsheet_Import
        }
        public static eTest_ActionType mTestAction = eTest_ActionType.None;
        public static object mTestActionArg, mTestActionArg2;

        #endregion

        #region Editor

        /*[MenuItem("Window/Localization", false)]
		public static void OpenLocalizationEditor()
		{
			EditorWindow.GetWindow<LocalizationEditor>(false, "Localization", true);
		}*/

        #endregion

        #region GUI

        void InitializeStyles()
		{
			Style_ToolBar_Big = new GUIStyle(EditorStyles.toolbar);
			Style_ToolBar_Big.fixedHeight = Style_ToolBar_Big.fixedHeight*1.5f;

			Style_ToolBarButton_Big = new GUIStyle(EditorStyles.toolbarButton);
			Style_ToolBarButton_Big.fixedHeight = Style_ToolBarButton_Big.fixedHeight*1.5f;
		}


		void OnGUI_Main()
		{
			OnGUI_Warning_SourceInScene();
			OnGUI_Warning_SourceInsidePluginsFolder();
            OnGUI_Warning_SourceNotUpToDate();

            var prevViewMode = mCurrentViewMode;

			GUILayout.BeginHorizontal();
				//OnGUI_ToggleEnumBig( "Spreadsheets", ref mCurrentViewMode, eViewMode.ImportExport, GUI.skin.GetStyle("CN EntryWarn").normal.background, "External Spreadsheet File or Service" );
				OnGUI_ToggleEnumBig( "表格 Spreadsheets", ref mCurrentViewMode, eViewMode.ImportExport, null, "外部电子表格文件或服务\nExternal Spreadsheet File or Service" );
				OnGUI_ToggleEnumBig( "术语 Terms", ref mCurrentViewMode, eViewMode.Keys, null, null );
				OnGUI_ToggleEnumBig( "语言 Languages", ref mCurrentViewMode, eViewMode.Languages, null, null );
				OnGUI_ToggleEnumBig( "工具 Tools", ref mCurrentViewMode, eViewMode.Tools, null, null );
				OnGUI_ToggleEnumBig( "资源 Assets", ref mCurrentViewMode, eViewMode.References, null, null );
			GUILayout.EndHorizontal();
			//GUILayout.Space(10);

			switch (mCurrentViewMode)
			{
				case eViewMode.ImportExport 			: OnGUI_ImportExport(); break;
				case eViewMode.Keys 					: OnGUI_KeysList(); break;
				case eViewMode.Languages 				: OnGUI_Languages(); break;
				case eViewMode.Tools 					: OnGUI_Tools(prevViewMode != mCurrentViewMode); break;
				case eViewMode.References 				: OnGUI_References(); break;
			}
		}

		void OnGUI_ImportExport()
		{
			eSpreadsheetMode OldMode = mSpreadsheetMode;
			mSpreadsheetMode = (eSpreadsheetMode)GUITools.DrawShadowedTabs ((int)mSpreadsheetMode, new[]{"Local", "Google"});
			if (mSpreadsheetMode != OldMode)
				ClearErrors();

			GUITools.BeginContents();
			switch (mSpreadsheetMode)
			{
				case eSpreadsheetMode.Local 	: OnGUI_Spreadsheet_Local();  break;
				case eSpreadsheetMode.Google	: OnGUI_Spreadsheet_Google(); break;
			}
			GUITools.EndContents(false);
		}

		void OnGUI_References()
		{
			EditorGUILayout.HelpBox("这些是由Terms引用的资产，而不是在Resources文件夹中 \nThese are the assets that are referenced by the Terms and not in the Resources folder", MessageType.Info);

            bool canTest = Event.current.type == EventType.Repaint;

            var testAddObj = canTest && mTestAction == eTest_ActionType.Button_Assets_Add ? (Object)mTestActionArg : null;
            var testReplaceIndx = canTest && mTestAction == eTest_ActionType.Button_Assets_Replace ? (int)mTestActionArg : -1;
            var testReplaceObj = canTest && mTestAction == eTest_ActionType.Button_Assets_Replace ? (Object)mTestActionArg2 : null;
            var testDeleteIndx = canTest && mTestAction == eTest_ActionType.Button_Assets_Delete ? (int)mTestActionArg : -1;

            bool changed = GUITools.DrawObjectsArray( mProp_Assets, false, false, false, testAddObj, testReplaceObj, testReplaceIndx, testDeleteIndx);
            if (changed)
            {
			    serializedObject.ApplyModifiedProperties();
                foreach (var obj in serializedObject.targetObjects)
                    (obj as LanguageSource).mSource.UpdateAssetDictionary();
            }
        }	

		#endregion

		#region Misc

		void OnGUI_ToggleEnumBig<Enum>( string text, ref Enum currentMode, Enum newMode, Texture texture, string tooltip) 	{ OnGUI_ToggleEnum( text, ref currentMode, newMode, texture, tooltip, Style_ToolBarButton_Big); }
		void OnGUI_ToggleEnumSmall<Enum>( string text, ref Enum currentMode, Enum newMode, Texture texture, string tooltip) { OnGUI_ToggleEnum( text, ref currentMode, newMode, texture, tooltip, EditorStyles.toolbarButton); }
		void OnGUI_ToggleEnum<Enum>( string text, ref Enum currentMode, Enum newMode, Texture texture, string tooltip, GUIStyle style)
		{
			GUI.changed = false;
			if (GUILayout.Toggle( currentMode.Equals(newMode), new GUIContent(text, texture, tooltip), style, GUILayout.ExpandWidth(true)))
			{ 
				currentMode = newMode;
				if (GUI.changed)
					ClearErrors();
			}			
		}
		
		int OnGUI_FlagToogle( string Text, string tooltip, int flags, int bit )
		{
			bool State = (flags & bit)>0;
			bool NewState = GUILayout.Toggle(State, new GUIContent(Text, tooltip), "toolbarbutton");
			if (State!=NewState)
			{
				if (!NewState && flags==bit)
					return flags;
				
				flags = NewState ? flags | bit  : flags & ~bit;
			}
			
			return flags;
		}
		
		void OnGUI_SelectableToogleListItem( string Element, ref List<string> Selections, string Style )
		{
			bool WasEnabled = Selections.Contains(Element);
			bool IsEnabled = GUILayout.Toggle( WasEnabled, "", Style, GUILayout.ExpandWidth(false) );
			
			if (IsEnabled && !WasEnabled)
				Selections.Add(Element);
			else
				if (!IsEnabled && WasEnabled)
					Selections.Remove(Element);
		}

		void OnGUI_SelectableToogleListItem( Rect rect, string Element, ref List<string> Selections, string Style )
		{
			bool WasEnabled = Selections.Contains(Element);
			bool IsEnabled = GUI.Toggle( rect, WasEnabled, "", Style );
			
			if (IsEnabled && !WasEnabled)
				Selections.Add(Element);
			else
				if (!IsEnabled && WasEnabled)
					Selections.Remove(Element);
		}

        static bool InTestAction( eTest_ActionType testType )
        {
            return mTestAction == testType && Event.current.type == EventType.Repaint;
        }
        static bool TestButton(eTest_ActionType action, string text, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, style, options) || mTestAction == action && Event.current.type == EventType.Repaint;
        }

        static bool TestButtonArg(eTest_ActionType action, object arg, string text, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, style, options) || mTestAction == action && (mTestActionArg==null || mTestActionArg.Equals(arg)) && Event.current.type == EventType.Repaint;
        }


        static bool TestButton(eTest_ActionType action, GUIContent text, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, style, options) || mTestAction == action && Event.current.type == EventType.Repaint;
        }

        static bool TestButtonArg(eTest_ActionType action, object arg, GUIContent text, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, style, options) || mTestAction == action && (mTestActionArg == null || mTestActionArg.Equals(arg)) && Event.current.type == EventType.Repaint;
        }

        #endregion

        #region Error Management

        static void OnGUI_ShowMsg()
		{
			if (!string.IsNullOrEmpty(mLocalizationMsg))
			{
                GUILayout.BeginHorizontal();
				    EditorGUILayout.HelpBox(mLocalizationMsg, mLocalizationMessageType);

                    GUILayout.Space(-5);
                    GUILayout.BeginVertical(GUILayout.Width(15), GUILayout.ExpandHeight(false));
                        GUILayout.Space(15);
                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                            ClearErrors();
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(8);
			}
		}
		
		static void ShowError  ( string Error, bool ShowInConsole = true )  { ShowMessage ( Error, MessageType.Error, ShowInConsole ); }
		static void ShowInfo   ( string Msg,   bool ShowInConsole = false ) { ShowMessage ( Msg, MessageType.Info, ShowInConsole ); }
		static void ShowWarning( string Msg,   bool ShowInConsole = true)   { ShowMessage ( Msg, MessageType.Warning, ShowInConsole ); }
		
		static void ShowMessage( string Msg, MessageType msgType, bool ShowInConsole )
		{
			if (string.IsNullOrEmpty(Msg))
			    Msg = string.Empty;

			mLocalizationMsg = Msg;
			mLocalizationMessageType = msgType;
			if (ShowInConsole)
			{
				switch (msgType)
				{
					case MessageType.Error 	 : Debug.LogError(Msg); break;
					case MessageType.Warning : Debug.LogWarning(Msg); break;
					default 	 			 : Debug.Log(Msg); break;
				}
			}
		}
		
		
		public static void ClearErrors()
		{
			GUI.FocusControl(null);

			mLocalizationMsg = string.Empty;
		}
		
		#endregion

		#region Unity Version branching

		public static string Editor_GetCurrentScene()
		{
			#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				return EditorApplication.currentScene;
			#else
				return SceneManager.GetActiveScene().path;
			#endif
		}

        public static void Editor_MarkSceneDirty()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            #else
                EditorApplication.MarkSceneDirty();
            #endif
        }

        public static void Editor_SaveScene(bool force=false)
		{
			if (force)
				Editor_MarkSceneDirty();

			#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			EditorApplication.SaveScene ();
			#else
			EditorSceneManager.SaveOpenScenes();
			#endif
		}

		public static void Editor_OpenScene( string sceneName )
		{
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            if (string.IsNullOrEmpty(sceneName))
                EditorApplication.NewEmptyScene();
            else
                EditorApplication.OpenScene(sceneName);
#else
			if (string.IsNullOrEmpty(sceneName))
				EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
			else
				EditorSceneManager.OpenScene(sceneName);
			#endif
		}

		#endregion
	}
}