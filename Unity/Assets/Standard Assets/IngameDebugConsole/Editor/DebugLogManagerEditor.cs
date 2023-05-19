using UnityEditor;

namespace IngameDebugConsole
{
	[CustomEditor( typeof( DebugLogManager ) )]
	public class DebugLogManagerEditor : Editor
	{
		private SerializedProperty singleton;
		private SerializedProperty minimumHeight;
		private SerializedProperty enablePopup;
		private SerializedProperty startInPopupMode;
		private SerializedProperty startMinimized;
		private SerializedProperty toggleWithKey;
		private SerializedProperty toggleKey;
		private SerializedProperty enableSearchbar;
		private SerializedProperty topSearchbarMinWidth;
		private SerializedProperty clearCommandAfterExecution;
		private SerializedProperty commandHistorySize;
		private SerializedProperty showCommandSuggestions;
		private SerializedProperty receiveLogcatLogsInAndroid;
		private SerializedProperty logcatArguments;

		private void OnEnable()
		{
			singleton = serializedObject.FindProperty( "singleton" );
			minimumHeight = serializedObject.FindProperty( "minimumHeight" );
			enablePopup = serializedObject.FindProperty( "enablePopup" );
			startInPopupMode = serializedObject.FindProperty( "startInPopupMode" );
			startMinimized = serializedObject.FindProperty( "startMinimized" );
			toggleWithKey = serializedObject.FindProperty( "toggleWithKey" );
			toggleKey = serializedObject.FindProperty( "toggleKey" );
			enableSearchbar = serializedObject.FindProperty( "enableSearchbar" );
			topSearchbarMinWidth = serializedObject.FindProperty( "topSearchbarMinWidth" );
			clearCommandAfterExecution = serializedObject.FindProperty( "clearCommandAfterExecution" );
			commandHistorySize = serializedObject.FindProperty( "commandHistorySize" );
			showCommandSuggestions = serializedObject.FindProperty( "showCommandSuggestions" );
			receiveLogcatLogsInAndroid = serializedObject.FindProperty( "receiveLogcatLogsInAndroid" );
			logcatArguments = serializedObject.FindProperty( "logcatArguments" );
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField( singleton );
			EditorGUILayout.PropertyField( minimumHeight );
			EditorGUILayout.PropertyField( enablePopup );

			if( enablePopup.boolValue )
				DrawSubProperty( startInPopupMode );
			else
				DrawSubProperty( startMinimized );

			EditorGUILayout.PropertyField( toggleWithKey );

			if( toggleWithKey.boolValue )
				DrawSubProperty( toggleKey );

			EditorGUILayout.PropertyField( enableSearchbar );

			if( enableSearchbar.boolValue )
				DrawSubProperty( topSearchbarMinWidth );

			EditorGUILayout.PropertyField( clearCommandAfterExecution );
			EditorGUILayout.PropertyField( commandHistorySize );
			EditorGUILayout.PropertyField( showCommandSuggestions );
			EditorGUILayout.PropertyField( receiveLogcatLogsInAndroid );

			if( receiveLogcatLogsInAndroid.boolValue )
				DrawSubProperty( logcatArguments );

			DrawPropertiesExcluding( serializedObject, "m_Script" );
			serializedObject.ApplyModifiedProperties();
		}

		private void DrawSubProperty( SerializedProperty property )
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField( property );
			EditorGUI.indentLevel--;
		}
	}
}