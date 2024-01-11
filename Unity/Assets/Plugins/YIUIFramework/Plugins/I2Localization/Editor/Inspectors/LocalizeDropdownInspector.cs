using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace I2.Loc
{
	#if !UNITY_5_0 && !UNITY_5_1

	[CustomEditor(typeof(LocalizeDropdown))]
	public class LocalizeDropdownInspector : Editor
	{
		private ReorderableList mList;

		private List<string> terms;

		private ReorderableList getList(SerializedObject serObject)
		{
			if (mList == null) {
				mList = new ReorderableList (serObject, serObject.FindProperty ("_Terms"), true, true, true, true);
				mList.drawElementCallback = drawElementCallback;
				mList.drawHeaderCallback = drawHeaderCallback;
				mList.onAddCallback = addElementCallback;
				mList.onRemoveCallback = removeElementCallback;
			} 
			else
			{
				mList.serializedProperty = serObject.FindProperty ("_Terms");
			}
			return mList;
		}

		private void addElementCallback( ReorderableList list )
		{
			serializedObject.ApplyModifiedProperties();

			var objParams = target as LocalizeDropdown;
			objParams._Terms.Add(string.Empty);

			list.index = objParams._Terms.Count - 1;

			serializedObject.Update();
		}

		private void removeElementCallback( ReorderableList list )
		{
			if (list.index < 0)
				return;
			serializedObject.ApplyModifiedProperties();

			var objParams = target as LocalizeDropdown;
			objParams._Terms.RemoveAt(list.index);

			serializedObject.Update();
		}

		private void drawHeaderCallback(Rect rect)
		{
			GUI.Label(rect, "Terms:");
		}

		private void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			var serializedElement = mList.serializedProperty.GetArrayElementAtIndex (index);

			EditorGUI.BeginChangeCheck ();

			var prvIndex =  serializedElement.stringValue == "-" || serializedElement.stringValue == "" ? terms.Count - 1 : 
				serializedElement.stringValue == " " ? terms.Count - 2 : 
				terms.IndexOf(serializedElement.stringValue);

			var newIndex = EditorGUI.Popup(rect, prvIndex, terms.ToArray());

			if (EditorGUI.EndChangeCheck ())
			{
				if (newIndex == terms.Count - 1)
					serializedElement.stringValue = "-";
				else
				if (newIndex < 0 || newIndex == terms.Count - 2)
					serializedElement.stringValue = string.Empty;
				else
					serializedElement.stringValue = terms[newIndex];
			}
		}

		void OnEnable()
		{
			mList = getList(serializedObject);
		}

		public override void OnInspectorGUI()
		{
			#if UNITY_5_6_OR_NEWER
				serializedObject.UpdateIfRequiredOrScript();
			#else
				serializedObject.UpdateIfDirtyOrScript();
			#endif
			terms =  LocalizationManager.GetTermsList ();
			terms.Sort(StringComparer.OrdinalIgnoreCase);
			terms.Add("");
			terms.Add("<inferred from text>");
			terms.Add("<none>");

			GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_Background);
			GUI.backgroundColor = Color.white;

			if (GUILayout.Button("Localize DropDown", LocalizeInspector.GUIStyle_Header))
			{
				Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
			}


			GUILayout.Space(5);
			mList.DoLayoutList();

			GUILayout.Space (10);

			GUITools.OnGUI_Footer("I2 Localization", LocalizationManager.GetVersion(), LocalizeInspector.HelpURL_forum, LocalizeInspector.HelpURL_Documentation, LocalizeInspector.HelpURL_AssetStore);

			EditorGUIUtility.labelWidth = 0;


			GUILayout.EndVertical();
			serializedObject.ApplyModifiedProperties();
			terms = null;
		}
	}
	#endif
}