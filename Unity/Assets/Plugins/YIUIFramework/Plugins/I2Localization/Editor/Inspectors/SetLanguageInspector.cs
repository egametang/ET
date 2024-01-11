using System;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	[CustomEditor(typeof(SetLanguage))]
	public class SetLanguageInspector : Editor
	{
		public SetLanguage setLan;
		public SerializedProperty mProp_Language;

		public void OnEnable()
		{
			setLan = (SetLanguage)target;
			mProp_Language = serializedObject.FindProperty("_Language");
		}

		public override void OnInspectorGUI()
		{
			string[] Languages;
			LanguageSource sourceObj = setLan.mSource;
			if (sourceObj == null)
			{
				LocalizationManager.UpdateSources();
				Languages = LocalizationManager.GetAllLanguages().ToArray();
				Array.Sort(Languages);
			}
			else
			{
				Languages = sourceObj.mSource.GetLanguages().ToArray();
				Array.Sort(Languages);
			}

            int index = Array.IndexOf(Languages, mProp_Language.stringValue);

			GUI.changed = false;
			index = EditorGUILayout.Popup("Language", index, Languages);
			if (GUI.changed)
			{
				if (index<0 || index>=Languages.Length)
					mProp_Language.stringValue = string.Empty;
				else
					mProp_Language.stringValue = Languages[index];
				GUI.changed = false;
				serializedObject.ApplyModifiedProperties();
			}

			GUILayout.Space(5);
			if (setLan.mSource==null) GUI.contentColor = Color.Lerp (Color.gray, Color.yellow, 0.1f);
			sourceObj = EditorGUILayout.ObjectField("Language Source:", sourceObj, typeof(LanguageSource), true) as LanguageSource;
			GUI.contentColor = Color.white;

			if (GUI.changed)
				setLan.mSource = sourceObj;

			serializedObject.ApplyModifiedProperties();
		}
	}
}