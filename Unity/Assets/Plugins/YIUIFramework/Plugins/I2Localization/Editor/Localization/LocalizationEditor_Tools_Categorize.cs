using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables
		
		Vector2 mScrollPos_CategorizedKeys = Vector2.zero;
		string mNewCategory = string.Empty;

		#endregion

		#region GUI

		void OnGUI_Tools_Categorize()
		{
			OnGUI_ScenesList(true);
			
			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));
				GUI.backgroundColor = Color.white;
				GUILayout.Space (5);
				
				EditorGUILayout.HelpBox("此工具更改所选术语的类别并更新高亮显示的场景。\nThis tool changes the category of the selected Terms and updates the highlighted scenes", MessageType.Info);

				GUILayout.Space (5);
			GUITools.CloseHeader();

			OnGUI_Tools_Categorize_Terms();
			OnGUI_NewOrExistingCategory();
		}

		void OnGUI_Tools_Categorize_Terms()
		{
			GUILayout.Label("Change Category of the following Terms:", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));

            GUI.backgroundColor = Color.Lerp(GUITools.LightGray, Color.white, 0.5f);
            mScrollPos_CategorizedKeys = GUILayout.BeginScrollView( mScrollPos_CategorizedKeys, LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height ( 100));
            GUI.backgroundColor = Color.white;

            if (mSelectedKeys.Count==0)
			{
				GUILayout.FlexibleSpace();

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
						//GUILayout.BeginVertical();
							EditorGUILayout.HelpBox("没有选择任何条款 No Terms has been selected", MessageType.Warning);
							/*if (GUILayout.Button("Select Terms", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true))) 
								mCurrentViewMode = eViewMode.Keys;*/
						//GUILayout.EndVertical();
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				
				GUILayout.FlexibleSpace();
			}
			else
			{
				bool DoubleColumn = mSelectedKeys.Count>5;
				int HalfCount = Mathf.CeilToInt(mSelectedKeys.Count/2.0f);

				for (int i=0, imax=mSelectedKeys.Count; i<imax; ++i)
				{
					if (DoubleColumn && i>=HalfCount) break;

					GUILayout.BeginHorizontal();
						OnGUI_CategorizedTerm(mSelectedKeys[i]);

						if (DoubleColumn && i+HalfCount<mSelectedKeys.Count)
							OnGUI_CategorizedTerm(mSelectedKeys[i+HalfCount]);
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndScrollView();
		}

		void OnGUI_CategorizedTerm( string Term )
		{
			GUILayout.BeginHorizontal();
			string sKey, sCategory;
			LanguageSourceData.DeserializeFullTerm(Term, out sKey, out sCategory);
			if (!string.IsNullOrEmpty(sCategory))
			{
				GUI.color = Color.gray;
				GUILayout.Label(sCategory+"/");
				GUI.color = Color.white;
			}
			GUILayout.Label(sKey);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		void OnGUI_NewOrExistingCategory()
		{
			//--[ Create Category ]------------------------
			GUILayout.BeginHorizontal();
				mNewCategory = GUILayout.TextField(mNewCategory, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Create", "toolbarbutton", GUILayout.Width(60)))
				{
					EditorApplication.update += AssignCategoryToSelectedTerms;
				}
			GUILayout.EndHorizontal();

			//--[ Existing Category ]------------------------
			int Index = 0;
			List<string> Categories = LocalizationManager.GetCategories();

			for (int i=0, imax=Categories.Count; i<imax; ++i)
				if (Categories[i].ToLower().Contains(mNewCategory.ToLower()))
				{
					Index = i;
					break;
				}

			GUILayout.BeginHorizontal();
				int NewIndex = EditorGUILayout.Popup(Index, Categories.ToArray(), EditorStyles.toolbarPopup, GUILayout.ExpandWidth(true));
				if (NewIndex!=Index)
					mNewCategory = Categories[ NewIndex ];
				if (GUILayout.Button("Use", "toolbarbutton", GUILayout.Width(60)))
				{
					mNewCategory = Categories[ NewIndex ];
					EditorApplication.update += AssignCategoryToSelectedTerms;
				}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region Assigning Category

		public static Dictionary<string, string> TermReplacements;

		void AssignCategoryToSelectedTerms()
		{
			mIsParsing = true;
			
			EditorApplication.update -= AssignCategoryToSelectedTerms;

			mNewCategory = mNewCategory.Trim (LanguageSourceData.CategorySeparators);

			if (mNewCategory==LanguageSourceData.EmptyCategory)
				mNewCategory = string.Empty;

			TermReplacements = new Dictionary<string, string>(StringComparer.Ordinal);
			for (int i=mSelectedKeys.Count-1; i>=0; --i)
			{
				string sKey, sCategory;
				string OldTerm = mSelectedKeys[i];

				LanguageSourceData.DeserializeFullTerm( OldTerm, out sKey, out sCategory );
				if (!string.IsNullOrEmpty(mNewCategory))
					sKey = string.Concat(mNewCategory, "/", sKey);

				if (OldTerm == sKey)
					continue;

				TermReplacements[ OldTerm ] = sKey;
				if (!mLanguageSource.ContainsTerm(sKey))
				{
					TermData termData = mLanguageSource.GetTermData( OldTerm );
					if (termData != null)
						termData.Term = sKey;
					else
						TermReplacements.Remove (OldTerm);
                    mLanguageSource.Editor_SetDirty();
				}
			}
			if (TermReplacements.Count<=0)
			{
				ShowError ("Unable to assign category: Terms were not found in the selected LanguageSource");
			}
			else
			{
				mLanguageSource.UpdateDictionary(true);
				ExecuteActionOnSelectedScenes( ReplaceTermsInCurrentScene );
				ParseTerms(true, false, true);

                if (string.IsNullOrEmpty(mNewCategory)) 
					mNewCategory = LanguageSourceData.EmptyCategory;
				if (!mSelectedCategories.Contains(mNewCategory))
					mSelectedCategories.Add (mNewCategory);
                //RemoveUnusedCategoriesFromSelected();
                ScheduleUpdateTermsToShowInList();
            }
            TermReplacements = null;
			mIsParsing = false;
		}

		public static void ReplaceTermsInCurrentScene()
		{
			Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll(typeof(Localize));
			
			if (Locals==null)
				return;

            bool changed = false;
			for (int i=0, imax=Locals.Length; i<imax; ++i)
			{
				Localize localize = Locals[i];
				if (localize==null || localize.gameObject==null || !GUITools.ObjectExistInScene(localize.gameObject))
					continue;

				string NewTerm;
                if (TermReplacements.TryGetValue(localize.Term, out NewTerm))
                {
                    localize.mTerm = NewTerm;
                    changed = true;
                }

                if (TermReplacements.TryGetValue(localize.SecondaryTerm, out NewTerm))
                {
                    localize.mTermSecondary = NewTerm;
                    changed = true;
                }
			}
			if (changed)
				Editor_SaveScene(true);

		}
		#endregion
	}
}