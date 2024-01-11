using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngineInternal;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables

		int Script_Tool_MaxVariableLength = 50;

		#endregion
		
		#region GUI Generate Script
		
		void OnGUI_Tools_Script()
		{
			OnGUI_KeysList (false, 200, false);

			//GUILayout.Space (5);
			
			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical(LocalizeInspector.GUIStyle_OldTextArea, GUILayout.Height(1));
			GUI.backgroundColor = Color.white;
			
			EditorGUILayout.HelpBox("这个工具用所选的术语创建\nScriptLocalization.cs\n这允许对脚本中引用的已使用术语进行编译时检查\n\nScriptLocalization.cs。\nThis tool creates the ScriptLocalization.cs with the selected terms.\nThis allows for Compile Time Checking on the used Terms referenced in scripts", MessageType.Info);
			
			GUILayout.Space (5);

			GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace();
				EditorGUIUtility.labelWidth = 240;
				EditorGUILayout.IntField("生成的Term id的最大长度:Max Length of the Generated Term IDs:", Script_Tool_MaxVariableLength);
				EditorGUIUtility.labelWidth = 0;
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space (10);
			
			GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("选择烘焙术语\nSelect Baked Terms", "选择之前在ScriptLocalization.cs中构建的所有术语\nSelects all the terms previously built in ScriptLocalization.cs")))
                    SelectTermsFromScriptLocalization();

				if (GUILayout.Button("使用选定的术语构建脚本\nBuild Script with Selected Terms"))
					EditorApplication.update += BuildScriptWithSelectedTerms;
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
		}

        void SelectTermsFromScriptLocalization()
        {
            var ScriptFile = GetPathToGeneratedScriptLocalization();

            try
            {
                var text = File.ReadAllText(ScriptFile, Encoding.UTF8);

                mSelectedKeys.Clear();
                foreach (Match match in Regex.Matches(text, "\".+\""))
                {
                    var term = match.Value.Substring(1, match.Value.Length - 2);

                    if (!mSelectedKeys.Contains(term))
                    {
                        mSelectedKeys.Add(term);
                    }
                }
            }
            catch(Exception)
            { }
        }

		#endregion

		#region Generate Script File

		private static string I2LocalizeCS = "I2Localize";
		private static string I2TermsCS = "I2Terms";
		
		void BuildScriptWithSelectedTerms()
		{
			EditorApplication.update -= BuildScriptWithSelectedTerms;
            var sbTrans = new StringBuilder();
            var sbTerms = new StringBuilder();
            sbTrans.AppendLine( "using UnityEngine;" );
            sbTrans.AppendLine();
            sbTrans.AppendLine( "namespace I2.Loc" );
            sbTrans.AppendLine( "{" );
            sbTrans.AppendLine( $"	public static class {I2LocalizeCS}" );
            sbTrans.AppendLine( "	{" );


            sbTerms.AppendLine();
            sbTerms.AppendLine($"    public static class {I2TermsCS}");
            sbTerms.AppendLine("	{");



            BuildScriptWithSelectedTerms( sbTrans, sbTerms );
            sbTrans.AppendLine("	}");    // Closing both classes
            sbTerms.AppendLine("	}");


            string ScriptFile = GetPathToGeneratedScriptLocalization ();
			Debug.Log ("Generating: " + ScriptFile);

            var filePath = Application.dataPath + ScriptFile.Substring("Assets".Length);
            string fileText = sbTrans + sbTerms.ToString() + "}";

            File.WriteAllText(filePath, fileText, Encoding.UTF8);

			AssetDatabase.ImportAsset(ScriptFile);
		}

		static string GetPathToGeneratedScriptLocalization()
		{
			/*string[] assets = AssetDatabase.FindAssets("ScriptLocalization");
			if (assets.Length>0)
            {
                try
                {
                    string FilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
                    return FilePath;
                }
                catch(Exception)
                { }
            }*/
			
			var path = $"{YIUIFramework.UIStaticHelper.UIETComponentGenPath}/I2Localization";
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			
			return $"{path}/{I2LocalizeCS}.cs";
		}

		void BuildScriptWithSelectedTerms( StringBuilder sbTrans, StringBuilder sbTerms )
		{
			List<string> Categories = LocalizationManager.GetCategories();
			foreach (string Category in Categories)
			{
				List<string> CategoryTerms = ScriptTool_GetSelectedTermsInCategory(Category);
				if (CategoryTerms.Count<=0)
					continue;

				List<string> AdjustedCategoryTerms = new List<string>(CategoryTerms);
				for (int i=0, imax=AdjustedCategoryTerms.Count; i<imax; ++i)
					AdjustedCategoryTerms[i] = ScriptTool_AdjustTerm( AdjustedCategoryTerms[i] );
				ScriptTool_EnumerateDuplicatedTerms(AdjustedCategoryTerms);

                sbTrans.AppendLine();
                sbTerms.AppendLine();
                if (Category != LanguageSourceData.EmptyCategory)
				{
                    sbTrans.AppendLine("		public static class " + ScriptTool_AdjustTerm(Category,true));
                    sbTrans.AppendLine("		{");

                    sbTerms.AppendLine("		public static class " + ScriptTool_AdjustTerm(Category, true));
                    sbTerms.AppendLine("		{");
                }

                BuildScriptCategory( sbTrans, sbTerms, Category, AdjustedCategoryTerms, CategoryTerms );

				if (Category != LanguageSourceData.EmptyCategory)
				{
                    sbTrans.AppendLine("		}");
                    sbTerms.AppendLine("		}");
                }
            }
		}

		List<string> ScriptTool_GetSelectedTermsInCategory( string Category )
		{
			List<string> list = new List<string>();
			foreach (string FullKey in mSelectedKeys)
			{
				string categ =  LanguageSourceData.GetCategoryFromFullTerm(FullKey);
				if (categ == Category && ShouldShowTerm(FullKey))
				{
					list.Add(  LanguageSourceData.GetKeyFromFullTerm(FullKey) );
				}
			}

			return list;
		}

		void BuildScriptCategory( StringBuilder sbTrans, StringBuilder sbTerms, string Category, List<string> AdjustedTerms, List<string> Terms )
		{
			if (Category==LanguageSourceData.EmptyCategory)
			{
                for (int i = 0; i < Terms.Count; ++i)
                {
	                if (Terms[i] == Category)
	                {
		                Debug.LogError($"分组名称与术语相同 请修改 {Category}");
		                continue;
	                }
                    sbTrans.AppendLine( "		public static string " + AdjustedTerms[i] + " \t\t{ get{ return LocalizationManager.GetTranslation (\"" + Terms[i] + "\"); } }");
                    sbTerms.AppendLine("		public const string " + AdjustedTerms[i] + " = \"" + Terms[i] + "\";");
                }
            }
			else
				for (int i=0; i<Terms.Count; ++i)
				{
					if (Terms[i] == Category)
					{
						Debug.LogError($"分组名称与术语相同 请修改 {Category}");
						continue;
					}
					sbTrans.AppendLine("			public static string "+AdjustedTerms[i]+ " \t\t{ get{ return LocalizationManager.GetTranslation (\"" + Category+"/"+Terms[i]+"\"); } }");
					sbTerms.AppendLine("		    public const string " + AdjustedTerms[i] + " = \"" + Category + "/" + Terms[i] + "\";");
				}
		}

		string ScriptTool_AdjustTerm( string Term, bool allowFullLength = false )
		{
            Term = I2Utils.GetValidTermName(Term);

			// C# IDs can't start with a number
			if (I2Utils.NumberChars.IndexOf(Term[0])>=0)
				Term = "_"+Term;
			
			if (!allowFullLength && Term.Length>Script_Tool_MaxVariableLength)
				Term = Term.Substring(0, Script_Tool_MaxVariableLength);
			
			// Remove invalid characters
			char[] chars = Term.ToCharArray();
			for (int i=0, imax=chars.Length; i<imax; ++i)
			{
				if (!IsValidCharacter(chars[i]))
					chars[i] = '_';
			}
			
			Term = new string(chars);
			if (IsCSharpKeyword(Term)) return string.Concat('@', Term);
			return Term;

			bool IsValidCharacter(char c)
			{
				if (I2Utils.ValidChars.IndexOf(c)>=0) return true;
				return c>='\u4e00' && c<='\u9fff'; // Chinese/Japanese characters
			}
		}

		void ScriptTool_EnumerateDuplicatedTerms(List<string> AdjustedTerms)
		{
			string lastTerm = "$";
			int Counter = 1;
			for (int i=0, imax=AdjustedTerms.Count; i<imax; ++i)
			{
				string currentTerm = AdjustedTerms[i];
				if (lastTerm == currentTerm || i<imax-1 && currentTerm==AdjustedTerms[i+1])
				{
					AdjustedTerms[i] = AdjustedTerms[i] + "_" + Counter;
					Counter++;
				}
				else
					Counter = 1;

				lastTerm = currentTerm;
			}
		}

		#endregion

		bool IsCSharpKeyword(string variableName)
		{
			return variableName == "abstract" || variableName == "as" || variableName == "base" || variableName == "bool" || 
			       variableName == "break" || variableName == "byte" || variableName == "" || variableName == "case" || 
			       variableName == "catch" || variableName == "char" || variableName == "checked" || variableName == "class" || 
			       variableName == "const" || variableName == "continue" || variableName == "decimal" || variableName == "default" || 
			       variableName == "delegate" || variableName == "do" || variableName == "double" || variableName == "else" || 
			       variableName == "enum" || variableName == "event" || variableName == "explicit" || variableName == "extern" || 
			       variableName == "false" || variableName == "finally" || variableName == "fixed" || variableName == "float" || 
			       variableName == "for" || variableName == "foreach" || variableName == "goto" || variableName == "if" || 
			       variableName == "implicit" || variableName == "in" || variableName == "int" || variableName == "interface" || 
			       variableName == "internal" || variableName == "is" || variableName == "lock" || variableName == "long" || 
			       variableName == "namespace" || variableName == "new" || variableName == "null" || variableName == "object" || 
			       variableName == "operator" || variableName == "out" || variableName == "override" || variableName == "params" || 
			       variableName == "private" || variableName == "protected" || variableName == "public" || variableName == "readonly" || 
			       variableName == "ref" || variableName == "return" || variableName == "sbyte" || variableName == "sealed" || 
			       variableName == "short" || variableName == "sizeof" || variableName == "stackalloc" || variableName == "static" || 
			       variableName == "string" || variableName == "struct" || variableName == "switch" || variableName == "this" || 
			       variableName == "throw" || variableName == "true" || variableName == "try" || variableName == "typeof" || 
			       variableName == "uint" || variableName == "ulong" || variableName == "unchecked" || variableName == "unsafe" || 
			       variableName == "short" || variableName == "using" || variableName == "virtual" || variableName == "void" || 
			       variableName == "volatile" || variableName == "while";
		}
	}
}
