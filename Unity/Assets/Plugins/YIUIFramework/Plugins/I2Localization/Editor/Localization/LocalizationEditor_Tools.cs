using System.Collections.Generic;
using UnityEngine;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{

		#region Variables
		
		Vector2 mScrollPos_BuildScenes = Vector2.zero;
		
		static List<string> mSelectedScenes = new List<string>();

		public enum eToolsMode { Parse, Categorize, Merge, NoLocalized, Script, CharSet }
		public eToolsMode mCurrentToolsMode = eToolsMode.Parse;
		
		#endregion

		#region GUI

		void OnGUI_Tools( bool reset )
		{
			GUILayout.Space(10);
			eToolsMode OldMode = mCurrentToolsMode;
			mCurrentToolsMode = (eToolsMode)GUITools.DrawShadowedTabs ((int)mCurrentToolsMode, new[]{"解析Parse", "分类Categorize", "合并Merge", "无本地化 No Localized", "脚本Script", "字符CharSet"}, 30);
			if (mCurrentToolsMode != OldMode || reset)
			{
				ClearErrors();
				if (mCurrentToolsMode == eToolsMode.Script)
					SelectTermsFromScriptLocalization();
                OnGUI_ScenesList_SelectAllScenes(true);
            }

			switch (mCurrentToolsMode)
			{
				case eToolsMode.Parse 		: OnGUI_Tools_ParseTerms(); break;
				case eToolsMode.Categorize 	: OnGUI_Tools_Categorize(); break;
				case eToolsMode.Merge 		: OnGUI_Tools_MergeTerms(); break;
				case eToolsMode.NoLocalized : OnGUI_Tools_NoLocalized(); break;
				case eToolsMode.Script		: OnGUI_Tools_Script(); break;
				case eToolsMode.CharSet		: OnGUI_Tools_CharSet(); break;
			}
			OnGUI_ShowMsg();
		}

		#endregion
	}
}