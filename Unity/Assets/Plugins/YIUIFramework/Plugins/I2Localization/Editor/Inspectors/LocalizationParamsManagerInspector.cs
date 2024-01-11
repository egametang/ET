using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace I2.Loc
{
    [CustomEditor(typeof(LocalizationParamsManager))]
	public class LocalizationParamsManagerInspector : Editor
	{
		private ReorderableList mList;
        private SerializedProperty mProp_IsGlobalManager;


        private ReorderableList getList(SerializedObject serObject)
		{
			if (mList == null) {
                mList = new ReorderableList (serObject, serObject.FindProperty ("_Params"), true, true, true, true);
				mList.drawElementCallback = drawElementCallback;
				mList.drawHeaderCallback = drawHeaderCallback;
                mList.onAddCallback = addElementCallback;
                mList.onRemoveCallback = removeElementCallback;
			} 
			else
			{
                mList.serializedProperty = serObject.FindProperty ("_Params");
			}
			return mList;
		}

        private void addElementCallback( ReorderableList list )
        {
            serializedObject.ApplyModifiedProperties();
            var objParams = target as LocalizationParamsManager;
            objParams._Params.Add(new LocalizationParamsManager.ParamValue());
            list.index = objParams._Params.Count - 1;
            serializedObject.Update();
        }

        private void removeElementCallback( ReorderableList list )
        {
            if (list.index < 0)
                return;
            serializedObject.ApplyModifiedProperties();
            var objParams = target as LocalizationParamsManager;
            objParams._Params.RemoveAt(list.index);
            serializedObject.Update();
        }

		private void drawHeaderCallback(Rect rect)
		{
            GUI.Label(rect, "Parameters:");
		}

		private void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			var serializedElement = mList.serializedProperty.GetArrayElementAtIndex (index);
			var content = new GUIContent ();

            Rect r = rect;  r.xMax = r.xMin+40;
            GUI.Label(r, "Name");

            r = rect;	r.xMax = (r.xMax + r.xMin)/2 - 2; r.xMin = r.xMin+40;
			EditorGUI.PropertyField (r, serializedElement.FindPropertyRelative ("Name"),content);

            r = rect;  r.xMin = (r.xMax + r.xMin) / 2 + 2; r.xMax = r.xMin+40;
            GUI.Label(r, "Value");

            r = rect;	r.xMin = (r.xMax + r.xMin)/2 + 2 + 40;
			EditorGUI.PropertyField (r, serializedElement.FindPropertyRelative ("Value"), content);
		}

        void OnEnable()
        {
            mList = getList(serializedObject);
            mProp_IsGlobalManager = serializedObject.FindProperty("_IsGlobalManager");
        }
        public override void OnInspectorGUI()
		{
			#if UNITY_5_6_OR_NEWER
				serializedObject.UpdateIfRequiredOrScript();
			#else
				serializedObject.UpdateIfDirtyOrScript();
			#endif

            GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
            GUILayout.BeginVertical(LocalizeInspector.GUIStyle_Background);
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Dynamic Parameters", LocalizeInspector.GUIStyle_Header))
            {
                Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
            }

            GUILayout.Space(5);
            mProp_IsGlobalManager.boolValue = EditorGUILayout.Popup(new GUIContent("Manager Type", "本地管理器只对同一个游戏对象中的本地化组件应用参数\nLocal Manager only apply parameters to the Localize component in the same GameObject\n\n全局管理器将参数应用于所有本地化组件\nGlobal Manager apply parameters to all Localize components"), mProp_IsGlobalManager.boolValue ? 1 : 0, new[] { new GUIContent("Local"), new GUIContent("Global") }) == 1;


            GUILayout.Space(5);
            mList.DoLayoutList();

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("_AutoRegister"), new GUIContent("Auto Register"));

            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
		}
	}
}