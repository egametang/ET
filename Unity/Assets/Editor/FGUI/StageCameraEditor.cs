using UnityEngine;
using UnityEditor;
using FairyGUI;

namespace FairyGUIEditor
{
	/// <summary>
	/// 
	/// </summary>
	[CustomEditor(typeof(StageCamera))]
	public class StageCameraEditor : Editor
	{
		string[] propertyToExclude;

		void OnEnable()
		{
			propertyToExclude = new string[] { "m_Script" };
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawPropertiesExcluding(serializedObject, propertyToExclude);

			if (serializedObject.ApplyModifiedProperties())
				(target as StageCamera).ApplyModifiedProperties();
		}
	}
}
