using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace I2.Loc
{	
	public class GUITools
	{
		static public Color White = Color.white;
		static public Color LightGray = Color.Lerp(Color.gray, Color.white, 0.5f);
		static public Color DarkGray = Color.Lerp(Color.gray, Color.white, 0.2f);
		static public Color LightYellow = Color.Lerp(Color.yellow, Color.white, 0.5f);

        static public GUILayoutOption DontExpandWidth = GUILayout.ExpandWidth(false);
        static public GUIContent EmptyContent = new GUIContent ();

        static List<Action> mDelayedEditorCallbacks = new List<Action>();

        #region Delayed Editor Callback

        public static void DelayedCall( Action action )
        {
            if (mDelayedEditorCallbacks.Count == 0)
                EditorApplication.update += OnDelayedCall;

            mDelayedEditorCallbacks.Add(action);
        }

        static void OnDelayedCall()
        {
            EditorApplication.update -= OnDelayedCall;
            var calls = mDelayedEditorCallbacks.ToArray();
            mDelayedEditorCallbacks.Clear();

            foreach (var call in calls)
                call();
        }


        #endregion

        #region Header
        public delegate void fnOnToggled(bool enabled);
        static public bool DrawHeader (string text, string key, bool ShowToggle=false, bool ToggleState=false, fnOnToggled OnToggle = null, string HelpURL=default(string), Color disabledColor = default(Color))
		{
			bool state = EditorPrefs.GetBool(key, false);

			bool newState = DrawHeader (text, state, ShowToggle, ToggleState, OnToggle, HelpURL, disabledColor);

			if (state!=newState) EditorPrefs.SetBool(key, newState);
			return newState;
		}

		static public bool DrawHeader (string text, bool state, bool ShowToggle=false, bool ToggleState=false, fnOnToggled OnToggle = null, string HelpURL=default(string), Color disabledColor = default(Color), bool allowCollapsing = true)
		{
			GUIStyle Style = new GUIStyle(EditorStyles.foldout);
			Style.richText = true;
			EditorStyles.foldout.richText = true;
			if (state)
			{
				//GUI.backgroundColor=DarkGray;
				GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea/*, GUILayout.Height(1)*/);
				GUILayout.BeginHorizontal();
                if (!string.IsNullOrEmpty(text))
                {
                    if (allowCollapsing)
                        state = GUILayout.Toggle(state, text, Style, GUILayout.ExpandWidth(true));
                    else
                        GUILayout.Label(text, GUILayout.ExpandWidth(true));
                }

				if (!string.IsNullOrEmpty(HelpURL))
				{
					if (GUILayout.Button (Icon_Help, EditorStyles.label, GUILayout.ExpandWidth(false)))
						Application.OpenURL(HelpURL);
				}
				if (ShowToggle)
				{
					GUI.changed = false;
					bool newBool = GUILayout.Toggle(ToggleState, "", "OL Toggle", GUILayout.ExpandWidth(false));
					if (GUI.changed && OnToggle!=null)
						OnToggle(newBool);
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(2);
				
				//GUI.backgroundColor = Color.white;
			}
			else
			{
				if (ShowToggle && !ToggleState) 
					GUI.color = disabledColor;

				GUILayout.BeginHorizontal("Box");
				//GUILayout.BeginHorizontal(EditorStyles.toolbar);
				state = GUILayout.Toggle(state, text, Style, GUILayout.ExpandWidth(true));
				if (ShowToggle)
				{
					GUI.changed = false;
					bool newBool = GUILayout.Toggle(ToggleState, "", "OL Toggle", GUILayout.ExpandWidth(false));
					if (GUI.changed && OnToggle!=null)
						OnToggle(newBool);
				}
				GUILayout.EndHorizontal();
				GUI.color = White;
			}
			return state;
		}

		static public void CloseHeader()
		{
			GUILayout.EndHorizontal();
		}

		public static void OnGUI_Footer(string pluginName, string pluginVersion, string helpURL, string documentationURL, string assetStoreURL)
		{
			GUILayout.BeginHorizontal();
			string versionTip = "";
            /*if (I2Analytics.HasNewVersion(pluginName))
			{
				versionTip = "There is a new version of " + pluginName + ".\nClick here for more details";
				if (GUILayout.Button(new GUIContent("", versionTip), EditorStyles.label, GUILayout.Width(25)))
					I2AboutWindow.DoShowScreen();

				var rect = GUILayoutUtility.GetLastRect();
				rect.yMin = rect.yMax - 25;
				rect.xMax = rect.xMin + 25;
				rect.y += 3;
				GUITools.DrawSkinIcon(rect, "CN EntryWarnIcon", "CN EntryWarn");
			}*/

            if (GUILayout.Button(new GUIContent("v" + pluginVersion, versionTip), EditorStyles.miniLabel))
			{
				Application.OpenURL(assetStoreURL);
				//I2AboutWindow.DoShowScreen();
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Ask a Question", EditorStyles.miniLabel))
				Application.OpenURL(helpURL);

			GUILayout.Space(10);

			if (GUILayout.Button("Documentation", EditorStyles.miniLabel))
				Application.OpenURL(documentationURL);
			GUILayout.EndHorizontal();            
		}


		#endregion

		#region Content
	
		static public void BeginContents ()
		{
			EditorGUILayout.BeginHorizontal(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.MinHeight(10f));
			GUILayout.Space(2f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(2f);
		}
	
		static public void EndContents () { EndContents(true); }
		static public void EndContents ( bool closeHeader )
		{
			GUILayout.Space(2f);
			EditorGUILayout.EndVertical();
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();

			if (closeHeader) CloseHeader();
		}

		#endregion

		#region Tabs

		static public void DrawTabs( SerializedProperty mProperty, GUIStyle Style=null, int height=25 )
		{
			int curIndex = mProperty.enumValueIndex;
			int newIndex = DrawTabs( curIndex, mProperty.enumNames, Style, height);

			if (curIndex!=newIndex)
				mProperty.enumValueIndex = newIndex;
		}

		static public int DrawTabs( int Index, string[] Tabs, GUIStyle Style=null, int height=25, bool expand = true)
		{
			GUIStyle MyStyle = new GUIStyle(Style!=null?Style:GUI.skin.FindStyle("dragtab"));
			MyStyle.fixedHeight=0;

			GUILayout.BeginHorizontal();
			for (int i=0; i<Tabs.Length; ++i)
			{
				int idx = Tabs[i].IndexOf('|');
				if (idx>0)
				{
					string text = Tabs[i].Substring(0, idx);
					string tooltip = Tabs[i].Substring(idx+1);
					if ( GUILayout.Toggle(Index==i, new GUIContent(text, tooltip), MyStyle, GUILayout.Height(height), GUILayout.ExpandWidth(expand)) && Index!=i) 
					{
							Index=i;
							GUI.FocusControl(string.Empty);
					}
				}
				else
				{
					if ( GUILayout.Toggle(Index==i, Tabs[i], MyStyle, GUILayout.Height(height), GUILayout.ExpandWidth(expand)) && Index!=i) 
					{
						Index=i;
						GUI.FocusControl(string.Empty);
					}
				}
			}
			GUILayout.EndHorizontal();
			return Index;
		}

		static public void DrawShadowedTabs( SerializedProperty mProperty, GUIStyle Style=null, int height=25, bool expand=true )
		{
			int curIndex = mProperty.enumValueIndex;
			int newIndex = DrawShadowedTabs( curIndex, mProperty.enumNames, height, expand);

			if (curIndex!=newIndex)
				mProperty.enumValueIndex = newIndex;
		}

		static public int DrawShadowedTabs( int Index, string[] Tabs, int height = 25, bool expand=true )
		{
			GUI.backgroundColor=Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));
				GUI.backgroundColor=Color.white;
				GUILayout.Space(2);
				Index = DrawTabs( Index, Tabs, height: height, expand:expand );
			GUILayout.EndVertical();
			return Index;
		}

        static public int DrawTabs( int Index, Texture2D[] Tabs, GUIStyle Style, int height )
		{
			GUIStyle MyStyle = new GUIStyle(Style!=null?Style:GUI.skin.FindStyle("dragtab"));
			MyStyle.fixedHeight=0;

			//width = Mathf.Max (width, height * Tabs[0].width/(float)Tabs[0].height);

			GUILayout.BeginHorizontal();
			float width = (EditorGUIUtility.currentViewWidth-(MyStyle.border.left+MyStyle.border.right)*(Tabs.Length-1)) / Tabs.Length;
			for (int i=0; i<Tabs.Length; ++i)
			{
				if ( GUILayout.Toggle(Index==i, Tabs[i], MyStyle, GUILayout.Height(height), GUILayout.Width(width)) && Index!=i) 
				{
					Index=i;
					GUI.changed = true;
				}
			}
			GUILayout.EndHorizontal();
			return Index;
		}

		#endregion

		#region Object Array

		static public bool DrawObjectsArray( SerializedProperty PropArray, bool allowDuplicates=false, bool allowResources=false, bool allowSceneObj=false, Object testAdd=null, Object testReplace=null, int testReplaceIndex=-1, int testDeleteIndex=-1 )
		{
            bool hasChanged = false;
			GUILayout.BeginVertical();

				int DeleteElement = -1, MoveUpElement = -1;

				for (int i=0, imax=PropArray.arraySize; i<imax; ++i)
				{
					SerializedProperty Prop = PropArray.GetArrayElementAtIndex(i);
					GUILayout.BeginHorizontal();

						//--[ Delete Button ]-------------------
						if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)) || i == testDeleteIndex)
				    		DeleteElement = i;

						GUILayout.Space(2);
				    	//--[ Object ]--------------------------
						GUILayout.BeginHorizontal(EditorStyles.toolbar);
							GUI.changed = false;
							Object Obj = EditorGUILayout.ObjectField( Prop.objectReferenceValue, typeof(Object), allowSceneObj, GUILayout.ExpandWidth(true));
                            if (testReplaceIndex == i)
                            {
                                Obj = testReplace;
                                GUI.changed = true;
                            }

                            if (!allowResources && Obj != null)
                            {
                                var path = AssetDatabase.GetAssetPath(Obj);
                                if (path != null && path.Contains("/Resources/"))
                                    Obj = null;
                            }

							if (Obj==null)
								DeleteElement = i;
							else
							if (GUI.changed && (allowDuplicates || !ObjectsArrayHasReference(PropArray, Obj)))
                            {
                                Prop.objectReferenceValue = Obj;
                                hasChanged = true;
                            }
						GUILayout.EndHorizontal();

						//--[ MoveUp Button ]-------------------
						if (i==0)
						{
							if (imax>1)
								GUILayout.Space (18);
						}
						else
						{
							if (GUILayout.Button( "\u25B2", EditorStyles.toolbarButton, GUILayout.Width(18)))
								MoveUpElement = i;
						}

					GUILayout.EndHorizontal();
				}

				GUILayout.BeginHorizontal(EditorStyles.toolbar);
					Object NewObj = EditorGUILayout.ObjectField( null, typeof(Object), allowSceneObj, GUILayout.ExpandWidth(true));
                    if (testAdd != null)
                    {
                        NewObj = testAdd;
                    }

                    if (!allowResources && NewObj != null)
                    {
                        var path = AssetDatabase.GetAssetPath(NewObj);
                        if (path != null && path.Contains("/Resources/"))
                    NewObj = null;
                    }
                    if (NewObj && (allowDuplicates || !ObjectsArrayHasReference(PropArray, NewObj)))
					{
						int Index = PropArray.arraySize;
						PropArray.InsertArrayElementAtIndex( Index );
						PropArray.GetArrayElementAtIndex(Index).objectReferenceValue = NewObj;
                        hasChanged = true;
                    }
				GUILayout.EndHorizontal();

				if (DeleteElement>=0)
				{
					PropArray.DeleteArrayElementAtIndex( DeleteElement );
					//PropArray.DeleteArrayElementAtIndex( DeleteElement );
                    hasChanged = true;
				}

				if (MoveUpElement>=0)
                {
					PropArray.MoveArrayElement(MoveUpElement, MoveUpElement-1);
                    hasChanged = true;
                }

            GUILayout.EndVertical();
            return hasChanged;
		}

        static public bool ObjectsArrayHasReference(SerializedProperty PropArray, Object obj)
        {
            for (int i = 0, imax = PropArray.arraySize; i < imax; ++i)
            {
                SerializedProperty Prop = PropArray.GetArrayElementAtIndex(i);
                if (Prop.objectReferenceValue == obj)
                    return true;
            }
            return false;
        }


        #endregion

        #region Toggle

        static public int ToggleToolbar( int Index, string[] Options )
		{
			GUILayout.BeginHorizontal();
			for (int i=0; i<Options.Length; ++i)
			{
				if ( GUILayout.Toggle(Index==i, Options[i], EditorStyles.toolbarButton)) 
					Index=i;
			}
			GUILayout.EndHorizontal();
			return Index;
		}

		static public void ToggleToolbar( SerializedProperty EnumVar )
		{
			int index = ToggleToolbar( EnumVar.enumValueIndex, EnumVar.enumNames);
			if (EnumVar.enumValueIndex != index)
				EnumVar.enumValueIndex = index;
		}

		#endregion

		#region Misc
		
		public static bool ObjectExistInScene( GameObject Obj )
		{
            return Obj.scene.IsValid() && Obj.scene.isLoaded;
			/* //if (Obj.transform.root != Obj.transform)
			//	continue;
			
			// We are only interested in GameObjects that are visible in the Hierachy panel and are persitent 
			if ((Obj.hideFlags & (HideFlags.DontSave|HideFlags.HideInHierarchy)) > 0)
				return false;
			
			// We are not interested in Prefab, unless they are Prefab Instances
			PrefabType pfType = PrefabUtility.GetPrefabType(Obj);
			if(pfType == PrefabType.Prefab || pfType == PrefabType.ModelPrefab)
				return false;
			
			// If the database contains the object then its not an scene object, 
			// but the previous test should get rid of them, so I will just comment this 
			// unless an false positive object is found in the future
			//if (AssetDatabase.Contains(Obj))
			//		return false;
			
			return true;*/
		}

		public static IEnumerable<GameObject> SceneRoots()
		{
			var prop = new HierarchyProperty(HierarchyType.GameObjects);
			var expanded = new int[0];
			while (prop.Next(expanded)) {
				yield return prop.pptrValue as GameObject;
			}
		}
		
		public static List<GameObject> SceneRootsList()
		{
			return new List<GameObject>(SceneRoots());
		}
		
		public static IEnumerable<Transform> AllSceneObjects()
		{
			var queue = new Queue<Transform>();
			
			foreach (var root in SceneRoots()) {
				var tf = root.transform;
				yield return tf;
				queue.Enqueue(tf);
			}
			
			while (queue.Count > 0) {
				foreach (Transform child in queue.Dequeue()) {
					yield return child;
					queue.Enqueue(child);
				}
			}
		}

		public static string GetScenePath(Transform tr)
		{
			if (tr==null)
				return string.Empty;
			
			string path = tr.name;
			while (tr.parent != null)
			{
				tr = tr.parent;
				path = tr.name + "/" + path;
			}
			return path;
		}

		public static Transform FindObjectInEditor( string scenePath )
		{
			if (string.IsNullOrEmpty(scenePath))
				return null;

			int index = scenePath.IndexOfAny("/\\".ToCharArray());
			string first = index<0 ? scenePath : scenePath.Substring(0, index);

			foreach (var root in AllSceneObjects())
				if (root.name==first)
				{
					if (index<0) 
						return root;

					return root.Find(scenePath.Substring(index+1));
				}
			return null;
		}


		public static GUIContent Icon_Help { 
			get{
				if (mIconHelp == null)
					mIconHelp = EditorGUIUtility.IconContent("_Help");
				return mIconHelp;
			}
		}
		static GUIContent mIconHelp;

        public static GUIStyle FindSkinStyle(string name)
        {
            var allStyles = GUI.skin.customStyles;
            for (int i = 0, imax = allStyles.Length; i < imax; ++i)
            {
                if (allStyles[i].name == name)
                    return allStyles[i];
            }
            return null;
        }
        public static void DrawSkinIcon(Rect rect, params string[] iconNames)
        {
            foreach (var icon in iconNames)
            {
                var style = FindSkinStyle(icon);
                if (style == null || style.normal == null || style.normal.background == null)
                    continue;

                GUI.DrawTexture(rect, style.normal.background);
                return;
            }
            //Debug.Log("unable to find icon");
        }

        #endregion

        #region Angle Drawer
        private static Vector2 mAngle_lastMousePosition;
		static Texture mAngle_TextureCircle;
		static Texture pAngle_TextureCircle { 
			get{ 
				if (mAngle_TextureCircle) return mAngle_TextureCircle;  
				mAngle_TextureCircle = GUI.skin.GetStyle("CN CountBadge").normal.background;
				return mAngle_TextureCircle;
			}
		}
		
		public static float FloatAngle(Rect rect, float value)
		{
			return FloatAngle(rect, value, -1, -1, -1);
		}
		
		public static float FloatAngle(Rect rect, float value, float snap)
		{
			return FloatAngle(rect, value, snap, -1, -1);
		}
		
		public static float FloatAngle(Rect rect, float value, float snap, float min, float max)
		{
			int id = GUIUtility.GetControlID(FocusType.Passive, rect);
			
			Rect knobRect = new Rect(rect.x, rect.y, rect.height, rect.height);
			
			float delta;
			if (min != max)
				delta = (max - min) / 360;
			else
				delta = 1;
			
			if (Event.current != null)
			{
				if (Event.current.type == EventType.MouseDown && knobRect.Contains(Event.current.mousePosition))
				{
					GUIUtility.hotControl = id;
					mAngle_lastMousePosition = Event.current.mousePosition;
				}
				else if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == id)
					GUIUtility.hotControl = -1;
				else if (Event.current.type == EventType.MouseDrag && GUIUtility.hotControl == id)
				{
					Vector2 move = mAngle_lastMousePosition - Event.current.mousePosition;
					value += delta * (-move.x - move.y);
					
					if (snap > 0)
					{
						float mod = value % snap;
						
						if (mod < delta * 3 || Mathf.Abs(mod - snap) < delta * 3)
							value = Mathf.Round(value / snap) * snap;
					}
					
					mAngle_lastMousePosition = Event.current.mousePosition;
					GUI.changed = true;
				}
			}

			if (pAngle_TextureCircle) GUI.DrawTexture(knobRect, pAngle_TextureCircle);
			Matrix4x4 matrix = GUI.matrix;
			
			if (min != max)
				GUIUtility.RotateAroundPivot(value * (360 / (max - min)), knobRect.center);
			else
				GUIUtility.RotateAroundPivot(value, knobRect.center);

			knobRect.height = 5;
			knobRect.width = 5;
			if (pAngle_TextureCircle) GUI.DrawTexture(knobRect, pAngle_TextureCircle);
			GUI.matrix = matrix;
			
			Rect label = new Rect(rect.x + rect.height, rect.y + rect.height / 2 - 9, rect.height, 18);
			value = EditorGUI.FloatField(label, value);
			
			if (min != max)
				value = Mathf.Clamp(value, min, max);
			
			return value;
		}

		public static float AngleCircle(Rect rect, float angle, float snap, float min, float max, Texture background=null, Texture knobLine=null)
		{
			Rect knobRect = new Rect(rect.x, rect.y, rect.height, rect.height);
			
			float delta;
			if (min != max)
				delta = (max - min) / 360;
			else
				delta = 1;

			if (Event.current != null && GUIUtility.hotControl<=0 &&  (Event.current.type==EventType.MouseDown || Event.current.type==EventType.MouseDrag) && knobRect.Contains(Event.current.mousePosition))
			{
				angle = Vector2.Angle( Vector2.right, Event.current.mousePosition-knobRect.center);
				if (Event.current.mousePosition.y<knobRect.center.y) angle = 360-angle;
				if (Event.current.alt || Event.current.control)
					snap = 5;
				if (snap > 0)
				{
					float mod = Mathf.Repeat(angle, snap);
					
					if (mod < delta * 3 || Mathf.Abs(mod - snap) < delta * 3)
						angle = Mathf.Round(angle / snap) * snap;
				}
				
				GUI.changed = true;
			}

			if (background==null) background = pAngle_TextureCircle;
			if (background) GUI.DrawTexture (knobRect, background);

			Matrix4x4 matrix = GUI.matrix;
			
			if (min != max)
				GUIUtility.RotateAroundPivot(angle * (360 / (max - min))+90, knobRect.center);
			else
				GUIUtility.RotateAroundPivot(angle+90, knobRect.center);

			float Radius = Mathf.Min (knobRect.width, knobRect.height) * 0.5f;
			knobRect.x = knobRect.x + 0.5f * knobRect.width - 4;
			knobRect.y += 2;
			knobRect.width = 8;
			knobRect.height = Radius+2;
			if (knobLine == null)
				knobLine = GUI.skin.FindStyle ("MeBlendPosition").normal.background;
			if (knobLine) GUI.DrawTexture(knobRect, knobLine);
			GUI.matrix = matrix;
			
			return Mathf.Repeat(angle, 360);
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

        public static void Editor_SaveScene()
		{
			#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			EditorApplication.SaveScene ();
			#else
			EditorSceneManager.SaveOpenScenes();
			#endif
		}

		public static void Editor_OpenScene( string sceneName )
		{
			#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			EditorApplication.OpenScene( sceneName );
			#else
			EditorSceneManager.OpenScene(sceneName);
			#endif
		}

		#endregion

		#region Reflection
		static public object Reflection_InvokeMethod ( object instanceObject, string methodName, params object[] p_args )
		{
			BindingFlags _flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
			MethodInfo mi = instanceObject.GetType().GetMethods( _flags ).Where( x => x.Name==methodName ).FirstOrDefault();
			if (mi == null) return null;
			return mi.Invoke( instanceObject, p_args );
		}
		static public object Reflection_InvokeMethod ( Type targetType, string methodName, params object[] p_args )
		{
			BindingFlags _flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
			MethodInfo mi = targetType.GetMethods( _flags ).Where( x => x.Name==methodName ).FirstOrDefault();
			if (mi == null) return null;
			return mi.Invoke( null, p_args );
		}


        public static object s_RecycledEditor;
        public static string TextField ( Rect position, string text, int maxLength, GUIStyle style, int controlID )
		{
            if (s_RecycledEditor==null)
            {
                FieldInfo info = typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static);
                s_RecycledEditor = info.GetValue(null);
            }

            if (s_RecycledEditor == null)
                return "";

            return Reflection_InvokeMethod( typeof( EditorGUI ), "DoTextField", s_RecycledEditor, controlID, position, text, style, null, false, false, false, false ) as string;
		}

        static public void RepaintInspectors()
        {
            EditorApplication.update -= RepaintInspectors;
            var assemblyEditor = Assembly.GetAssembly(typeof(Editor));
            var typeInspectorWindow = assemblyEditor.GetType("UnityEditor.InspectorWindow");
            typeInspectorWindow.GetMethod("RepaintAllInspectors", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        }

        public static void ScheduleRepaintInspectors()
        {
            EditorApplication.update += RepaintInspectors;
        }


        #endregion
    }
}
