using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables
		
		#endregion
		
		#region GUI
		
		void OnGUI_Tools_MergeTerms()
		{
			OnGUI_ScenesList(true);
			
			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));
			GUI.backgroundColor = Color.white;
			GUILayout.Space (5);
			
			EditorGUILayout.HelpBox("此选项替换所选场景中该键的所有出现\nThis option replace all occurrences of this key in the selected scenes", MessageType.Info);
			
			GUILayout.Space (5);
			GUITools.CloseHeader();
			
			OnGUI_Tools_Categorize_Terms();
			OnGUI_NewOrExistingTerm();
		}
		
		void OnGUI_NewOrExistingTerm()
		{
			if (mKeyToExplore==null)
				mKeyToExplore = string.Empty;

			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));
			GUI.backgroundColor = Color.white;
				GUILayout.Space(5);
				GUILayout.Label("Replace By:");
			GUILayout.EndVertical();

			//--[ Create Term ]------------------------
			GUILayout.BeginHorizontal();
				mKeyToExplore = GUILayout.TextField(mKeyToExplore, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Create", "toolbarbutton", GUILayout.Width(60)))
				{
					LanguageSourceData.ValidateFullTerm( ref mKeyToExplore );
					EditorApplication.update += ReplaceSelectedTerms;
				}
			GUILayout.EndHorizontal();
			
			//--[ Existing Term ]------------------------
			int Index = 0;
			List<string> Terms = mLanguageSource.GetTermsList();

			for (int i=0, imax=Terms.Count; i<imax; ++i)
				if (Terms[i].ToLower().Contains(mKeyToExplore.ToLower()))
				{
					Index = i;
					break;
				}
			
			GUILayout.BeginHorizontal();
			int NewIndex = EditorGUILayout.Popup(Index, Terms.ToArray(), EditorStyles.toolbarPopup, GUILayout.ExpandWidth(true));
			if (NewIndex != Index) 
			{
				SelectTerm (Terms [NewIndex]);
				ClearErrors();
			}
			if (GUILayout.Button("Use", "toolbarbutton", GUILayout.Width(60)))
			{
				SelectTerm( Terms[ NewIndex ] );
				EditorApplication.update += ReplaceSelectedTerms;
			}
			GUILayout.EndHorizontal();
		}
		
		#endregion
		
		#region Merge Terms
		
		void ReplaceSelectedTerms()
		{
			EditorApplication.update -= ReplaceSelectedTerms;
			if (string.IsNullOrEmpty(mKeyToExplore))
				return;

			mIsParsing = true;
			string sNewKey = mKeyToExplore;

			//--[ Create new Term ]-----------------------
			if (mLanguageSource.GetTermData(sNewKey)==null)
			{
				TermData termData = AddLocalTerm(sNewKey);

				//--[ Copy the values from any existing term if the target is a new term
				TermData oldTerm = null;
				for (int i=0, imax=mSelectedKeys.Count; i<imax; ++i)
				{
					oldTerm = mLanguageSource.GetTermData(mSelectedKeys[i]);
					if (oldTerm!=null) break;
				}

				if (oldTerm!=null)
				{
					termData.TermType 		= oldTerm.TermType;
					termData.Description	= oldTerm.Description;
					Array.Copy(oldTerm.Languages, termData.Languages, oldTerm.Languages.Length);
				}
			}

			//--[ Delete the selected Terms from the source ]-----------------
            TermReplacements = new Dictionary<string, string>(StringComparer.Ordinal);
			for (int i=mSelectedKeys.Count-1; i>=0; --i)
			{
				string OldTerm = mSelectedKeys[i];
				if (OldTerm == sNewKey)
					continue;
				
				TermReplacements[ OldTerm ] = mKeyToExplore;
				DeleteTerm(OldTerm);
			}
			ExecuteActionOnSelectedScenes( ReplaceTermsInCurrentScene );
			DoParseTermsInCurrentScene();

			//--[ Update Selected Categories ]-------------
			string mNewCategory = LanguageSourceData.GetCategoryFromFullTerm(sNewKey);
			if (mNewCategory == string.Empty)
				mNewCategory = LanguageSourceData.EmptyCategory;
			if (!mSelectedCategories.Contains(mNewCategory))
				mSelectedCategories.Add (mNewCategory);
			//RemoveUnusedCategoriesFromSelected();
			ScheduleUpdateTermsToShowInList();
			TermReplacements = null;
			mIsParsing = false;
		}

		void RemoveUnusedCategoriesFromSelected()
		{
			List<string> Categories = LocalizationManager.GetCategories();
			for (int i=mSelectedCategories.Count-1; i>=0; --i)
				if (!Categories.Contains( mSelectedCategories[i] ))
					mSelectedCategories.RemoveAt(i);

			if (mSelectedCategories.Count == 0)
				mSelectedCategories.AddRange(Categories);

			ScheduleUpdateTermsToShowInList();
		}

		#endregion
	}
}