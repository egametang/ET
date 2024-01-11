//#define UGUI
//#define NGUI
//#define DFGUI

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I2.Loc
{
	[CustomEditor(typeof(Localize))]
	[CanEditMultipleObjects]
	public class LocalizeInspector : Editor
	{
		#region Variables

		Localize mLocalize;
		SerializedProperty 	mProp_mTerm, mProp_mTermSecondary,
							mProp_TranslatedObjects, mProp_LocalizeOnAwake, mProp_AlwaysForceLocalize, mProp_AllowLocalizedParameters, mProp_AllowParameters,
                            mProp_IgnoreRTL, mProp_MaxCharactersInRTL, mProp_CorrectAlignmentForRTL, mProp_IgnoreNumbersInRTL, mProp_TermSuffix, mProp_TermPrefix, mProp_SeparateWords,
                            mProp_CallbackEvent;


		bool mAllowEditKeyName;
		string mNewKeyName = "";

		string[] mTermsArray;


		public static string HelpURL_forum 			= "http://goo.gl/Uiyu8C";//http://www.inter-illusion.com/forum/i2-localization";
		public static string HelpURL_Documentation 	= "http://www.inter-illusion.com/assets/I2LocalizationManual/I2LocalizationManual.html";
		public static string HelpURL_Tutorials		= "http://inter-illusion.com/tools/i2-localization";
		public static string HelpURL_ReleaseNotes	= "http://inter-illusion.com/forum/i2-localization/26-release-notes";
		public static string HelpURL_AssetStore		= "https://www.assetstore.unity3d.com/#!/content/14884";

        public static LocalizeInspector mLocalizeInspector;
		#endregion
		
		#region Inspector
		
		void OnEnable()
		{
			mLocalize = (Localize)target;
            mLocalizeInspector = this;
            LocalizationEditor.mCurrentInspector = this;
            mProp_mTerm 			 		= serializedObject.FindProperty("mTerm");
			mProp_mTermSecondary	 		= serializedObject.FindProperty("mTermSecondary");
			mProp_TranslatedObjects  		= serializedObject.FindProperty("TranslatedObjects");
			mProp_IgnoreRTL			 		= serializedObject.FindProperty("IgnoreRTL");
            mProp_SeparateWords             = serializedObject.FindProperty("AddSpacesToJoinedLanguages");
            mProp_MaxCharactersInRTL 		= serializedObject.FindProperty ("MaxCharactersInRTL");
			mProp_IgnoreNumbersInRTL        = serializedObject.FindProperty("IgnoreNumbersInRTL");
			mProp_CorrectAlignmentForRTL 	= serializedObject.FindProperty ("CorrectAlignmentForRTL");
			mProp_LocalizeOnAwake    		= serializedObject.FindProperty("LocalizeOnAwake");
			mProp_AlwaysForceLocalize		= serializedObject.FindProperty("AlwaysForceLocalize");
			mProp_TermSuffix                = serializedObject.FindProperty("TermSuffix");
			mProp_TermPrefix                = serializedObject.FindProperty("TermPrefix");
            mProp_CallbackEvent             = serializedObject.FindProperty("LocalizeEvent");
            mProp_AllowLocalizedParameters  = serializedObject.FindProperty("AllowLocalizedParameters");
            mProp_AllowParameters           = serializedObject.FindProperty("AllowParameters");


            if (LocalizationManager.Sources.Count==0)
				LocalizationManager.UpdateSources();
			//LocalizationEditor.ParseTerms (true);

			//mGUI_ShowReferences = (mLocalize.TranslatedObjects!=null && mLocalize.TranslatedObjects.Length>0);
			//mGUI_ShowCallback = (mLocalize.LocalizeCallBack.Target!=null);
			//mGUI_ShowTems = true;
			LocalizationEditor.mKeysDesc_AllowEdit = false;
			GUI_SelectedTerm = 0;
			mNewKeyName = mLocalize.Term;

			if (mLocalize.Source != null)
				LocalizationEditor.mLanguageSource = mLocalize.Source.SourceData;
            else
            {
				if (LocalizationManager.Sources.Count==0)
					LocalizationManager.UpdateSources();
				LocalizationEditor.mLanguageSource = LocalizationManager.GetSourceContaining( mLocalize.Term );
			}

			//UpgradeManager.EnablePlugins();
			LocalizationEditor.ApplyInferredTerm (mLocalize);
            RemoveUnusedReferences(mLocalize);
		}

		void OnDisable()
		{
            mLocalizeInspector = null;
            if (LocalizationEditor.mCurrentInspector == this) LocalizationEditor.mCurrentInspector = null;


            if (mLocalize == null)
				return;

            //#if TextMeshPro
            //string previous = null;

            //if (!Application.isPlaying && !string.IsNullOrEmpty(mLocalize.TMP_previewLanguage))
            //{
            //	previous = LocalizationManager.CurrentLanguage;
            //	LocalizationManager.PreviewLanguage( mLocalize.TMP_previewLanguage );
            //}
            //#endif

            //mLocalize.OnLocalize();

            // Revert the preview language
            // except when in TMPro and not changing to another GameObject (TMPro has a bug where any change causes the inspector to Disable and Enable)
            if (!mLocalize.mLocalizeTargetName.Contains("LocalizeTarget_TextMeshPro") || Selection.activeGameObject==null || !Selection.gameObjects.Contains(mLocalize.gameObject))
            {
                LocalizationManager.LocalizeAll();
            }

            //#if TextMeshPro
            //if (!string.IsNullOrEmpty(previous))
            //{
            //	LocalizationManager.PreviewLanguage(previous);
            //	mLocalize.TMP_previewLanguage = null;
            //}
            //#endif

            RemoveUnusedReferences(mLocalize);
        }

        #endregion

        #region GUI

        public override void OnInspectorGUI()
		{
			Undo.RecordObject(target, "Localize");

			//GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
			//GUILayout.BeginVertical(GUIStyle_Background, GUILayout.Height(1));
			//GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Localize", GUIStyle_Header))
			{
				//Application.OpenURL(HelpURL_Documentation);
			}
			GUILayout.Space(-10);

			LocalizationManager.UpdateSources();

			if (LocalizationManager.Sources.Count==0)
			{
				EditorGUILayout.HelpBox("无法找到语言来源。\nUnable to find a Language Source.", MessageType.Warning);
			}
			else
			{
				GUILayout.Space(10);
					OnGUI_Target ();
				GUILayout.Space(10);
					OnGUI_Terms();

				//if (mGUI_ShowTems || mGUI_ShowReferences) GUILayout.Space(5);

					OnGUI_References();

				if (mLocalize.mGUI_ShowReferences || mLocalize.mGUI_ShowCallback) GUILayout.Space(10);

					//Localize loc = target as Localize;

				//--[ Localize Callback ]----------------------
                    EditorGUILayout.PropertyField(mProp_CallbackEvent, new GUIContent("On Localize Callback"));

					//string HeaderTitle = "On Localize Call:";
					//if (!mLocalize.mGUI_ShowCallback && loc.LocalizeCallBack.Target!=null && !string.IsNullOrEmpty(loc.LocalizeCallBack.MethodName))
					//	HeaderTitle = string.Concat(HeaderTitle, " <b>",loc.LocalizeCallBack.Target.name, ".</b><i>", loc.LocalizeCallBack.MethodName, "</i>");
					//mLocalize.mGUI_ShowCallback = GUITools.DrawHeader(HeaderTitle, mLocalize.mGUI_ShowCallback);
					//if (mLocalize.mGUI_ShowCallback)
					//{
					//	GUITools.BeginContents();
					//		DrawEventCallBack( loc.LocalizeCallBack, loc );
					//	GUITools.EndContents();
					//}
			}
			OnGUI_Source ();

			GUILayout.Space (10);

			GUITools.OnGUI_Footer("I2 Localization", LocalizationManager.GetVersion(), HelpURL_forum, HelpURL_Documentation, HelpURL_AssetStore);

			//GUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
            if (Event.current.type == EventType.Repaint)
            {
                LocalizationEditor.mTestAction = LocalizationEditor.eTest_ActionType.None;
                LocalizationEditor.mTestActionArg = null;
                LocalizationEditor.mTestActionArg2 = null;
            }
        }

		#endregion

		#region References

		void OnGUI_References()
		{
			if (mLocalize.mGUI_ShowReferences = GUITools.DrawHeader ("References", mLocalize.mGUI_ShowReferences))
			{
				GUITools.BeginContents();

                bool canTest = Event.current.type == EventType.Repaint;

                var testAddObj = canTest && LocalizationEditor.mTestAction == LocalizationEditor.eTest_ActionType.Button_Assets_Add ? (Object)LocalizationEditor.mTestActionArg : null;
                var testReplaceIndx = canTest && LocalizationEditor.mTestAction == LocalizationEditor.eTest_ActionType.Button_Assets_Replace ? (int)LocalizationEditor.mTestActionArg : -1;
                var testReplaceObj = canTest && LocalizationEditor.mTestAction == LocalizationEditor.eTest_ActionType.Button_Assets_Replace ? (Object)LocalizationEditor.mTestActionArg2 : null;
                var testDeleteIndx = canTest && LocalizationEditor.mTestAction == LocalizationEditor.eTest_ActionType.Button_Assets_Delete ? (int)LocalizationEditor.mTestActionArg : -1;

                bool changed = GUITools.DrawObjectsArray( mProp_TranslatedObjects, false, false, true, testAddObj, testReplaceObj, testReplaceIndx, testDeleteIndx);
                if (changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    foreach (var obj in serializedObject.targetObjects)
                        (obj as Localize).UpdateAssetDictionary();
                }

                GUITools.EndContents();
			}
		}

        void RemoveUnusedReferences(Localize cmp)
        {
            cmp.TranslatedObjects.RemoveAll(x => !IsUsingReference(LocalizationManager.GetTermData(cmp.Term), x) && !IsUsingReference(LocalizationManager.GetTermData(cmp.SecondaryTerm), x));
            if (cmp.TranslatedObjects.Count != cmp.mAssetDictionary.Count)
                cmp.UpdateAssetDictionary();
        }

        bool IsUsingReference(TermData termData, Object obj )
        {
            if (obj == null || termData==null) return false;

            string objName = obj.name;
            foreach (string translation in termData.Languages)
            {
                if (translation != null && translation.Contains(objName))
                    return true;
            }
            return false;
        }


        #endregion


        #region Terms

        int GUI_SelectedTerm;
		void OnGUI_Terms()
		{
			if ((mLocalize.mGUI_ShowTems=GUITools.DrawHeader ("Terms", mLocalize.mGUI_ShowTems)))
			{
				//--[ tabs: Main and Secondary Terms ]----------------
				int oldTab = GUI_SelectedTerm;
				if (mLocalize.mLocalizeTarget!=null && mLocalize.mLocalizeTarget.CanUseSecondaryTerm())
				{
					GUI_SelectedTerm = GUITools.DrawTabs (GUI_SelectedTerm, new[]{"Main", "Secondary"});
				}
				else
				{
					GUI_SelectedTerm = 0;
					GUITools.DrawTabs (GUI_SelectedTerm, new[]{"Main", ""});
				}

				GUITools.BeginContents();

                TermData termData = null;

					if (GUI_SelectedTerm==0) termData = OnGUI_PrimaryTerm( oldTab!=GUI_SelectedTerm );
										else termData = OnGUI_SecondaryTerm(oldTab!=GUI_SelectedTerm);

				GUITools.EndContents();

				//--[ Modifier ]-------------
				if (mLocalize.Term != "-" && termData!=null && termData.TermType==eTermType.Text)
				{
					EditorGUI.BeginChangeCheck();
					GUILayout.BeginHorizontal();
						GUILayout.Label("Prefix:");
						EditorGUILayout.PropertyField(mProp_TermPrefix, GUITools.EmptyContent);
						GUILayout.Label("Suffix:");
						EditorGUILayout.PropertyField(mProp_TermSuffix, GUITools.EmptyContent);
					GUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck())
					{
						EditorApplication.delayCall += () =>
						{
							if (targets != null)
							{
								foreach (var t in targets)
									if (t as Localize != null)
										(t as Localize).OnLocalize(true);
							}
						};
					}
                    EditorGUI.BeginChangeCheck();
                    int val = EditorGUILayout.Popup("Modifier", GUI_SelectedTerm == 0 ? (int)mLocalize.PrimaryTermModifier : (int)mLocalize.SecondaryTermModifier, Enum.GetNames(typeof(Localize.TermModification)));
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.FindProperty(GUI_SelectedTerm == 0 ? "PrimaryTermModifier" : "SecondaryTermModifier").enumValueIndex = val;
                        GUI.changed = false;
                    }
                }

                OnGUI_Options();
                //--[ OnAwake vs OnEnable ]-------------
                //GUILayout.BeginHorizontal();
                //mProp_LocalizeOnAwake.boolValue = GUILayout.Toggle(mProp_LocalizeOnAwake.boolValue, new GUIContent(" Pre-Localize on Awake", "Localizing on Awake could result in a lag when the level is loaded but faster later when objects are enabled. If false, it will Localize OnEnable, so will yield faster level load but could have a lag when screens are enabled"));
                //GUILayout.FlexibleSpace();
                //if (mLocalize.HasCallback())
                //{
                //    GUI.enabled = false;
                //    GUILayout.Toggle(true, new GUIContent(" Force Localize", "Enable this when the translations have parameters (e.g. Thew winner is {[WINNER}]) to prevent any optimization that could prevent updating the translation when the object is enabled"));
                //    GUI.enabled = true;
                //}
                //else
                //{
                //    mProp_AlwaysForceLocalize.boolValue = GUILayout.Toggle(mProp_AlwaysForceLocalize.boolValue, new GUIContent(" Force Localize", "Enable this when the translations have parameters (e.g. Thew winner is {[WINNER}]) to prevent any optimization that could prevent updating the translation when the object is enabled"));
                //}
                //GUILayout.EndHorizontal();

                //--[ Right To Left ]-------------
                if (!mLocalize.IgnoreRTL && mLocalize.Term!="-" &&  termData != null && termData.TermType == eTermType.Text)
				{ 
					GUILayout.BeginVertical("Box");
                        //GUILayout.BeginHorizontal();
                        //    mProp_IgnoreRTL.boolValue = GUILayout.Toggle(mProp_IgnoreRTL.boolValue, new GUIContent(" Ignore Right To Left", "Arabic and other RTL languages require processing them so they render correctly, this toogle allows ignoring that processing (in case you are doing it manually during a callback)"));
                        //    GUILayout.FlexibleSpace();
                        //    mProp_SeparateWords.boolValue = GUILayout.Toggle(mProp_SeparateWords.boolValue, new GUIContent(" Separate Words", " Some languages (e.g. Chinese, Japanese and Thai) don't add spaces to their words (all characters are placed toguether), enabling this checkbox, will add spaces to all characters to allow wrapping long texts into multiple lines."));
                        //GUILayout.EndHorizontal();
						{
							mProp_MaxCharactersInRTL.intValue = EditorGUILayout.IntField( new GUIContent("最大线长 Max line length", "如果语言是从右到左，那么长行将按此长度分割，并且RTL修复将应用于每行，则应将其设置为适合此文本宽度的最大字符数。0禁用每行修复\nIf the language is Right To Left, long lines will be split at this length and the RTL fix will be applied to each line, this should be set to the maximum number of characters that fit in this text width. 0 disables the per line fix"), mProp_MaxCharactersInRTL.intValue );
							GUILayout.BeginHorizontal();
							mProp_CorrectAlignmentForRTL.boolValue = GUILayout.Toggle(mProp_CorrectAlignmentForRTL.boolValue, new GUIContent(" 调整对齐 Adjust Alignment", "当从右到左语言时为右对齐，否则为左对齐\nRight-align when Right-To-Left Language, and Left-Align otherwise") );
							GUILayout.FlexibleSpace();
							mProp_IgnoreNumbersInRTL.boolValue = GUILayout.Toggle(mProp_IgnoreNumbersInRTL.boolValue, new GUIContent(" 忽略数字 Ignore Numbers", "将数字保留为拉丁字符，而不是转换它们\nPreserve numbers as latin characters instead of converting them"));
						    GUILayout.EndHorizontal();
						}

					GUILayout.EndVertical();
				}
				

				////GUILayout.EndHorizontal();
			}
		}

        void OnGUI_Options()
        {
            int mask = 0;
            if (mProp_LocalizeOnAwake.boolValue)          mask |= 1 << 0;
            if (mProp_AlwaysForceLocalize.boolValue)      mask |= 1 << 1;
            if (mProp_AllowParameters.boolValue)          mask |= 1 << 2;
            if (mProp_AllowLocalizedParameters.boolValue) mask |= 1 << 3;
            if (mProp_SeparateWords.boolValue)            mask |= 1 << 4;
            if (mProp_IgnoreRTL.boolValue)                mask |= 1 << 5;

            EditorGUI.BeginChangeCheck();
            mask = EditorGUILayout.MaskField(new GUIContent("Options"), mask, new []{
                "Localize On Awake",
                "Force Localize",
                "Allow Parameters",
                "Allow Localized Parameters",
                "Separate Words",
                "Ignore RTL"
            });
            if (EditorGUI.EndChangeCheck())
            {
                mProp_LocalizeOnAwake.boolValue          = (mask & (1 << 0))> 0;
                mProp_AlwaysForceLocalize.boolValue      = (mask & (1 << 1))> 0;
                mProp_AllowParameters.boolValue          = (mask & (1 << 2))> 0;
                mProp_AllowLocalizedParameters.boolValue = (mask & (1 << 3))> 0;
                mProp_SeparateWords.boolValue            = (mask & (1 << 4))> 0;
                mProp_IgnoreRTL.boolValue                = (mask & (1 << 5))> 0;
            }
        }

		TermData OnGUI_PrimaryTerm( bool OnOpen )
		{
			string Key = mLocalize.mTerm;
			if (string.IsNullOrEmpty(Key))
			{
				string SecondaryTerm;
				mLocalize.GetFinalTerms( out Key, out SecondaryTerm );
            }

			if (OnOpen) mNewKeyName = Key;
			if ( OnGUI_SelectKey( ref Key, string.IsNullOrEmpty(mLocalize.mTerm)))
				mProp_mTerm.stringValue = Key;
			return LocalizationEditor.OnGUI_Keys_Languages( Key, mLocalize );
		}

        TermData OnGUI_SecondaryTerm( bool OnOpen )
		{
			string Key = mLocalize.mTermSecondary;

			if (string.IsNullOrEmpty(Key))
			{
				string ss;
				mLocalize.GetFinalTerms( out ss, out Key );
			}
			
			if (OnOpen) mNewKeyName = Key;
			if ( OnGUI_SelectKey( ref Key, string.IsNullOrEmpty(mLocalize.mTermSecondary)))
				mProp_mTermSecondary.stringValue = Key;
			return LocalizationEditor.OnGUI_Keys_Languages( Key, mLocalize, false );
		}

		bool OnGUI_SelectKey( ref string Term, bool Inherited )  // Inherited==true means that the mTerm is empty and we are using the Label.text instead
		{
			GUILayout.Space (5);
			GUILayout.BeginHorizontal();

			GUI.changed = false;
			mAllowEditKeyName = GUILayout.Toggle(mAllowEditKeyName, "Term:", EditorStyles.foldout, GUILayout.ExpandWidth(false));
			if (GUI.changed && mAllowEditKeyName) {
				mNewKeyName = Term;
				mTermsArray = null;
			}

			bool bChanged = false;

			if (mTermsArray==null || Term!="-" && Array.IndexOf(mTermsArray, Term)<0)
				UpdateTermsList(Term);

			if (Inherited)
				GUI.contentColor = Color.Lerp (Color.gray, Color.yellow, 0.1f);

			int Index = Term=="-" || Term=="" ? mTermsArray.Length-1 : Array.IndexOf( mTermsArray, Term );

			GUI.changed = false;

			int newIndex = EditorGUILayout.Popup( Index, mTermsArray);

			GUI.contentColor = Color.white;
			if (/*newIndex != Index && newIndex>=0*/GUI.changed)
			{
				GUI.changed = false;
                if (mLocalize.Source != null && newIndex == mTermsArray.Length - 4)  //< show terms from all sources >
                {
                    mLocalize.Source = null;
                    mTermsArray = null;
                }
                else
                if (newIndex == mTermsArray.Length - 2)  //<inferred from text>
                    mNewKeyName = Term = string.Empty;
                else
                if (newIndex == mTermsArray.Length - 1)  //<none>
                    mNewKeyName = Term = "-";
                else
                    mNewKeyName = Term = mTermsArray[newIndex];


				if (GUI_SelectedTerm==0)
					mLocalize.SetTerm (mNewKeyName);
				else
					mLocalize.SetTerm (null, mNewKeyName);
				mAllowEditKeyName = false;
				bChanged = true;
			}

			LanguageSourceData source =  LocalizationManager.GetSourceContaining(Term);
			TermData termData = source.GetTermData(Term);
			if (termData!=null)
			{
				if (Inherited)
					bChanged = true; // if the term its inferred and a matching term its found, then use that
				eTermType NewType = (eTermType)EditorGUILayout.EnumPopup(termData.TermType, GUILayout.Width(90));
				if (termData.TermType != NewType)
					termData.TermType = NewType;
			}
			
			GUILayout.EndHorizontal();
			
			if (mAllowEditKeyName)
			{
				GUILayout.BeginHorizontal(GUILayout.Height (1));
				GUILayout.BeginHorizontal(EditorStyles.toolbar);
				if(mNewKeyName==null) mNewKeyName = string.Empty;

				GUI.changed = false;
				mNewKeyName = EditorGUILayout.TextField(mNewKeyName, new GUIStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true));
				if (GUI.changed)
				{
					mTermsArray = null;	// regenerate this array to apply filtering
					GUI.changed = false;
				}

				if (GUILayout.Button (string.Empty, string.IsNullOrEmpty(mNewKeyName) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton", GUILayout.ExpandWidth(false)))
				{
					mTermsArray = null;	// regenerate this array to apply filtering
					mNewKeyName = string.Empty;
				}

				GUILayout.EndHorizontal();

				string ValidatedName = mNewKeyName;
				LanguageSourceData.ValidateFullTerm( ref ValidatedName );

				bool CanUseNewName = source.GetTermData(ValidatedName)==null;
				GUI.enabled = !string.IsNullOrEmpty(mNewKeyName) && CanUseNewName;
				if (GUILayout.Button ("Create", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
				{
					mNewKeyName = ValidatedName;
					mTermsArray=null;	// this recreates that terms array

					LanguageSourceData Source = null;
					#if UNITY_EDITOR
					if (mLocalize.Source!=null)
						Source = mLocalize.Source.SourceData;
					#endif

					if (Source==null)
						Source = LocalizationManager.Sources[0];
                    Term = mNewKeyName;
                    var data = Source.AddTerm( mNewKeyName, eTermType.Text, false );
					if (data.Languages.Length > 0)
						data.Languages[0] = mLocalize.GetMainTargetsText();
					Source.Editor_SetDirty();
					AssetDatabase.SaveAssets();
					mAllowEditKeyName = false;
					bChanged = true;
					GUIUtility.keyboardControl = 0;
				}
				GUI.enabled = termData!=null && !string.IsNullOrEmpty(mNewKeyName) && CanUseNewName;
				if (GUILayout.Button (new GUIContent("Rename","重命名源中的术语并更新当前场景中使用它的每个对象\nRenames the term in the source and updates every object using it in the current scene"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
				{
					mNewKeyName = ValidatedName;
					Term = mNewKeyName;
					mTermsArray=null;     // this recreates that terms array
					mAllowEditKeyName = false;
					bChanged = true;
					LocalizationEditor.TermReplacements = new Dictionary<string, string>(StringComparer.Ordinal);
					LocalizationEditor.TermReplacements[ termData.Term ] = mNewKeyName;
					termData.Term = mNewKeyName;
					source.UpdateDictionary(true);
					LocalizationEditor.ReplaceTermsInCurrentScene();
					GUIUtility.keyboardControl = 0;
					EditorApplication.update += LocalizationEditor.DoParseTermsInCurrentScene;
				}
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				bChanged |= OnGUI_SelectKey_PreviewTerms ( ref Term);
			}
			
			GUILayout.Space (5);
			return bChanged;
		}

		void UpdateTermsList( string currentTerm )
		{
			List<string> Terms = mLocalize.Source==null ? LocalizationManager.GetTermsList() : mLocalize.Source.SourceData.GetTermsList();
			
			// If there is a filter, remove all terms not matching that filter
			if (mAllowEditKeyName && !string.IsNullOrEmpty(mNewKeyName)) 
			{
				string Filter = mNewKeyName.ToUpper();
				for (int i=Terms.Count-1; i>=0; --i)
					if (!Terms[i].ToUpper().Contains(Filter) && Terms[i]!=currentTerm)
						Terms.RemoveAt(i);
				
			}

			if (!string.IsNullOrEmpty(currentTerm) && currentTerm!="-" && !Terms.Contains(currentTerm))
				Terms.Add (currentTerm);

			Terms.Sort(StringComparer.OrdinalIgnoreCase);
			Terms.Add("");
            if (mLocalize.Source != null)
            {
                Terms.Add("< Show Terms from all sources >");
                Terms.Add("");
            }
            Terms.Add("<inferred from text>");
			Terms.Add("<none>");

			mTermsArray = Terms.ToArray();
		}

		bool OnGUI_SelectKey_PreviewTerms ( ref string Term)
		{
			if (mTermsArray==null)
				UpdateTermsList(Term);

			int nTerms = mTermsArray.Length;
			if (nTerms<=0)
				return false;

			if (nTerms==1 && mTermsArray[0]==Term)
				return false;

			bool bChanged = false;
			GUI.backgroundColor = Color.gray;
			GUILayout.BeginVertical (GUIStyle_OldTextArea);
			for (int i = 0, imax = Mathf.Min (nTerms, 3); i < imax; ++i) 
			{
				ParsedTerm parsedTerm;
				int nUses = -1;
				if (LocalizationEditor.mParsedTerms.TryGetValue (mTermsArray [i], out parsedTerm))
					nUses = parsedTerm.Usage;

				string FoundText = mTermsArray [i];
				if (nUses > 0)
					FoundText = string.Concat ("(", nUses, ") ", FoundText);

				if (GUILayout.Button (FoundText, EditorStyles.miniLabel, GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - 70))) 
				{
					if (mTermsArray[i] == "<inferred from text>")
						mNewKeyName = Term = string.Empty;
					else
					if (mTermsArray[i] == "<none>")
						mNewKeyName = Term = "-";
                    else
                    if (mTermsArray[i] != "< Show Terms from all sources >")
						mNewKeyName = Term = mTermsArray[i];

    				//mNewKeyName = Term = (mTermsArray [i]=="<inferred from text>" ? string.Empty : mTermsArray [i]);
					GUIUtility.keyboardControl = 0;
					mAllowEditKeyName = false;
					bChanged = true;
				}
			}
			if (nTerms > 3)
				GUILayout.Label ("...");
			GUILayout.EndVertical ();
			GUI.backgroundColor = Color.white;

			return bChanged;
		}

		#endregion

		#region Target

		void OnGUI_Target()
		{
			List<string> TargetTypes = new List<string>();
			int CurrentTarget = -1;

			mLocalize.FindTarget();

            foreach (var desc in LocalizationManager.mLocalizeTargets)
            {
                if (desc.CanLocalize(mLocalize))
                {
                    TargetTypes.Add(desc.Name);

                    if (mLocalize.mLocalizeTarget!=null && desc.GetTargetType() == mLocalize.mLocalizeTarget.GetType())
                        CurrentTarget = TargetTypes.Count - 1;
                }
            }

			if (CurrentTarget==-1)
			{
				CurrentTarget = TargetTypes.Count;
				TargetTypes.Add("None");
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label ("Target:", GUILayout.Width (60));
            EditorGUI.BeginChangeCheck();
			    int index = EditorGUILayout.Popup(CurrentTarget, TargetTypes.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                foreach (var obj in serializedObject.targetObjects)
                {
                    var cmp = obj as Localize;
                    if (cmp == null)
                        continue;

                    if (cmp.mLocalizeTarget != null)
                        DestroyImmediate(cmp.mLocalizeTarget);
                    cmp.mLocalizeTarget = null;

                    foreach (var desc in LocalizationManager.mLocalizeTargets)
                    {
                        if (desc.Name == TargetTypes[index])
                        {
                            cmp.mLocalizeTarget = desc.CreateTarget(cmp);
                            cmp.mLocalizeTargetName = desc.GetTargetType().ToString();
                            break;
                        }
                    }
                }
				serializedObject.Update();
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region Source

		void OnGUI_Source()
		{
			GUILayout.BeginHorizontal();

				ILanguageSource currentSource  = mLocalize.Source;
                if (currentSource==null)
				{
                    LanguageSourceData source = LocalizationManager.GetSourceContaining(mLocalize.Term);
                    currentSource = source==null ? null : source.owner;
				}

				if (GUILayout.Button("Open Source", EditorStyles.toolbarButton, GUILayout.Width (100)))
				{
					Selection.activeObject = currentSource as Object;

					string sTerm, sSecondary;
					mLocalize.GetFinalTerms( out sTerm, out sSecondary );
					if (GUI_SelectedTerm==1) sTerm = sSecondary;
					LocalizationEditor.mKeyToExplore = sTerm;
                }

				GUILayout.Space (2);

				GUILayout.BeginHorizontal(EditorStyles.toolbar);
					EditorGUI.BeginChangeCheck ();
					if (mLocalize.Source == null)
					{
						GUI.contentColor = Color.Lerp (Color.gray, Color.yellow, 0.1f);
					}
                    Object obj = EditorGUILayout.ObjectField(currentSource as Object, typeof(Object), true);
					GUI.contentColor = Color.white;
					if (EditorGUI.EndChangeCheck())
					{
                        ILanguageSource NewSource = obj as ILanguageSource;
                        if (NewSource == null && obj as GameObject != null)
                        {
                            NewSource = (obj as GameObject).GetComponent<LanguageSource>();
                        }

                        mLocalize.Source = NewSource;
                        string sTerm, sSecondary;
                        mLocalize.GetFinalTerms(out sTerm, out sSecondary);
                        if (GUI_SelectedTerm == 1) sTerm = sSecondary;
                        UpdateTermsList(sTerm);
                    }

                    if (GUILayout.Button(new GUIContent("Detect", "找到包含所选术语的LanguageSource，术语列表现在将只显示该源中的术语。\nFinds the LanguageSource containing the selected term, the term list will now only show terms inside that source."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                    {
                        string sTerm, sSecondary;
                        mLocalize.GetFinalTerms(out sTerm, out sSecondary);
                        if (GUI_SelectedTerm == 1) sTerm = sSecondary;

                        var data = LocalizationManager.GetSourceContaining(sTerm, false);
                        mLocalize.Source = data==null ? null : data.owner;
                        mTermsArray = null;
                    }
            GUILayout.EndHorizontal();

			GUILayout.EndHorizontal();
		}

		#endregion

		
		#region Event CallBack
		
		//public void DrawEventCallBack( EventCallback CallBack, Localize loc )
		//{
			//if (CallBack==null)
			//	return;

			//GUI.changed = false;

			//GUILayout.BeginHorizontal();
			//GUILayout.Label("Target:", GUILayout.ExpandWidth(false));
			//CallBack.Target = EditorGUILayout.ObjectField( CallBack.Target, typeof(MonoBehaviour), true) as MonoBehaviour;
			//GUILayout.EndHorizontal();
			
			//if (CallBack.Target!=null)
			//{
			//	GameObject GO = CallBack.Target.gameObject;
			//	List<MethodInfo> Infos = new List<MethodInfo>();

			//	var targets = GO.GetComponents(typeof(MonoBehaviour));
			//	foreach (var behavior in targets)
			//		Infos.AddRange( behavior.GetType().GetMethods() );

			//	List<string> Methods = new List<string>();
				
			//	for (int i = 0, imax=Infos.Count; i<imax; ++i)
			//	{
			//		MethodInfo mi = Infos[i];
					
			//		if (IsValidMethod(mi))
			//			Methods.Add (mi.Name);
			//	}
				
			//	int Index = Methods.IndexOf(CallBack.MethodName);
				
			//	int NewIndex = EditorGUILayout.Popup(Index, Methods.ToArray(), GUILayout.ExpandWidth(true));
			//	if (NewIndex!=Index)
			//		CallBack.MethodName = Methods[ NewIndex ];
			//}
			//if (GUI.changed)
			//{
			//	GUI.changed = false;
			//	EditorUtility.SetDirty(loc);
			//	//UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty() EditorApplication.MakeSceneDirty();
			//}
		//}
		
		static bool IsValidMethod( MethodInfo mi )
		{
			if (mi.DeclaringType == typeof(MonoBehaviour) || mi.ReturnType != typeof(void))
				return false;
			
			ParameterInfo[] Params = mi.GetParameters ();
			if (Params.Length == 0)	return true;
			if (Params.Length > 1)  return false;
			
			if (Params [0].ParameterType.IsSubclassOf (typeof(Object)))	return true;
			if (Params [0].ParameterType == typeof(Object))	return true;
			return false;
		}
		
		
		#endregion

		#region Styles
		
		public static GUIStyle GUIStyle_Header {
			get{
				if (mGUIStyle_Header==null)
				{
					mGUIStyle_Header = new GUIStyle("HeaderLabel");
					mGUIStyle_Header.fontSize = 25;
					mGUIStyle_Header.normal.textColor = Color.Lerp(Color.white, Color.gray, 0.5f);
					mGUIStyle_Header.fontStyle = FontStyle.BoldAndItalic;
					mGUIStyle_Header.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_Header;
			}
		}
		static GUIStyle mGUIStyle_Header;
		
		public static GUIStyle GUIStyle_SubHeader {
			get{
				if (mGUIStyle_SubHeader==null)
				{
					mGUIStyle_SubHeader = new GUIStyle("HeaderLabel");
					mGUIStyle_SubHeader.fontSize = 13;
					mGUIStyle_SubHeader.fontStyle = FontStyle.Normal;
					mGUIStyle_SubHeader.margin.top = -50;
					mGUIStyle_SubHeader.alignment = TextAnchor.UpperCenter;
				}
				return mGUIStyle_SubHeader;
			}
		}
		static GUIStyle mGUIStyle_SubHeader;
		
		public static GUIStyle GUIStyle_Background {
			get{
				if (mGUIStyle_Background==null)
				{
                    mGUIStyle_Background = new GUIStyle(EditorStyles.textArea);
                    mGUIStyle_Background.fixedHeight = 0;
                    mGUIStyle_Background.overflow.left = 50;
					mGUIStyle_Background.overflow.right = 50;
					mGUIStyle_Background.overflow.top = -5;
					mGUIStyle_Background.overflow.bottom = 0;
				}
				return mGUIStyle_Background;
			}
		}
		static GUIStyle mGUIStyle_Background;

        public static GUIStyle GUIStyle_OldTextArea
        {
            get
            {
                if (mGUIStyle_OldTextArea == null)
                {
                    mGUIStyle_OldTextArea = new GUIStyle(EditorStyles.textArea);
                    mGUIStyle_OldTextArea.fixedHeight = 0;
                }
                return mGUIStyle_OldTextArea;
            }
        }
        static GUIStyle mGUIStyle_OldTextArea;

        #endregion
    }
}