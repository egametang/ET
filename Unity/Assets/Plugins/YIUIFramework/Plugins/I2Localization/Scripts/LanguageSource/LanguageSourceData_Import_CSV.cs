using System;
using System.Collections.Generic;
using UnityEngine;

namespace I2.Loc
{
	public partial class LanguageSourceData
	{
		public string Import_CSV( string Category, string CSVstring, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace, char Separator = ',' )
		{
			List<string[]> CSV = LocalizationReader.ReadCSV (CSVstring, Separator);
			return Import_CSV( Category, CSV, UpdateMode );
		}

		public string Import_I2CSV( string Category, string I2CSVstring, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace )
		{
			List<string[]> CSV = LocalizationReader.ReadI2CSV (I2CSVstring);
			return Import_CSV( Category, CSV, UpdateMode );
		}

		public string Import_CSV( string Category, List<string[]> CSV, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace )
		{
			string[] Tokens = CSV[0];

			int LanguagesStartIdx = 1;
			int TypeColumnIdx = -1;
			int DescColumnIdx = -1;

			var ValidColumnName_Key  = new[]{ "Key" };
			var ValidColumnName_Type = new[]{ "Type" };
			var ValidColumnName_Desc = new[]{ "Desc", "Description" };

			if (Tokens.Length>1 && ArrayContains(Tokens[0], ValidColumnName_Key))
			{
				if (UpdateMode == eSpreadsheetUpdateMode.Replace)
					ClearAllData();

				if (Tokens.Length>2)
				{
					if (ArrayContains(Tokens[1], ValidColumnName_Type)) 
					{
						TypeColumnIdx = 1;
						LanguagesStartIdx = 2;
					}
					if (ArrayContains(Tokens[1], ValidColumnName_Desc)) 
					{
						DescColumnIdx = 1;
						LanguagesStartIdx = 2;
					}

				}
				if (Tokens.Length>3)
				{
					if (ArrayContains(Tokens[2], ValidColumnName_Type)) 
					{
						TypeColumnIdx = 2;
						LanguagesStartIdx = 3;
					}
					if (ArrayContains(Tokens[2], ValidColumnName_Desc)) 
					{
						DescColumnIdx = 2;
						LanguagesStartIdx = 3;
					}
				}
			}
			else
				return "Bad Spreadsheet Format.\nFirst columns should be 'Key', 'Type' and 'Desc'";

			int nLanguages = Mathf.Max (Tokens.Length-LanguagesStartIdx, 0);
			int[] LanIndices = new int[nLanguages];
			for (int i=0; i<nLanguages; ++i)
			{
				if (string.IsNullOrEmpty(Tokens[i+LanguagesStartIdx]))
				{
					LanIndices [i] = -1;
					continue;
				}

				string langToken = Tokens[i + LanguagesStartIdx];

				string LanName, LanCode;
				bool isLangEnabled = true;
				if (langToken.StartsWith("$", StringComparison.Ordinal))
				{
					isLangEnabled = false;
					langToken = langToken.Substring(1);
				}
				GoogleLanguages.UnPackCodeFromLanguageName(langToken, out LanName, out LanCode);

				int LanIdx = -1;
				if (!string.IsNullOrEmpty(LanCode))
					LanIdx = GetLanguageIndexFromCode(LanCode);
				else
					LanIdx = GetLanguageIndex(LanName, SkipDisabled:false);

				if (LanIdx < 0)
				{
					LanguageData lanData = new LanguageData();
					lanData.Name = LanName;
					lanData.Code = LanCode;
					lanData.Flags = (byte)(0 | (isLangEnabled?0:(int)eLanguageDataFlags.DISABLED));
					mLanguages.Add (lanData);
					LanIdx = mLanguages.Count-1;
				}
				LanIndices[i] = LanIdx;
			}

			//--[ Update the Languages array in the existing terms]-----
			nLanguages = mLanguages.Count;
			for (int i=0, imax=mTerms.Count; i<imax; ++i)
			{
				TermData termData = mTerms[i];
				if (termData.Languages.Length < nLanguages)
				{
					Array.Resize( ref termData.Languages, nLanguages );
					Array.Resize( ref termData.Flags, nLanguages );
				}
			}

            //--[ Keys ]--------------

            for (int i = 1, imax = CSV.Count; i < imax; ++i)
            {
                Tokens = CSV[i];
                string sKey = string.IsNullOrEmpty(Category) ? Tokens[0] : string.Concat(Category, "/", Tokens[0]);

                string specialization = null;
                if (sKey.EndsWith("]", StringComparison.Ordinal))
                {
                    int idx = sKey.LastIndexOf('[');
                    if (idx>0)
                    {
                        specialization = sKey.Substring(idx + 1, sKey.Length - idx - 2);
                        if (specialization == "touch") specialization = "Touch";
                        sKey = sKey.Remove(idx);
                    }
                }
				ValidateFullTerm(ref sKey);
				if (string.IsNullOrEmpty(sKey))
					continue;

				TermData termData = GetTermData(sKey);

				// Check to see if its a new term
				if (termData==null)
				{
					termData = new TermData();
					termData.Term = sKey;

					termData.Languages = new string[ mLanguages.Count ];
					termData.Flags = new byte[ mLanguages.Count ];
					for (int j=0; j<mLanguages.Count; ++j) 
						termData.Languages[j] = string.Empty;

					mTerms.Add (termData);
					mDictionary.Add (sKey, termData);
				}
				else
				// This term already exist
				if (UpdateMode==eSpreadsheetUpdateMode.AddNewTerms)
					continue;

				if (TypeColumnIdx>0)
					termData.TermType = GetTermType(Tokens[TypeColumnIdx]);

				if (DescColumnIdx>0)
					termData.Description = Tokens[DescColumnIdx];

                for (int j = 0; j < LanIndices.Length && j < Tokens.Length - LanguagesStartIdx; ++j)
                    if (!string.IsNullOrEmpty(Tokens[j + LanguagesStartIdx]))   // Only change the translation if there is a new value
                    {
                        var lanIdx = LanIndices[j];
                        if (lanIdx < 0)
                            continue;
                        var value = Tokens[j + LanguagesStartIdx];

                        if (value == "-")
                            value = string.Empty;
                        else
                        if (value == "")
                            value = null;

                        termData.SetTranslation(lanIdx, value, specialization);
                    }
            }
            if (Application.isPlaying)
            {
                SaveLanguages(HasUnloadedLanguages());
            }
            return string.Empty;
		}

