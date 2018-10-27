using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class DisplayOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public static HideFlags hideFlags = HideFlags.None;

		/// <summary>
		/// 
		/// </summary>
		public static void SetEditModeHideFlags()
		{
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
	#if SHOW_HIERARCHY_EDIT_MODE
			hideFlags = HideFlags.DontSaveInEditor;
	#else
			hideFlags = HideFlags.DontSaveInEditor;
	#endif
#else
			hideFlags = HideFlags.DontSave;
#endif
		}
	}
}
