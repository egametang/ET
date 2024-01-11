//#define UGUI
//#define NGUI

using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables	
		internal static bool mKeysDesc_AllowEdit;
		internal static string GUI_SelectedSpecialization
		{
			get{
                if (string.IsNullOrEmpty(mGUI_SelectedSpecialization)) 
					mGUI_SelectedSpecialization = EditorPrefs.GetString ("I2Loc Specialization", "Any");
				return mGUI_SelectedSpecialization;
			}
			set{
                if (value!=mGUI_SelectedSpecialization) 
					EditorPrefs.SetString ("I2Loc Specialization", value);
				 mGUI_SelectedSpecialization = value;
			}
		}
		internal static string mGUI_SelectedSpecialization;

		internal static bool GUI_ShowDisabledLanguagesTranslation = true;

		internal static int mShowPlural = -1;
		#endregion
		
		#region Key Description
		
		void OnGUI_KeyList_ShowKeyDetails()
		{
			GUI.backgroundColor = Color.Lerp(Color.blue, Color.white, 0.9f);
            GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));
			OnGUI_Keys_Languages(mKeyToExplore, null);

            GUILayout.BeginHorizontal();
            if (TestButton(eTest_ActionType.Button_DeleteTerm, "Delete", "Button", GUILayout.ExpandWidth(true)))
            {
                if (mTestAction != eTest_ActionType.None || EditorUtility.DisplayDialog("Confirm delete", "Are you sure you want to delete term '" + mKeyToExplore + "'", "Yes", "Cancel"))
                    EditorApplication.update += DeleteCurrentKey;
            }

            if (GUILayout.Button("Rename"))
            {
                mCurrentViewMode = eViewMode.Tools;
                mCurrentToolsMode = eToolsMode.Merge;
                if (!mSelectedKeys.Contains(mKeyToExplore))
                    mSelectedKeys.Add(mKeyToExplore);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
		}

		void DeleteTerm( string Term, bool updateStructures = true )
		{
			mLanguageSource.RemoveTerm (Term);
			RemoveParsedTerm(Term);
			mSelectedKeys.Remove(Term);

			if (Term==mKeyToExplore)
				mKeyToExplore = string.Empty;

			if (updateStructures)
			{
				UpdateParsedCategories();
				mTermList_MaxWidth = -1;
				serializedObject.ApplyModifiedProperties();
				mLanguageSource.Editor_SetDirty();
				ScheduleUpdateTermsToShowInList();
			}
			EditorApplication.update += RepaintScene;
		}

		void RepaintScene()
		{
			EditorApplication.update -= RepaintScene;
			Repaint();
		}
		
		void DeleteCurrentKey()
		{
			EditorApplication.update -= DeleteCurrentKey;
			DeleteTerm (mKeyToExplore);

			mKeyToExplore = "";
			EditorApplication.update += DoParseTermsInCurrentScene;
		}

		TermData AddLocalTerm( string Term, bool AutoSelect = true )
		{
			var data = AddTerm(Term, AutoSelect);
			if (data==null)
				return null;

			mTermList_MaxWidth = -1;			
			serializedObject.ApplyModifiedProperties();
			mLanguageSource.Editor_SetDirty();
			return data;
		}

		static TermData AddTerm(string Term, bool AutoSelect = true, eTermType termType = eTermType.Text)
		{
			if (Term == "-" || string.IsNullOrEmpty(Term))
				return null;

			Term = I2Utils.GetValidTermName(Term, true);

			TermData data = mLanguageSource.AddTerm(Term, termType);
			GetParsedTerm(Term);
			string sCategory = LanguageSourceData.GetCategoryFromFullTerm(Term);
			mParsedCategories.Add(sCategory);

			if (AutoSelect)
			{
				if (!mSelectedKeys.Contains(Term))
					mSelectedKeys.Add(Term);

				if (!mSelectedCategories.Contains(sCategory))
					mSelectedCategories.Add(sCategory);
			}
			ScheduleUpdateTermsToShowInList();
            mLanguageSource.Editor_SetDirty();
            return data;
		}

		// this method shows the key description and the localization to each language
		public static TermData OnGUI_Keys_Languages( string Key, Localize localizeCmp, bool IsPrimaryKey=true )
		{
			if (Key==null)
				Key = string.Empty;

			TermData termdata = null;

            LanguageSourceData source = mLanguageSource;
            if (localizeCmp != null && localizeCmp.Source != null)
                source = localizeCmp.Source.SourceData;

			if (source==null)
				source = LocalizationManager.GetSourceContaining(Key, false);

			if (source==null)
			{
				if (localizeCmp == null)
					source = LocalizationManager.Sources[0];
				else
					source = LocalizationManager.GetSourceContaining(IsPrimaryKey ? localizeCmp.SecondaryTerm : localizeCmp.Term);
			}


			if (string.IsNullOrEmpty(Key))
			{
				EditorGUILayout.HelpBox( "选择要本地化的术语\nSelect a Term to Localize", MessageType.Info );
				return null;
			}

			termdata = source.GetTermData(Key);
			if (termdata==null && localizeCmp!=null)
			{
				var realSource = LocalizationManager.GetSourceContaining(Key, false);
				if (realSource != null)
				{
					termdata = realSource.GetTermData(Key);
					source = realSource;
				}
			}
			if (termdata==null)
			{
				if (Key == "-")
					return null;
				EditorGUILayout.HelpBox( string.Format("Key '{0}' 没有本地化，还是使用不同的语言源\nis not Localized or it is in a different Language Source", Key), MessageType.Error );
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Add Term to Source"))
				{
					var termType = eTermType.Text;
					if (localizeCmp!=null && localizeCmp.mLocalizeTarget != null)
					{
						termType = IsPrimaryKey ? localizeCmp.mLocalizeTarget.GetPrimaryTermType(localizeCmp)
							: localizeCmp.mLocalizeTarget.GetSecondaryTermType(localizeCmp);
					}

					AddTerm(Key, true, termType);
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
					
				return null;
			}

			//--[ Type ]----------------------------------
			if (localizeCmp==null)
			{
				GUILayout.BeginHorizontal();
					GUILayout.Label ("Type:", GUILayout.ExpandWidth(false));
				eTermType NewType = (eTermType)EditorGUILayout.EnumPopup(termdata.TermType, GUILayout.ExpandWidth(true));
				if (termdata.TermType != NewType)
					termdata.TermType = NewType;
				GUILayout.EndHorizontal();
			}


			//--[ Description ]---------------------------

			mKeysDesc_AllowEdit = GUILayout.Toggle(mKeysDesc_AllowEdit, "Description", EditorStyles.foldout, GUILayout.ExpandWidth(true));

			if (mKeysDesc_AllowEdit)
			{
				string NewDesc = EditorGUILayout.TextArea( termdata.Description, Style_WrapTextField );
				if (NewDesc != termdata.Description)
				{
					termdata.Description = NewDesc;
					source.Editor_SetDirty();
				}
			}
			else
				EditorGUILayout.HelpBox( string.IsNullOrEmpty(termdata.Description) ? "没有描述 No description" : termdata.Description, MessageType.Info );

			OnGUI_Keys_Language_SpecializationsBar (termdata, source);

			OnGUI_Keys_Languages(Key, ref termdata, localizeCmp, IsPrimaryKey, source);
            return termdata;
		}

		static void OnGUI_Keys_Languages( string Key, ref TermData termdata, Localize localizeCmp, bool IsPrimaryKey, LanguageSourceData source )
		{
			//--[ Languages ]---------------------------
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));

			OnGUI_Keys_LanguageTranslations(Key, localizeCmp, IsPrimaryKey, ref termdata, source);

			if (termdata.TermType == eTermType.Text)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
                if (TestButton(eTest_ActionType.Button_Term_TranslateAll, "Translate All", "Button", GUILayout.Width(85)))
                {
                    var termData = termdata;
                    GUITools.DelayedCall(() => TranslateLanguage( Key, termData, localizeCmp, source));
                    GUI.FocusControl(string.Empty);
                }
				GUILayout.EndHorizontal();
                OnGUI_TranslatingMessage();
            }
			GUILayout.EndVertical();
		}

        static void TranslateLanguage( string Key, TermData termdata, Localize localizeCmp, LanguageSourceData source)
        {
            ClearErrors();
            string mainText = localizeCmp == null ? LanguageSourceData.GetKeyFromFullTerm(Key) : localizeCmp.GetMainTargetsText();

            for (int i = 0; i < source.mLanguages.Count; ++i)
                if (source.mLanguages[i].IsEnabled() && string.IsNullOrEmpty(termdata.Languages[i]))
                {
                    var langIdx = i;
                    var term = termdata;
                    var i2source = source;
                    Translate(mainText, ref termdata, source.mLanguages[i].Code,
                                (translation, error) =>
                                {
                                    if (error != null)
                                        ShowError(error);
                                    else
                                    if (translation != null)
                                    {
                                        term.Languages[langIdx] = translation; //SetTranslation(langIdx, translation);
                                        i2source.Editor_SetDirty();
                                    }
                                }, null);
                }
        }

        static void OnGUI_TranslatingMessage()
        {
			if (GoogleTranslation.IsTranslating())
            {
                // Connection Status Bar
                int time = (int)(Time.realtimeSinceStartup % 2 * 2.5);
                string Loading = "Translating" + ".....".Substring(0, time);
                GUI.color = Color.gray;
                GUILayout.BeginHorizontal(LocalizeInspector.GUIStyle_OldTextArea);
                GUILayout.Label(Loading, EditorStyles.miniLabel);
                GUI.color = Color.white;
                if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                {
                    GoogleTranslation.CancelCurrentGoogleTranslations();
                }
                GUILayout.EndHorizontal();
				HandleUtility.Repaint ();
            }
        }

		static void OnGUI_Keys_Language_SpecializationsBar(TermData termData, LanguageSourceData source)
		{
            var activeSpecializations = termData.GetAllSpecializations();

			GUILayout.BeginHorizontal();

                var TabStyle = new GUIStyle(GUI.skin.FindStyle("dragtab"));
                TabStyle.fixedHeight = 0;

            //var ss = GUI.skin.FindStyle("TL tab left");
                var TabOpenStyle = new GUIStyle(GUI.skin.FindStyle("minibuttonmid"));
                TabOpenStyle.margin.right = -1;
                var TabCloseStyle = new GUIStyle(EditorStyles.label);
                //var TabCloseStyle = new GUIStyle(GUI.skin.FindStyle("TL tab right"));
                TabCloseStyle.margin.left = -1;
                TabCloseStyle.padding.left=4;

                //-- Specialization Tabs -----

                var prevSpecialization = "Any";
                foreach (var specialization in SpecializationManager.Singleton.mSpecializations)
                {
                    if (!activeSpecializations.Contains(specialization) && specialization != GUI_SelectedSpecialization)
                        continue;

                    bool isActive = specialization == GUI_SelectedSpecialization;
                    var labelContent = new GUIContent(specialization, "Specialization of the main translation (i.e. variants that show only on specific platforms or devices)\nThis allows using 'tap' instead of 'click' for touch devices.");

                    if (isActive && activeSpecializations.Count>1)
                    {
                        GUILayout.BeginHorizontal(TabOpenStyle);
                            GUILayout.Toggle(isActive, labelContent, TabStyle, GUILayout.Height(20), GUILayout.ExpandWidth(false));
                            //GUILayout.Label(labelContent, TabOpenStyle);
                            if (specialization != "Any" && GUILayout.Button("x", TabCloseStyle, GUILayout.Width(15)))
                            {
                                termData.RemoveSpecialization(specialization);
                                GUI_SelectedSpecialization = prevSpecialization;
                                GUI.FocusControl(null);
                            }
                        GUILayout.EndHorizontal();
                    }
                    else
                    if (GUILayout.Toggle(isActive, labelContent, TabStyle, GUILayout.Height(25), GUILayout.ExpandWidth(false)) && !isActive)
                    {
                        GUI_SelectedSpecialization = specialization;
                        GUI.FocusControl(null);
                    }
                }


                //-- Add new Specialization -----
                int newIndex = EditorGUILayout.Popup(-1, SpecializationManager.Singleton.mSpecializations, "DropDown", GUILayout.Width(20));
                if (newIndex>=0)
                {
                    string newSpecialization = SpecializationManager.Singleton.mSpecializations[newIndex];
                    if (!activeSpecializations.Contains(newSpecialization))
                    {
                        for (int iLang = 0; iLang < source.mLanguages.Count; ++iLang)
                        {
                            string Translation = termData.GetTranslation(iLang, GUI_SelectedSpecialization, editMode: true);
                            termData.SetTranslation(iLang, Translation, GUI_SelectedSpecialization);
                        }
                        GUI_SelectedSpecialization = newSpecialization;
                    }
                }

                GUILayout.FlexibleSpace();


                GUI_ShowDisabledLanguagesTranslation = GUILayout.Toggle(GUI_ShowDisabledLanguagesTranslation, new GUIContent("L", "Show Disabled Languages"), "Button", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			GUILayout.Space(-3);
		}

		static void OnGUI_Keys_LanguageTranslations (string Key, Localize localizeCmp, bool IsPrimaryKey, ref TermData termdata, LanguageSourceData source)
		{
			bool IsSelect = Event.current.type==EventType.MouseUp;
			for (int i=0; i< source.mLanguages.Count; ++ i)
			{
				bool forcePreview = false;
				bool isEnabledLanguage = source.mLanguages[i].IsEnabled();

				if (!isEnabledLanguage)
				{
					if (!GUI_ShowDisabledLanguagesTranslation)
						continue;
					GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.35f);
				}
				GUILayout.BeginHorizontal();

                if (GUILayout.Button(source.mLanguages[i].Name, EditorStyles.label, GUILayout.Width(100)))
					forcePreview = true;


				string Translation = termdata.GetTranslation(i, GUI_SelectedSpecialization, editMode:true);
                if (Translation == null) Translation = string.Empty;

//				if (termdata.Languages[i] != termdata.Languages_Touch[i] && !string.IsNullOrEmpty(termdata.Languages[i]) && !string.IsNullOrEmpty(termdata.Languages_Touch[i]))
//					GUI.contentColor = GUITools.LightYellow;

				if (termdata.TermType == eTermType.Text || termdata.TermType==eTermType.Child)
				{
					EditorGUI.BeginChangeCheck ();
					string CtrName = "TranslatedText"+i;
					GUI.SetNextControlName(CtrName);

					EditPluralTranslations (ref Translation, i, source.mLanguages[i].Code);
                    //Translation = EditorGUILayout.TextArea(Translation, Style_WrapTextField, GUILayout.Width(Screen.width - 260 - (autoTranslated ? 20 : 0)));
					if (EditorGUI.EndChangeCheck ())
					{
                        termdata.SetTranslation(i, Translation, GUI_SelectedSpecialization);
						source.Editor_SetDirty();
                        forcePreview = true;
                    }

					if (localizeCmp!=null &&
						(forcePreview || /*GUI.changed || */GUI.GetNameOfFocusedControl()==CtrName && IsSelect))
					{
						if (IsPrimaryKey && string.IsNullOrEmpty(localizeCmp.Term))
						{
							localizeCmp.mTerm = Key;
						}

						if (!IsPrimaryKey && string.IsNullOrEmpty(localizeCmp.SecondaryTerm))
						{
							localizeCmp.mTermSecondary = Key;
						}

						string PreviousLanguage = LocalizationManager.CurrentLanguage;
						LocalizationManager.PreviewLanguage(source.mLanguages[i].Name);
						if (forcePreview || IsSelect)
							LocalizationManager.LocalizeAll();
						else
							localizeCmp.OnLocalize(true);
						LocalizationManager.PreviewLanguage(PreviousLanguage);
						EditorUtility.SetDirty(localizeCmp);
					}
					GUI.contentColor = Color.white;

                    //if (autoTranslated)
                    //{
                    //    if (GUILayout.Button(new GUIContent("\u2713"/*"A"*/,"Translated by Google Translator\nClick the button to approve the translation"), EditorStyles.toolbarButton, GUILayout.Width(autoTranslated ? 20 : 0)))
                    //    {
                    //        termdata.Flags[i] &= (byte)(byte.MaxValue ^ (byte)(GUI_SelectedSpecialization==0 ? TranslationFlag.AutoTranslated_Normal : TranslationFlag.AutoTranslated_Touch));
                    //    }
                    //}

                    if (termdata.TermType == eTermType.Text)
                    {
                        if (TestButtonArg(eTest_ActionType.Button_Term_Translate, i, new GUIContent("T", "Translate"), EditorStyles.toolbarButton, GUILayout.Width(20)))
                        {
                            var termData = termdata;
                            var indx = i;
                            var key = Key;
                            GUITools.DelayedCall(()=>TranslateTerm(key, termData, source, indx));
                            GUI.FocusControl(string.Empty);
                        }
                    }
				}
				else
				{
					string MultiSpriteName = string.Empty;

					if (termdata.TermType==eTermType.Sprite && Translation.EndsWith("]", StringComparison.Ordinal))	// Handle sprites of type (Multiple):   "SpritePath[SpriteName]"
					{
						int idx = Translation.LastIndexOf("[", StringComparison.Ordinal);
						int len = Translation.Length-idx-2;
						MultiSpriteName = Translation.Substring(idx+1, len);
						Translation = Translation.Substring(0, idx);
					}

					Object Obj = null;

					// Try getting the asset from the References section
					if (localizeCmp!=null)
						Obj = localizeCmp.FindTranslatedObject<Object>(Translation);
					if (Obj==null && source != null)
						Obj = source.FindAsset(Translation);

					// If it wasn't in the references, Load it from Resources
					if (Obj==null && localizeCmp==null)
						Obj = ResourceManager.pInstance.LoadFromResources<Object>(Translation);

                    Type ObjType = typeof(Object);
 					switch (termdata.TermType)
					{
						case eTermType.Font			: ObjType = typeof(Font); break;
						case eTermType.Texture		: ObjType = typeof(Texture); break;
						case eTermType.AudioClip	: ObjType = typeof(AudioClip); break;
						case eTermType.GameObject	: ObjType = typeof(GameObject); break;
						case eTermType.Sprite		: ObjType = typeof(Sprite); break;
                        case eTermType.Material     : ObjType = typeof(Material); break;
                        case eTermType.Mesh         : ObjType = typeof(Mesh); break;
#if NGUI
						case eTermType.UIAtlas		: ObjType = typeof(UIAtlas); break;
						case eTermType.UIFont		: ObjType = typeof(UIFont); break;
#endif
#if TK2D
						case eTermType.TK2dFont			: ObjType = typeof(tk2dFont); break;
						case eTermType.TK2dCollection	: ObjType = typeof(tk2dSpriteCollection); break;
#endif

#if TextMeshPro
                        case eTermType.TextMeshPFont	: ObjType = typeof(TMP_FontAsset); break;
#endif

#if SVG
						case eTermType.SVGAsset	: ObjType = typeof(SVGImporter.SVGAsset); break;
#endif

						case eTermType.Object		: ObjType = typeof(Object); break;
					}

					if (Obj!=null && !string.IsNullOrEmpty(MultiSpriteName))
					{
						string sPath = AssetDatabase.GetAssetPath(Obj);
						Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(sPath);
						Obj = null;
						for (int j=0, jmax=objs.Length; j<jmax; ++j)
							if (objs[j].name.Equals(MultiSpriteName))
							{
								Obj = objs[j];
								break;
							}
					}

					bool bShowTranslationLabel = Obj==null && !string.IsNullOrEmpty(Translation);
					if (bShowTranslationLabel)
					{
						GUI.backgroundColor=GUITools.DarkGray;
						GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));
						GUILayout.Space(2);
						
						GUI.backgroundColor = Color.white;
					}

					Object NewObj = EditorGUILayout.ObjectField(Obj, ObjType, true, GUILayout.ExpandWidth(true));
					if (Obj!=NewObj)
					{
                        string sPath = null;
                        if (NewObj != null)
                        {
                            sPath = AssetDatabase.GetAssetPath(NewObj);

                            mCurrentInspector.serializedObject.ApplyModifiedProperties();
                            foreach (var cmp in mCurrentInspector.serializedObject.targetObjects)
                            {
                                AddObjectPath(ref sPath, cmp as Localize, NewObj);
                            }
                            mCurrentInspector.serializedObject.ApplyModifiedProperties();

                            if (HasObjectInReferences(NewObj, localizeCmp))
                                sPath = NewObj.name;
                            else
                            if (termdata.TermType == eTermType.Sprite)
                                sPath += "[" + NewObj.name + "]";
                        }

                        termdata.SetTranslation(i, sPath, GUI_SelectedSpecialization);
						source.Editor_SetDirty();
					}

					if (bShowTranslationLabel)
					{
						GUILayout.BeginHorizontal();
							GUI.color = Color.red;
							GUILayout.FlexibleSpace();
							GUILayout.Label (Translation, EditorStyles.miniLabel);
							GUILayout.FlexibleSpace();
							GUI.color = Color.white;
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
					}
				}
				
				GUILayout.EndHorizontal();
				GUI.color = Color.white;
			}
		}

        private static void TranslateTerm(string Key, TermData termdata, LanguageSourceData source, int i)
        {
            string sourceText = null;
            string sourceLangCode = null;
            FindTranslationSource(Key, termdata, source.mLanguages[i].Code, null, out sourceText, out sourceLangCode);

            var term = termdata;
            var specialization = GUI_SelectedSpecialization;
            var langIdx = i;
            var i2source = source;
            Translate(sourceText, ref termdata, source.mLanguages[i].Code, (translation, error) => 
            {
                term.SetTranslation(langIdx, translation, specialization);
                i2source.Editor_SetDirty();
            }, specialization);
        }

        static void EditPluralTranslations( ref string translation, int langIdx, string langCode )
		{
			bool hasParameters = false;
			int paramStart = translation.IndexOf("{[");
			hasParameters = paramStart >= 0 && translation.IndexOf ("]}", paramStart) > 0;

			if (mShowPlural == langIdx && string.IsNullOrEmpty (translation))
				mShowPlural = -1;
				
			bool allowPlural = hasParameters || translation.Contains("[i2p_");

			if (allowPlural) 
			{
				if (GUILayout.Toggle (mShowPlural == langIdx, "", EditorStyles.foldout, GUILayout.Width (13)))
					mShowPlural = langIdx;
				else if (mShowPlural == langIdx)
					mShowPlural = -1;

				GUILayout.Space (-5);
			}

			string finalTranslation = "";
			bool unfolded = mShowPlural == langIdx;
			bool isPlural = allowPlural && translation.Contains("[i2p_");
			if (unfolded) 
				GUILayout.BeginVertical ("Box");

                ShowPluralTranslation("Plural", langCode,  translation, ref finalTranslation, true, unfolded, unfolded|isPlural );
				ShowPluralTranslation("Zero", langCode, translation, ref finalTranslation, unfolded, true, true );
				ShowPluralTranslation("One", langCode, translation, ref finalTranslation, unfolded, true, true );
				ShowPluralTranslation("Two", langCode, translation, ref finalTranslation, unfolded, true, true );
				ShowPluralTranslation("Few", langCode, translation, ref finalTranslation, unfolded, true, true );
				ShowPluralTranslation("Many", langCode, translation, ref finalTranslation, unfolded, true, true );

			if (unfolded) 
				GUILayout.EndVertical ();

			translation = finalTranslation;
		}

		static void ShowPluralTranslation(string pluralType, string langCode, string translation, ref string finalTranslation, bool show, bool allowDelete, bool showTag )
		{
			string tag = "[i2p_" + pluralType + "]";
			int idx0 = translation.IndexOf (tag, StringComparison.OrdinalIgnoreCase);
			bool hasTranslation = idx0 >= 0 || pluralType=="Plural";
			if (idx0 < 0) idx0 = 0;
					 else idx0 += tag.Length;

			int idx1 = translation.IndexOf ("[i2p_", idx0, StringComparison.OrdinalIgnoreCase);
			if (idx1 < 0) idx1 = translation.Length;

			var pluralTranslation = translation.Substring(idx0, idx1-idx0);
			var newTrans = pluralTranslation;

			bool allowPluralForm = GoogleLanguages.LanguageHasPluralType (langCode, pluralType);

			if (hasTranslation && !allowPluralForm) {
				newTrans = "";
				GUI.changed = true;
			}

			if (show && allowPluralForm)
			{
				if (!hasTranslation)
					GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.35f);
				
				GUILayout.BeginHorizontal ();
					if (showTag)
						GUILayout.Label (pluralType, EditorStyles.miniLabel, GUILayout.Width(35));
					newTrans = EditorGUILayout.TextArea (pluralTranslation, Style_WrapTextField);

					if (allowDelete  && GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(15)))
					{
						newTrans = string.Empty;
						GUI.changed = true;
						GUIUtility.keyboardControl = 0;
					}
					
				GUILayout.EndHorizontal ();
				if (!hasTranslation)
					GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);
			}

			if (!string.IsNullOrEmpty (newTrans)) 
			{
				if (hasTranslation || newTrans != pluralTranslation) 
				{
					if (pluralType != "Plural")
						finalTranslation += tag;
					finalTranslation += newTrans;
				}
			}
		}

		/*static public int DrawTranslationTabs( int Index )
		{
			GUIStyle MyStyle = new GUIStyle(GUI.skin.FindStyle("dragtab"));
			MyStyle.fixedHeight=0;
			
			GUILayout.BeginHorizontal();
			for (int i=0; i<Tabs.Length; ++i)
			{
				if ( GUILayout.Toggle(Index==i, Tabs[i], MyStyle, GUILayout.Height(height)) && Index!=i) 
					Index=i;
			}
			GUILayout.EndHorizontal();
			return Index;
		}*/

		static bool HasObjectInReferences( Object obj, Localize localizeCmp )
		{
			if (localizeCmp!=null && localizeCmp.TranslatedObjects.Contains(obj))
				return true;

			if (mLanguageSource!=null && mLanguageSource.Assets.Contains(obj))
				return true;

			return false;
		}

		static void AddObjectPath( ref string sPath, Localize localizeCmp, Object NewObj )
		{
			if (I2Utils.RemoveResourcesPath (ref sPath))
				return;

			// If its not in the Resources folder and there is no object reference already in the
			// Reference array, then add that to the Localization component or the Language Source
			if (HasObjectInReferences(NewObj, localizeCmp))
				return;

			if (localizeCmp!=null)
			{
				localizeCmp.AddTranslatedObject(NewObj);
				EditorUtility.SetDirty(localizeCmp);
			}
			else
			if (mLanguageSource!=null)
			{
				mLanguageSource.AddAsset(NewObj);
                mLanguageSource.Editor_SetDirty();
			}
		}

		static void Translate ( string Key, ref TermData termdata, string TargetLanguageCode, GoogleTranslation.fnOnTranslated onTranslated, string overrideSpecialization )
		{
			#if UNITY_WEBPLAYER
			ShowError ("Contacting google translation is not yet supported on WebPlayer" );
			#else

			if (!GoogleTranslation.CanTranslate())
			{
				ShowError ("WebService is not set correctly or needs to be reinstalled");
				return;
			}

			// Translate first language that has something
			// If no language found, translation will fallback to autodetect language from key

			string sourceCode, sourceText;
			FindTranslationSource( Key, termdata, TargetLanguageCode, overrideSpecialization, out sourceText, out sourceCode );
			GoogleTranslation.Translate( sourceText, sourceCode, TargetLanguageCode, onTranslated );
			
			#endif
		}

		static void FindTranslationSource( string Key, TermData termdata, string TargetLanguageCode, string forceSpecialization, out string sourceText, out string sourceLanguageCode )
		{
			sourceLanguageCode = "auto";
			sourceText = Key;
			
            for (int i = 0, imax = termdata.Languages.Length; i < imax; ++i)
            {
                if (mLanguageSource.mLanguages[i].IsEnabled() && !string.IsNullOrEmpty(termdata.Languages[i]))
                {
                    sourceText = forceSpecialization==null ? termdata.Languages[i] : termdata.GetTranslation(i, forceSpecialization, editMode:true);
                    if (mLanguageSource.mLanguages[i].Code != TargetLanguageCode)
                    {
                        sourceLanguageCode = mLanguageSource.mLanguages[i].Code;
                        return;
                    }
                }
            }
		}
		
		#endregion
	}
}