		bool ArrayContains( string MainText, params string[] texts )
		{
			for (int i=0, imax=texts.Length; i<imax; ++i)
				if (MainText.IndexOf(texts[i], StringComparison.OrdinalIgnoreCase)>=0)
					return true;
			return false;
		}

		public static eTermType GetTermType( string type )
		{
			for (int i=0, imax=(int)eTermType.Object; i<=imax; ++i)
				if (string.Equals( ((eTermType)i).ToString(), type, StringComparison.OrdinalIgnoreCase))
					return (eTermType)i;
			
			return eTermType.Text;
		}

        #region Language Cache format

        void Import_Language_from_Cache(int langIndex, string langData, bool useFallback, bool onlyCurrentSpecialization)
        {
            int index = 0;
            while (index < langData.Length)
            {
                int nextIndex = langData.IndexOf("[i2t]", index, StringComparison.Ordinal);
                if (nextIndex < 0) nextIndex = langData.Length;

                // check for errors
                int termNameEnd = langData.IndexOf("=", index, StringComparison.Ordinal);
                if (termNameEnd >= nextIndex)
                    return;

                string termName = langData.Substring(index, termNameEnd - index);
                index = termNameEnd+1;

                var termData = GetTermData(termName);
                if (termData != null)
                {
                    string translation = null;

                    if (index != nextIndex)
                    {
                        translation = langData.Substring(index, nextIndex - index);
                        if (translation.StartsWith("[i2fb]", StringComparison.Ordinal))
                        {
                            translation = useFallback ? translation.Substring(6) : null;
                        }
                        if (onlyCurrentSpecialization && translation != null)
                        {
                            translation = SpecializationManager.GetSpecializedText(translation);
                        }
                    }
                    termData.Languages[langIndex] = translation;
                }
                index = nextIndex + 5;
            }
        }

        #endregion
    }
}