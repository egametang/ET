using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables

		List<string> mCharSetTool_Languages = new List<string>();
		string mCharSet = string.Empty;
        bool mCharSetTool_CaseSensitive;

		#endregion
		
		#region GUI Generate Script
		
		void OnGUI_Tools_CharSet()
		{
			bool computeSet = false;

			// remove missing languages
			for (int i=mCharSetTool_Languages.Count-1; i>=0; --i)
			{
				if (mLanguageSource.GetLanguageIndex(mCharSetTool_Languages[i])<0)
					mCharSetTool_Languages.RemoveAt(i);
			}

			GUILayout.BeginHorizontal (EditorStyles.toolbar);
			GUILayout.Label ("Languages:", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
			if (GUILayout.Button ("All", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) 
			{
				mCharSetTool_Languages.Clear ();
				mCharSetTool_Languages.AddRange (mLanguageSource.mLanguages.Select(x=>x.Name));
				computeSet = true;
			}
			if (GUILayout.Button ("None", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) 
			{
				mCharSetTool_Languages.Clear ();
				computeSet = true;
			}
			if (GUILayout.Button ("Invert", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) 
			{
				var current = mCharSetTool_Languages.ToList ();
				mCharSetTool_Languages.Clear ();
				mCharSetTool_Languages.AddRange (mLanguageSource.mLanguages.Select(x=>x.Name).Where(j=>!current.Contains(j)));
				computeSet = true;
			}


			GUILayout.EndHorizontal ();

            //--[ Language List ]--------------------------

            GUI.backgroundColor = Color.Lerp(GUITools.LightGray, Color.white, 0.5f);
            mScrollPos_Languages = GUILayout.BeginScrollView( mScrollPos_Languages, LocalizeInspector.GUIStyle_OldTextArea, GUILayout.MinHeight (100), GUILayout.MaxHeight(Screen.height), GUILayout.ExpandHeight(false));
            GUI.backgroundColor = Color.white;

            for (int i=0, imax=mLanguageSource.mLanguages.Count; i<imax; ++i)
			{
				GUILayout.BeginHorizontal();
					var language = mLanguageSource.mLanguages[i].Name;
					bool hasLanguage = mCharSetTool_Languages.Contains(language);
					bool newValue = GUILayout.Toggle (hasLanguage, "", "OL Toggle", GUILayout.ExpandWidth(false));
					GUILayout.Label(language);
				GUILayout.EndHorizontal();

				if (hasLanguage != newValue)
				{
					if (newValue) 
						mCharSetTool_Languages.Add(language);
					else 
						mCharSetTool_Languages.Remove(language);

                    computeSet = true;
				}
			}
			
			GUILayout.EndScrollView();

			//GUILayout.Space (5);
			
			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));
			GUI.backgroundColor = Color.white;
			
			EditorGUILayout.HelpBox("此工具显示在所选语言中使用的所有字符\nThis tool shows all characters used in the selected languages", MessageType.Info);
			
            GUILayout.Space (5);
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.changed = false;
                mCharSetTool_CaseSensitive = GUILayout.Toggle(mCharSetTool_CaseSensitive, "Case-Sensitive", GUILayout.ExpandWidth(false));
                if (GUI.changed)
                    computeSet = true;
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
			GUILayout.Space (5);

            if (computeSet)
                UpdateCharSets();

			int numUsedChars = string.IsNullOrEmpty (mCharSet) ? 0 : mCharSet.Length;
			GUILayout.Label ("Used Characters: (" + numUsedChars+")");
			EditorGUILayout.TextArea (mCharSet ?? "");
			GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace ();
				if (GUILayout.Button ("Copy To Clipboard", GUITools.DontExpandWidth)) 
					EditorGUIUtility.systemCopyBuffer = mCharSet;
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
		}
		
		#endregion

		#region Generate Char Set

		void UpdateCharSets ()
		{
			mCharSet = "";
			var sb = new HashSet<char> ();
			var LanIndexes = new List<int> ();
			for (int i=0; i<mLanguageSource.mLanguages.Count; ++i)
				if (mCharSetTool_Languages.Contains(mLanguageSource.mLanguages[i].Name))
				    LanIndexes.Add(i);

			foreach (var termData in mLanguageSource.mTerms) 
			{
				for (int i=0; i<LanIndexes.Count; ++i)
				{
					int iLanguage = LanIndexes[i];
					bool isRTL = LocalizationManager.IsRTL( mLanguageSource.mLanguages[iLanguage].Code );
					AppendToCharSet( sb, termData.Languages[iLanguage], isRTL );
				}
			}
            var bytes = Encoding.UTF8.GetBytes( sb.ToArray().OrderBy(c => c).ToArray() );
            mCharSet = Encoding.UTF8.GetString(bytes);
		}

		void AppendToCharSet( HashSet<char> sb, string text, bool isRTL )
		{
			if (string.IsNullOrEmpty (text))
				return;

            text = RemoveTagsPrefix(text, "[i2p_");
            text = RemoveTagsPrefix(text, "[i2s_");

            if (isRTL)
				text = RTLFixer.Fix( text );

            foreach (char c in text)
            {
                if (!mCharSetTool_CaseSensitive)
                {
                    sb.Add(char.ToLowerInvariant(c));
                    sb.Add(char.ToUpperInvariant(c));
                }
                else
                    sb.Add(c);
            }
		}

        // Given "[i2p_"  it removes all tags that start with that  (e.g. [i2p_Zero]  [i2p_One], etc)
        string RemoveTagsPrefix(string text, string tagPrefix)
        {
            int idx = 0;
            while (idx < text.Length)
            {
                idx = text.IndexOf(tagPrefix);
                if (idx < 0)
                    break;

                int idx2 = text.IndexOf(']', idx);
                if (idx2 < 0)
                    break;

                text = text.Remove(idx, idx2 - idx+1);
            }
            return text;

        }



        #endregion
    }
}
