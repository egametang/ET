using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables
		
		Vector2 mScrollPos_Keys = Vector2.zero;
		
		public static string mKeyToExplore;  								// Key that should show all the language details
		static string KeyList_Filter = "";
		float mRowSize=-1;
		float ScrollHeight;
		float mTermList_MaxWidth = -1;

		public static List<string> mSelectedKeys = new List<string>(); 	// Selected Keys in the list of mParsedKeys
		public static List<string> mSelectedCategories = new List<string>();


		public enum eFlagsViewKeys
		{
			Used = 1<<1,
			Missing = 1<<2, 
			NotUsed = 1<<3
		}
		public static int mFlagsViewKeys = (int)eFlagsViewKeys.Used | (int)eFlagsViewKeys.NotUsed;

		public static string mTermsList_NewTerm;
		Rect mKeyListFilterRect;

		#endregion
		
		#region GUI Key List

		float ExpandedViewHeight;
		float TermsListHeight;

		void OnGUI_KeysList(bool AllowExpandKey = true, float Height = 300.0f, bool ShowTools=true)
		{
			///if (mTermList_MaxWidth<=0)
				CalculateTermsListMaxWidth();

			//--[ List Filters ]--------------------------------------

			// The ID of this control is registered here to avoid losing focus when the terms list grows in the scrollbox
			// This control is drawn later on
			int KeyListFilterID = GUIUtility.GetControlID( FocusType.Keyboard );

			OnGUI_ShowMsg();

			GUILayout.BeginHorizontal();
				GUIStyle bstyle = new GUIStyle ("toolbarbutton");
				bstyle.fontSize = 15;
				if (GUILayout.Button (new GUIContent("\u21ea", "解析场景并更新缺少和未使用术语的术语列表\nParse Scene and update terms list with missing and unused terms"), bstyle, GUILayout.Width(40)))
                    EditorApplication.update += DoParseTermsInCurrentSceneAndScripts;
                if (GUILayout.Button(new GUIContent("\u21bb", "刷新所有本地化对象的翻译\nRefresh the translation of all Localize objects"), bstyle, GUILayout.Width(40)))
                    CallLocalizeAll();

            GUILayout.Space (1);

			var oldFlags = mFlagsViewKeys;
			mFlagsViewKeys = OnGUI_FlagToogle("使用 Used","显示已解析场景中引用的所有术语\nShows All Terms referenced in the parsed scenes", 				mFlagsViewKeys, (int)eFlagsViewKeys.Used);
			mFlagsViewKeys = OnGUI_FlagToogle("未使用 Not Used", "显示源中未使用的所有术语\nShows all Terms from the Source that are not been used", 	mFlagsViewKeys, (int)eFlagsViewKeys.NotUsed);
			mFlagsViewKeys = OnGUI_FlagToogle("未定义 Missing","显示所有已使用但未在源代码中定义的术语。\nShows all Terms Used but not defined in the Source", 			mFlagsViewKeys, (int)eFlagsViewKeys.Missing);
			if (oldFlags!=mFlagsViewKeys)
                ScheduleUpdateTermsToShowInList();

            OnGUI_SelectedCategories();

			GUILayout.EndHorizontal();

            /*//if (Event.current.type == EventType.Repaint)
                TermsListHeight = Screen.height - 400;
            Debug.Log(Event.current.type + " " + TermsListHeight + " " + Screen.height + " " + GUILayoutUtility.GetLastRect().yMax);
                
            //TermsListHeight = Mathf.Min(Screen.height*0.5f, TermsListHeight);
            mScrollPos_Keys = GUILayout.BeginScrollView(mScrollPos_Keys, false, false, "horizontalScrollbar", "verticalScrollbar", LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(TermsListHeight));
            for (int i = 0; i < 1000; ++i)
                GUILayout.Label("ahhh" + i);
            GUILayout.EndScrollView();

            return;*/
            TermsListHeight = Mathf.Min(Screen.height*0.5f, TermsListHeight);

            //--[ Keys List ]-----------------------------------------
            GUI.backgroundColor = Color.Lerp(GUITools.LightGray, Color.white, 0.5f);
            mScrollPos_Keys = GUILayout.BeginScrollView( mScrollPos_Keys, false, false, "horizontalScrollbar", "verticalScrollbar", LocalizeInspector.GUIStyle_OldTextArea ,GUILayout.Height(TermsListHeight)/*GUILayout.MinHeight(Height), GUILayout.MaxHeight(Screen.height), GUILayout.ExpandHeight(true)*/);
            GUI.backgroundColor = Color.white;

            bool bAnyValidUsage = false;

			mRowSize = EditorStyles.toolbar.fixedHeight;
			if (Event.current!=null && Event.current.type == EventType.Layout)
				ScrollHeight = mScrollPos_Keys.y;

			float YPosMin = -ScrollHeight;
			int nSkip = 0;
			int nDraw = 0;

			if (mShowableTerms.Count == 0 && Event.current.type == EventType.Layout)
				UpdateTermsToShownInList ();

			float SkipSize = 0;
			foreach (var parsedTerm in mShowableTerms)
			{
				string sKey = parsedTerm.Term;
				string sCategory = parsedTerm.Category;
				string FullKey = parsedTerm.FullTerm;

				int nUses = parsedTerm.Usage;
				bAnyValidUsage = bAnyValidUsage | (nUses>=0);

				ShowTerm_termData = parsedTerm.termData;

				// Skip lines outside the view -----------------------
				YPosMin += mRowSize;
				SkipSize += mRowSize;
				float YPosMax = YPosMin + mRowSize;
				bool isExpanded = AllowExpandKey && mKeyToExplore==FullKey;
				if (!isExpanded && (YPosMax<-2*mRowSize || YPosMin>/*Screen.height*/TermsListHeight+mRowSize))
				{
					if (YPosMin>TermsListHeight+mRowSize)
						break;

					nSkip++;
					continue;
				}
				nDraw++;

				//------------------------------------------------------

				OnGUI_KeyHeader (sKey, sCategory, FullKey, nUses, YPosMin-mRowSize+mScrollPos_Keys.y);

				//--[ Key Details ]-------------------------------
				
				if (isExpanded)
				{
					GUILayout.Space(SkipSize);
					SkipSize = 0;
					OnGUI_KeyList_ShowKeyDetails();
					Rect rect = GUILayoutUtility.GetLastRect();
					if (rect.height>5)
						ExpandedViewHeight = rect.height;
					YPosMin += ExpandedViewHeight;
				}
			}
			SkipSize += (mShowableTerms.Count - nDraw-nSkip) * mRowSize;
			GUILayout.Space(SkipSize+2);
			if (mSelectedCategories.Count < mParsedCategories.Count) 
			{
                SkipSize += 25;
                if (GUILayout.Button ("...", EditorStyles.label)) 
				{
					mSelectedCategories.Clear ();
					mSelectedCategories.AddRange (mParsedCategories);
                }
			}
			OnGUI_KeysList_AddKey();

			GUILayout.Label("", GUILayout.Width(mTermList_MaxWidth+10+30), GUILayout.Height(1));

			GUILayout.EndScrollView();

            TermsListHeight = YPosMin + mRowSize + 25;//SkipSize+25;

            //Rect ListRect = GUILayoutUtility.GetLastRect();
            //if (ListRect.height>5)
            //	TermsListHeight = ListRect.height;
            //Debug.Log(nDraw + " " + nSkip + " " + Screen.height + " " + TermsListHeight);

            OnGUI_Keys_ListSelection( KeyListFilterID );    // Selection Buttons
			
//			if (!bAnyValidUsage)
//				EditorGUILayout.HelpBox("Use (Tools\\Parse Terms) to find how many times each of the Terms are used", UnityEditor.MessageType.Info);

			if (ShowTools)
			{
				GUILayout.BeginHorizontal();
				GUI.enabled = mSelectedKeys.Count>0 || !string.IsNullOrEmpty(mKeyToExplore);
					if (TestButton (eTest_ActionType.Button_AddSelectedTerms, new GUIContent("添加 Add Terms", "向源中添加术语 Add terms to Source"), "Button", GUITools.DontExpandWidth)) 		 AddTermsToSource();
					if (TestButton (eTest_ActionType.Button_RemoveSelectedTerms, new GUIContent("移除 Remove Terms", "从源中删除术语 Remove Terms from Source"), "Button", GUITools.DontExpandWidth)) 	 RemoveTermsFromSource();

					GUILayout.FlexibleSpace ();

					if (GUILayout.Button ("改变类别 Change Category")) OpenTool_ChangeCategoryOfSelectedTerms();
				GUI.enabled = true;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace ();
					bool newBool = GUILayout.Toggle(mLanguageSource.CaseInsensitiveTerms, "不区分大小写的术语 Case Insensitive Terms");
					if (newBool != mLanguageSource.CaseInsensitiveTerms)
					{
						mProp_CaseInsensitiveTerms.boolValue = newBool;
					}
					GUILayout.FlexibleSpace ();
				GUILayout.EndHorizontal();
			}
			//Debug.Log ("Draw: " + nDraw + " Skip: " + nSkip);
		}

        static void ScheduleUpdateTermsToShowInList()
        {
            if (!mUpdateShowTermIsScheduled)
            {
                EditorApplication.update += UpdateTermsToShownInList;
                mUpdateShowTermIsScheduled = true;
            }
        }
        static bool mUpdateShowTermIsScheduled;
		static void UpdateTermsToShownInList()
		{
            EditorApplication.update -= UpdateTermsToShownInList;
            mUpdateShowTermIsScheduled = false;

            mShowableTerms.Clear ();
            mSelectedCategories.Sort();
			foreach (KeyValuePair<string, ParsedTerm> kvp in mParsedTerms)
			{
				ParsedTerm parsedTerm = kvp.Value;
				if (ShouldShowTerm (parsedTerm.Term, parsedTerm.Category, parsedTerm.Usage, parsedTerm))
					mShowableTerms.Add(parsedTerm);
			}
            GUITools.RepaintInspectors();
            GUITools.ScheduleRepaintInspectors();
        }

        void OnGUI_KeyHeader (string sKey, string sCategory, string FullKey, int nUses, float YPosMin)
		{
			//--[ Toggle ]---------------------
			GUI.Box(new Rect(2, YPosMin+2, 18, mRowSize), "", "Toolbar");
			OnGUI_SelectableToogleListItem (new Rect(2, YPosMin+3, 15, mRowSize), FullKey, ref mSelectedKeys, "OL Toggle");

			bool bEnabled = mSelectedKeys.Contains (FullKey);
			//--[ Number of Objects using this key ]---------------------
			if (nUses >= 0) 
			{
				if (nUses == 0) 
				{
					GUI.color = Color.Lerp (Color.gray, Color.white, 0.5f);
					GUI.Label (new Rect(20, YPosMin+2, 30, mRowSize), nUses.ToString (), "toolbarbutton");
				}
				else 
				{
                    if (GUI.Button(new Rect(20, YPosMin + 2, 30, mRowSize), nUses.ToString(), "toolbarbutton"))
                    {
                        List<string> selection = new List<string>(mSelectedKeys);
                        if (!selection.Contains(FullKey))
                            selection.Add(FullKey);

                        List<GameObject> selGOs = new List<GameObject>();
                        for (int i=0; i<selection.Count; ++i)
                            selGOs.AddRange( FindObjectsUsingKey(selection[i]) );


                        if (selGOs.Count > 0)
                            Selection.objects = selGOs.ToArray();
                        else
                            ShowWarning("The selected Terms are not used in this Scene. Try opening other scenes");
                    }
                }
			}
			else 
			{
				GUI.color = Color.Lerp (Color.red, Color.white, 0.6f);
				if (GUI.Button (new Rect(20, YPosMin+2, 30, mRowSize), "", "toolbarbutton")) 
				{
					mCurrentToolsMode = eToolsMode.Parse;
					mCurrentViewMode = eViewMode.Tools;
				}
			}
			GUI.color = Color.white;

			TermData termData = ShowTerm_termData!=null ? ShowTerm_termData : mLanguageSource.GetTermData (FullKey);
			bool bKeyIsMissing = termData == null;
			float MinX = 50;
			if (bKeyIsMissing) 
			{
				Rect rect = new Rect(50, YPosMin+2, mRowSize, mRowSize+2);
                GUITools.DrawSkinIcon(rect, "CN EntryWarnIcon", "CN EntryWarn");
				GUI.Label (rect, new GUIContent ("", "This term is used in the scene, but its not localized in the Language Source"));
				MinX += rect.width;
			}
			else MinX += 3;

            float listWidth = Mathf.Max(EditorGUIUtility.currentViewWidth / EditorGUIUtility.pixelsPerPoint, mTermList_MaxWidth);
            Rect rectKey = new Rect(MinX, YPosMin+2, listWidth-MinX, mRowSize);
            if (sCategory != LanguageSourceData.EmptyCategory)
                rectKey.width -= 130;
            if (mKeyToExplore == FullKey) 
			{
				GUI.backgroundColor = Color.Lerp (Color.blue, Color.white, 0.8f);
				if (GUI.Button (rectKey, new GUIContent (sKey, EditorStyles.foldout.onNormal.background), LocalizeInspector.GUIStyle_OldTextArea)) 
				{
					mKeyToExplore = string.Empty;
                    ScheduleUpdateTermsToShowInList();
                    ClearErrors ();
				}
				GUI.backgroundColor = Color.white;
			}
			else 
			{
				GUIStyle LabelStyle = EditorStyles.label;
				if (!bKeyIsMissing && !TermHasAllTranslations (mLanguageSource, termData)) 
				{
					LabelStyle = new GUIStyle (EditorStyles.label);
					LabelStyle.fontStyle = FontStyle.Italic;
					GUI.color = Color.Lerp (Color.white, Color.yellow, 0.5f);
				}
				if (!bEnabled)
					GUI.contentColor = Color.Lerp (Color.gray, Color.white, 0.3f);
				if (GUI.Button (rectKey, sKey, LabelStyle)) 
				{
					SelectTerm (FullKey);
					ClearErrors ();
				}
				if (!bEnabled)
					GUI.contentColor = Color.white;
				GUI.color = Color.white;
			}
			//--[ Category ]--------------------------
			if (sCategory != LanguageSourceData.EmptyCategory) 
			{
				if (mKeyToExplore == FullKey) 
				{
                    rectKey.x = listWidth - 100-38-20;
					rectKey.width = 130;
					if (GUI.Button (rectKey, sCategory, EditorStyles.toolbarButton))
						OpenTool_ChangeCategoryOfSelectedTerms ();
				}
				else
				{
					GUIStyle stl = new GUIStyle(EditorStyles.miniLabel);
					stl.alignment = TextAnchor.MiddleRight;
                    rectKey.width = 130;//EditorStyles.miniLabel.CalcSize(new GUIContent(sCategory)).x;
					rectKey.x = listWidth - rectKey.width - 38-20;

					if (GUI.Button (rectKey, sCategory, stl)) 
					{
						SelectTerm (FullKey);
						ClearErrors ();
					}
				}
			}
        }


        void CalculateTermsListMaxWidth()
		{
            mTermList_MaxWidth = EditorGUIUtility.currentViewWidth / EditorGUIUtility.pixelsPerPoint - 120;
            /*float maxWidth = Screen.width / 18;
			foreach (KeyValuePair<string, ParsedTerm> kvp in mParsedTerms)
			{
                var txt = kvp.Key;
                if (txt.Length > 100)
                    txt = txt.Substring(0, 100);
                var size = EditorStyles.label.CalcSize(new GUIContent(txt));
				mTermList_MaxWidth  = Mathf.Max (mTermList_MaxWidth, size.x);
			}*/
        }

		bool TermHasAllTranslations( LanguageSourceData source, TermData data )
		{
            if (source==null) source = LocalizationManager.Sources[0];
			for (int i=0, imax=data.Languages.Length; i<imax; ++i)
			{
				bool isLangEnabled = source.mLanguages.Count>i ? source.mLanguages[i].IsEnabled() : true;
				if (string.IsNullOrEmpty(data.Languages[i]) && isLangEnabled)
					return false;
			}
			return true;
		}

		void OnGUI_KeysList_AddKey()
		{
			GUILayout.BeginHorizontal();
				GUI.color = Color.Lerp(Color.gray, Color.white, 0.5f);
				bool bWasEnabled = mTermsList_NewTerm!=null;
				bool bEnabled = !GUILayout.Toggle (!bWasEnabled, "+", EditorStyles.toolbarButton, GUILayout.Width(30));
				GUI.color = Color.white;

				if (bWasEnabled  && !bEnabled) mTermsList_NewTerm = null;
				if (!bWasEnabled &&  bEnabled) mTermsList_NewTerm = string.Empty;

				if (bEnabled)
				{
					GUILayout.BeginHorizontal(EditorStyles.toolbar);
					mTermsList_NewTerm = EditorGUILayout.TextField(mTermsList_NewTerm, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
					GUILayout.EndHorizontal();

					LanguageSourceData.ValidateFullTerm( ref mTermsList_NewTerm );
					if (string.IsNullOrEmpty(mTermsList_NewTerm) || mLanguageSource.ContainsTerm(mTermsList_NewTerm) || mTermsList_NewTerm=="-")
						GUI.enabled = false;
	
					if (TestButton (eTest_ActionType.Button_AddTerm_InTermsList, "Create Key", "toolbarbutton", GUILayout.ExpandWidth(false)))
					{
						AddLocalTerm(mTermsList_NewTerm);
						SelectTerm( mTermsList_NewTerm );
						ClearErrors();
						mTermsList_NewTerm = null;
						SetAllTerms_When_InferredTerms_IsInSource ();
					}
					GUI.enabled = true;
				}
			GUILayout.EndHorizontal();
		}

		void OpenTool_ChangeCategoryOfSelectedTerms()
		{
			mCurrentViewMode = eViewMode.Tools;
			mCurrentToolsMode = eToolsMode.Categorize;
			if (!string.IsNullOrEmpty(mKeyToExplore) && !mSelectedKeys.Contains(mKeyToExplore))
				mSelectedKeys.Add(mKeyToExplore);
			mSelectedKeys.Sort();
		}

        void OnGUI_SelectedCategories()
        {
            if (mParsedCategories.Count == 0)
                return;

            string text = "Categories";
            if (mSelectedCategories.Count() == 0)
                text = "无 Nothing";
            else
            if (mSelectedCategories.Count() == mParsedCategories.Count)
                text = "所有 Everything";
            else
                text = mSelectedCategories.Count + " categories";

            if (GUILayout.Button(new GUIContent(text), "toolbarbutton", GUILayout.Width(100)))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("所有 Everything"), false, () =>
                {
                    mSelectedCategories.Clear();
                    mSelectedCategories.AddRange(mParsedCategories);
                    ScheduleUpdateTermsToShowInList();
                });
                menu.AddItem(new GUIContent("无 Nothing"), false, () =>
                {
                    mSelectedCategories.Clear();
                    ScheduleUpdateTermsToShowInList();
                });
                menu.AddSeparator("");

                var parsedList = mParsedCategories.OrderBy(x=>x).ToList();
                for (int i=0, imax=parsedList.Count; i<imax ; ++i)
                {
                    var category = parsedList[i];
                    var nextCategory = i + 1 < imax ? parsedList[i + 1] : null;

                    bool isHeader = nextCategory != null && nextCategory.StartsWith(category + "/");

                    var displayName = category;
                    var categoryRoot = category;
                    if (isHeader)
                    {
                        categoryRoot += "/";
                        var newCateg = !category.Contains('/') ? category : category.Substring(category.LastIndexOf('/') + 1);
                        displayName = categoryRoot + newCateg;
                    }

                    menu.AddItem(new GUIContent(displayName), !string.IsNullOrEmpty(mSelectedCategories.FirstOrDefault(x=>x.StartsWith(categoryRoot))), () =>
                    {
                        var CatHeader = category + "/";
                        if (mSelectedCategories.Contains(category))
                        {
                            mSelectedCategories.Remove(category);

                            if (isHeader)
                            {
                                mSelectedCategories.RemoveAll(x => x.StartsWith(CatHeader));
                            }
                        }
                        else
                        {
                            mSelectedCategories.Add(category);
                            if (isHeader)
                            {
                                mSelectedCategories.AddRange( parsedList.Where(x=>x.StartsWith(CatHeader)));
                            }
                        }
                        ScheduleUpdateTermsToShowInList();
                    });
                    if (isHeader)
                    {
                        menu.AddSeparator(category+"/");
                    }
                }

                menu.ShowAsContext();
            }
        }

        void SaveSelectedCategories()
		{
			if (mSelectedCategories.Count == 0) {
				EditorPrefs.DeleteKey ("I2 CategoryFilter");
			} else {
				var data = string.Join(",", mSelectedCategories.ToArray());
				EditorPrefs.SetString ("I2 CategoryFilter", data);
			}
		}

		void LoadSelectedCategories()
		{
			var data = EditorPrefs.GetString ("I2 CategoryFilter", null);
			if (!string.IsNullOrEmpty(data))
			{
				mSelectedCategories.Clear ();
				mSelectedCategories.AddRange( data.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
			}
		}


		// Bottom part of the Key list (buttons: All, None, Used,...  to select the keys)
		void OnGUI_Keys_ListSelection ( int KeyListFilterID )
		{
			GUILayout.BeginHorizontal( "toolbarbutton" );

			if (TestButton( eTest_ActionType.Button_SelectTerms_All, new GUIContent( "所有 All", "选择列表中的所有\nTerms Selects All Terms in the list" ), "toolbarbutton", GUILayout.ExpandWidth( false ) ))
			{
				mSelectedKeys.Clear();
				foreach (var kvp in mParsedTerms)
					if (ShouldShowTerm( kvp.Value.Term, kvp.Value.Category, kvp.Value.Usage ))
						mSelectedKeys.Add( kvp.Key );
			}
			if (GUILayout.Button( new GUIContent( "清除 None", "清除选区\nClears the selection" ), "toolbarbutton", GUILayout.ExpandWidth( false ) )) { mSelectedKeys.Clear(); }
			GUILayout.Space( 5 );

			GUI.enabled = (mFlagsViewKeys & (int)eFlagsViewKeys.Used)>1;
			if (TestButton(eTest_ActionType.Button_SelectTerms_Used, new GUIContent( "选择 Used", "选择已解析场景中引用的所有Terms\nSelects All Terms referenced in the parsed scenes" ), "toolbarbutton", GUILayout.ExpandWidth( false ) ))
			{
				mSelectedKeys.Clear();
				foreach (var kvp in mParsedTerms)
					if (kvp.Value.Usage > 0 && ShouldShowTerm( kvp.Value.Term, kvp.Value.Category, kvp.Value.Usage ))
						mSelectedKeys.Add( kvp.Key );
			}
			GUI.enabled = (mFlagsViewKeys & (int)eFlagsViewKeys.NotUsed)>1;
			if (GUILayout.Button( new GUIContent( "未使用 Not Used", "从源中选择所有未使用的术语。\nSelects all Terms from the Source that are not been used" ), "toolbarbutton", GUILayout.ExpandWidth( false ) ))
			{
				mSelectedKeys.Clear();
				foreach (var kvp in mParsedTerms)
					if (kvp.Value.Usage == 0 && ShouldShowTerm( kvp.Value.Term, kvp.Value.Category, kvp.Value.Usage ))
						mSelectedKeys.Add( kvp.Key );
			}

			GUI.enabled = (mFlagsViewKeys & (int)eFlagsViewKeys.Missing)>1;
			if (TestButton(eTest_ActionType.Button_SelectTerms_Missing, new GUIContent( "未定义 Missing", "选择所有已使用但未在源中定义的术语\nSelects all Terms Used but not defined in the Source" ), "toolbarbutton", GUILayout.ExpandWidth( false ) ))
			{
				mSelectedKeys.Clear();
				foreach (var kvp in mParsedTerms)
					if (!mLanguageSource.ContainsTerm( kvp.Key ) && ShouldShowTerm( kvp.Value.Term, kvp.Value.Category, kvp.Value.Usage ))
						mSelectedKeys.Add( kvp.Key );
			}
			GUI.enabled = true;
			EditorGUI.BeginChangeCheck();

			// Terms Filter
			{
				//KeyList_Filter = EditorGUILayout.TextField(KeyList_Filter, GUI.skin.GetStyle("ToolbarSeachTextField"), GUILayout.ExpandWidth(true));
				GUILayout.Label( "", GUILayout.ExpandWidth( true ) );
				mKeyListFilterRect = GUILayoutUtility.GetLastRect();
                mKeyListFilterRect.xMax += 4;

                //KeyList_Filter = GUITools.TextField( mKeyListFilterRect, KeyList_Filter, 255, GUI.skin.GetStyle( "ToolbarSeachTextField" ), KeyListFilterID );
                KeyList_Filter = GUITools.TextField( mKeyListFilterRect, KeyList_Filter, 255, GUI.skin.GetStyle( "TextField" ), KeyListFilterID );
			}
			
			//if (GUILayout.Button( string.Empty, string.IsNullOrEmpty( KeyList_Filter ) ? "ToolbarSeachCancelButtonEmpty" : "ToolbarSeachCancelButton", GUILayout.ExpandWidth( false ) ))
			if (GUILayout.Button( string.Empty, string.IsNullOrEmpty( KeyList_Filter ) ? "Button" : "Button", GUILayout.ExpandWidth( false ) ))
			{
				KeyList_Filter = string.Empty;
				EditorApplication.update += RepaintScene;
				GUI.FocusControl( "" );
			}

            string filterHelp = "国际规则的选项:\n文本-显示所有匹配文本的键/类别\nc文本-显示文本类别的所有条款\nf文本-显示翻译中有“文本”的术语\n\nFiter Options:\ntext - shows all key/categories matching text\nc text - shows all terms of the text category\nf text - show terms having 'text' in their translations";
            GUILayout.Space(-5);
            GUI.contentColor = new Color(1, 1, 1, 0.5f);
            GUILayout.Label(new GUIContent(GUITools.Icon_Help.image, filterHelp), GUITools.DontExpandWidth);
            GUI.contentColor = GUITools.White;
            GUILayout.Space(-5);




            if (EditorGUI.EndChangeCheck())
			{
				mShowableTerms.Clear();
				GUI.changed = false;
			}

			GUILayout.EndHorizontal();
		}
		
		
		#endregion

		#region Filtering

		public bool ShouldShowTerm (string FullTerm)
		{
			ParsedTerm termData;
			if (!mParsedTerms.TryGetValue(FullTerm, out termData))
				return false;
			
			return ShouldShowTerm (termData.Term, termData.Category, termData.Usage, termData);
		}

		private static TermData ShowTerm_termData;
		public static bool ShouldShowTerm (string Term, string Category, int nUses, ParsedTerm parsedTerm=null )
		{
            if (!string.IsNullOrEmpty(Category) && !mSelectedCategories.Contains(Category))
                return false;
            if (Term == "-")
                return false;


            var fullTerm = Term;
            if (!string.IsNullOrEmpty(Category) && Category != LanguageSourceData.EmptyCategory)
                fullTerm = Category + "/" + Term;

			if (parsedTerm != null && parsedTerm.termData != null)
				ShowTerm_termData = parsedTerm.termData;
			else
			{
				ShowTerm_termData = mLanguageSource.GetTermData (fullTerm);
				if (parsedTerm!=null)
					parsedTerm.termData = ShowTerm_termData;
			}


            var filter = KeyList_Filter.Trim();
            bool useTranslation = filter.StartsWith("f ", StringComparison.OrdinalIgnoreCase);
            if (useTranslation)
            {
                if (ShowTerm_termData == null)
                    return false;

                filter = filter.Substring(2).Trim();
                if (!string.IsNullOrEmpty(filter))
                {
                    bool hasFilter = false;
                    for (int i = 0; i < ShowTerm_termData.Languages.Length; ++i)
                    {
                        if (!string.IsNullOrEmpty(ShowTerm_termData.Languages[i]) && StringContainsFilter(ShowTerm_termData.Languages[i], filter))
                        {
                            hasFilter = true;
                            break;
                        }
                    }
                    if (!hasFilter)
                        return false;
                }
            }
            else
            {
                bool onlyCategory = filter.StartsWith("c ", StringComparison.OrdinalIgnoreCase);
                if (onlyCategory)
                    filter = filter.Substring(2).Trim();

                if (!string.IsNullOrEmpty(filter))
                {
                    bool matchesCategory = StringContainsFilter(Category, filter);
                    bool matchesName = !onlyCategory && StringContainsFilter(Term, filter);

                    if (!matchesCategory && !matchesName)
                        return false;
                }
            }


            bool bIsMissing = ShowTerm_termData == null;
			if (nUses<0) return true;

			if ((mFlagsViewKeys & (int)eFlagsViewKeys.Missing)>0 && bIsMissing) return true;
			if ((mFlagsViewKeys & (int)eFlagsViewKeys.Missing)==0 && bIsMissing) return false;

			if ((mFlagsViewKeys & (int)eFlagsViewKeys.Used)>0 && nUses>0) return true;
			if ((mFlagsViewKeys & (int)eFlagsViewKeys.NotUsed)>0 && nUses==0) return true;

			return false;
		}

		static bool StringContainsFilter( string Term, string Filter )
		{
            if (string.IsNullOrEmpty(Filter) || string.IsNullOrEmpty(Term))
                return true;
            if (Term == "-")
                return false;
            Term = Term.ToLower();
            string[] Filters = Filter.ToLower().Split(";, ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0, imax = Filters.Length; i < imax; ++i)
                if (Term.Contains(Filters[i]))
                    return true;

            return false;
		}

		#endregion
		
		#region Add/Remove Keys to DB
		
		void AddTermsToSource()
		{
			if (!string.IsNullOrEmpty (mKeyToExplore) && !mSelectedKeys.Contains(mKeyToExplore))
				mSelectedKeys.Add (mKeyToExplore);

			for (int i=mSelectedKeys.Count-1; i>=0; --i)
			{
				string key = mSelectedKeys[i];

				if (!ShouldShowTerm(key))
					continue;

				AddLocalTerm(key);
				mSelectedKeys.RemoveAt(i);
			}
			SetAllTerms_When_InferredTerms_IsInSource ();
		}
		
		void RemoveTermsFromSource()
		{
            if (mTestAction==eTest_ActionType.None && !EditorUtility.DisplayDialog("Confirm delete", "Are you sure you want to delete the selected terms", "Yes", "Cancel"))
                return;

            if (!string.IsNullOrEmpty (mKeyToExplore) && !mSelectedKeys.Contains(mKeyToExplore))
				mSelectedKeys.Add (mKeyToExplore);

			for (int i=mSelectedKeys.Count-1; i>=0; --i)
			{
				string key = mSelectedKeys[i];
				
				if (!ShouldShowTerm(key)) 
					continue;

                mLanguageSource.RemoveTerm(key);
                RemoveParsedTerm(key);
                mSelectedKeys.Remove(key);
			}

            mKeyToExplore = string.Empty;
            mTermList_MaxWidth = -1;
            serializedObject.ApplyModifiedProperties();
            mLanguageSource.Editor_SetDirty();

            EditorApplication.update += DoParseTermsInCurrentScene;
			EditorApplication.update += RepaintScene;
		}

		#endregion
		
		#region Select Objects in Current Scene


		public static void SelectTerm( string Key, bool SwitchToKeysTab=false )
		{
			GUI.FocusControl(null);
			mKeyToExplore = Key;
			mKeysDesc_AllowEdit = false;
			if (SwitchToKeysTab)
				mCurrentViewMode = eViewMode.Keys;
		}


		void SelectObjectsUsingKey( string Key )
		{
			List<GameObject> SelectedObjs = FindObjectsUsingKey(Key);

			if (SelectedObjs.Count>0)
				Selection.objects = SelectedObjs.ToArray();
			else
				ShowWarning("The selected Terms are not used in this Scene. Try opening other scenes"); 
		}

        List<GameObject> FindObjectsUsingKey(string Key)
        {
            List<GameObject> SelectedObjs = new List<GameObject>();

            Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll(typeof(Localize));

            if (Locals == null)
                return SelectedObjs;

            for (int i = 0, imax = Locals.Length; i < imax; ++i)
            {
                Localize localize = Locals[i];
                if (localize == null || localize.gameObject == null || !GUITools.ObjectExistInScene(localize.gameObject))
                    continue;

                string Term, SecondaryTerm;
                localize.GetFinalTerms(out Term, out SecondaryTerm);

                if (Key == Term || Key == SecondaryTerm)
                    SelectedObjs.Add(localize.gameObject);
            }

            return SelectedObjs;
        }


        #endregion


        [MenuItem("Tools/I2 Localization/Refresh Localizations", false, 16)]
        public static void CallLocalizeAll()
        {
            LocalizationManager.LocalizeAll(true);
            HandleUtility.Repaint();
        }
    }
}