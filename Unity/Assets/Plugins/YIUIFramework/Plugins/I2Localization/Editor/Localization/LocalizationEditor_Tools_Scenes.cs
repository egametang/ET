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
		EditorBuildSettingsScene[] mScenesInBuildSettings;
		bool Tools_ShowScenesList;
		#endregion

		#region GUI

		void OnGUI_ScenesList( bool SmallSize = false )
		{
			mScenesInBuildSettings = EditorBuildSettings.scenes;
			string currentScene = Editor_GetCurrentScene ();

			List<string> sceneList = mScenesInBuildSettings.Select(x=>x.path).ToList();
			if (!sceneList.Contains (currentScene))
					sceneList.Insert (0, currentScene);

			mSelectedScenes.RemoveAll (x => !sceneList.Contains(x));
			if (mSelectedScenes.Count==0)
				mSelectedScenes.Add (currentScene);

			if (!Tools_ShowScenesList)
			{
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
					Tools_ShowScenesList = GUILayout.Toggle(Tools_ShowScenesList, "", EditorStyles.foldout, GUILayout.ExpandWidth(false));

					string sceneText = string.Empty;
					if (mSelectedScenes.Count==1 && mSelectedScenes[0]== currentScene)
						sceneText = "Current Scene";
					else
						sceneText = string.Format("{0} of {1} Scenes", mSelectedScenes.Count, Mathf.Max(mScenesInBuildSettings.Length, mSelectedScenes.Count));
					var stl = new GUIStyle("toolbarbutton");
					stl.richText = true;
					if (GUILayout.Button("Scenes to Parse: <i>"+sceneText+"</i>", stl))
						Tools_ShowScenesList = true;
				GUILayout.EndHorizontal();
				GUILayout.Space(10);
				return;
			}
			OnGUI_ScenesList_TitleBar();

            GUI.backgroundColor = Color.Lerp(GUITools.LightGray, Color.white, 0.5f);
            mScrollPos_BuildScenes = GUILayout.BeginScrollView( mScrollPos_BuildScenes, LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height ( SmallSize ? 100 : 200));
            GUI.backgroundColor = Color.white;

            for (int i=0, imax=sceneList.Count; i<imax; ++i)
			{
				GUILayout.BeginHorizontal();
				
					OnGUI_SelectableToogleListItem( sceneList[i], ref mSelectedScenes, "OL Toggle" );
					
					bool bSelected = mSelectedScenes.Contains(sceneList[i]);
					GUI.color = bSelected ? Color.white : Color.Lerp(Color.gray, Color.white, 0.5f);

					string scenePath = sceneList[i];
					if (scenePath.StartsWith("assets/", StringComparison.OrdinalIgnoreCase))
						scenePath = scenePath.Substring("Assets/".Length);

					if (currentScene == sceneList[i])
						scenePath = "[Current Scene] " + scenePath;

					if (GUILayout.Button (scenePath, "Label"))
					{
						if (mSelectedScenes.Contains(sceneList[i]))
							mSelectedScenes.Remove(sceneList[i]);
						else
							mSelectedScenes.Add(sceneList[i]);
					}
					GUI.color = Color.white;
				
				GUILayout.EndHorizontal();
			}
			
			GUILayout.EndScrollView();
		}

		void OnGUI_ScenesList_TitleBar()
		{
			GUILayout.BeginHorizontal();
				Tools_ShowScenesList = GUILayout.Toggle(Tools_ShowScenesList, "", EditorStyles.foldout, GUILayout.ExpandWidth(false));

				if (GUILayout.Button("Scenes to Parse:", "toolbarbutton"))
					Tools_ShowScenesList = false;

				if (GUILayout.Button("All", "toolbarbutton", GUILayout.ExpandWidth(false)))
                {
                    OnGUI_ScenesList_SelectAllScenes(false);
                }
                if (GUILayout.Button("None", "toolbarbutton", GUILayout.ExpandWidth(false)))
                {
                    mSelectedScenes.Clear();
                }
				if (GUILayout.Button("Used", "toolbarbutton", GUILayout.ExpandWidth(false)))  
				{ 
					mSelectedScenes.Clear(); 
					for (int i=0, imax=mScenesInBuildSettings.Length; i<imax; ++i)
						if (mScenesInBuildSettings[i].enabled)
							mSelectedScenes.Add (mScenesInBuildSettings[i].path);
				}
				if (GUILayout.Button("Current", "toolbarbutton", GUILayout.ExpandWidth(false)))  
				{ 
					mSelectedScenes.Clear(); 
					mSelectedScenes.Add (Editor_GetCurrentScene());
				}
			GUILayout.EndHorizontal();
		}

 
        private void OnGUI_ScenesList_SelectAllScenes(bool reset)
        {
            if (reset || mScenesInBuildSettings == null)
            {
                mScenesInBuildSettings = EditorBuildSettings.scenes;
            }

            mSelectedScenes.Clear();
            for (int i = 0, imax = mScenesInBuildSettings.Length; i < imax; ++i)
                mSelectedScenes.Add(mScenesInBuildSettings[i].path);
            if (!mSelectedScenes.Contains(Editor_GetCurrentScene()))
                mSelectedScenes.Add(Editor_GetCurrentScene());
        }

        void SelectUsedScenes()
		{
			mSelectedScenes.Clear();
			for (int i=0, imax=mScenesInBuildSettings.Length; i<imax; ++i)
				if (mScenesInBuildSettings[i].enabled)
					mSelectedScenes.Add( mScenesInBuildSettings[i].path );
		}
		
		#endregion
	
		#region Iterate thru the Scenes

		delegate void Delegate0();

		static void ExecuteActionOnSelectedScenes( Delegate0 Action )
		{
			string InitialScene = Editor_GetCurrentScene();
			
			if (mSelectedScenes.Count<=0)
				mSelectedScenes.Add (InitialScene);
			
			bool HasSaved = false;
			
			foreach (string ScenePath in mSelectedScenes)
			{
				if (ScenePath != Editor_GetCurrentScene())
				{
					if (!HasSaved)	// Saving the initial scene to avoid loosing changes
					{
						Editor_SaveScene ();
						HasSaved = true;
					}
					Editor_OpenScene( ScenePath );
				}

				Action();
			}

			if (InitialScene != Editor_GetCurrentScene())
				Editor_OpenScene( InitialScene );
			
			if (mLanguageSource!=null)
				Selection.activeObject = mLanguageSource.ownerObject;
		}
		#endregion
	}
}