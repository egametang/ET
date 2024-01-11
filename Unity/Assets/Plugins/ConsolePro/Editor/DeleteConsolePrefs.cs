using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DeletePrefs : MonoBehaviour
{
	[MenuItem("Tools/Delete Console Pro Toggle Prefs")]
	public static void DeleteConsolePrefs()
	{
		if(EditorPrefs.HasKey("ConsolePro3ToggleDict"))
		{
			Debug.Log("Current Console Pro Toggle Prefs: " + EditorPrefs.GetString("ConsolePro3ToggleDict"));
			Debug.Log("Deleting Console Pro Toggle Prefs...");
			EditorPrefs.DeleteKey("ConsolePro3ToggleDict");
		}
	}
}
