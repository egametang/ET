using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
	public partial class LanguageSourceData
	{
        #region Language

        public void UpdateDictionary(bool force = false)
        {
            if (!force && mDictionary != null && mDictionary.Count == mTerms.Count)
                return;

            StringComparer comparer = CaseInsensitiveTerms ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            if (mDictionary.Comparer != comparer)
                mDictionary = new Dictionary<string, TermData>(comparer);
            else
                mDictionary.Clear();

            for (int i = 0, imax = mTerms.Count; i < imax; ++i)
            {
                var termData = mTerms[i];
                ValidateFullTerm(ref termData.Term);

				mDictionary[termData.Term]= mTerms[i];
				mTerms[i].Validate();
			}

            if (I2Utils.IsPlaying())
            {
                SaveLanguages(true);
            }
        }

        public string GetTranslation (string term, string overrideLanguage = null, string overrideSpecialization = null, bool skipDisabled = false, bool allowCategoryMistmatch = false)
		{
			TryGetTranslation(term, out string translation, overrideLanguage:overrideLanguage, overrideSpecialization:overrideSpecialization, skipDisabled:skipDisabled, allowCategoryMistmatch:allowCategoryMistmatch);
			return translation;
		}

		public bool TryGetTranslation (string term, out string Translation, string overrideLanguage=null, string overrideSpecialization=null, bool skipDisabled=false, bool allowCategoryMistmatch=false)
		{
			int Index = GetLanguageIndex( overrideLanguage==null ? LocalizationManager.CurrentLanguage : overrideLanguage, SkipDisabled: false );

			if (Index>=0 && (!skipDisabled || mLanguages[Index].IsEnabled()))
			{
				TermData data = GetTermData(term, allowCategoryMistmatch:allowCategoryMistmatch);
				if (data!=null)
				{
					Translation = data.GetTranslation(Index, overrideSpecialization, editMode:true);

					// "---" is a code to define that the translation is meant to be empty
					if (Translation == "---")
					{
						Translation = string.Empty;
						return true;
					}

					if (!string.IsNullOrEmpty(Translation))
					{
						// has a valid translation
						return true;
					}

					Translation = null;
				}

				if (OnMissingTranslation == MissingTranslationAction.ShowWarning)
				{
					Translation = $"<!-Missing Translation [{term}]-!>";
					Debug.LogWarning($"Missing Translation for '{term}'", Localize.CurrentLocalizeComponent);
					return false;
				}

				if (OnMissingTranslation == MissingTranslationAction.Fallback && data!=null)
				{
					return TryGetFallbackTranslation(data, out Translation, Index, overrideSpecialization, skipDisabled);
				}

				if (OnMissingTranslation == MissingTranslationAction.Empty)
				{
					Translation = string.Empty;
					return false;
				}

				if (OnMissingTranslation == MissingTranslationAction.ShowTerm)
				{
					Translation = term;
					return false;
				}

			}

            Translation = null;
			return false;
		}

        bool TryGetFallbackTranslation(TermData termData, out string Translation, int langIndex, string overrideSpecialization = null, bool skipDisabled = false)
        {
            // Find base Language Code
            string baseLanguage = mLanguages[langIndex].Code;
            if (!string.IsNullOrEmpty(baseLanguage))
            {
                if (baseLanguage.Contains("-"))
                {
                    baseLanguage = baseLanguage.Substring(0, baseLanguage.IndexOf('-'));
                }

                // Try finding in any of the Region of the base language
                for (int i = 0; i < mLanguages.Count; ++i)
                {
                    if (i != langIndex && 
                        mLanguages[i].Code.StartsWith(baseLanguage, StringComparison.Ordinal) &&
                        (!skipDisabled || mLanguages[i].IsEnabled()) )
                    {
                        Translation = termData.GetTranslation(i, overrideSpecialization, editMode: true);
                        if (!string.IsNullOrEmpty(Translation))
                            return true;
                    }
                }
            }


            // Otherwise, Try finding the first active language with a valid translation
            for (int i = 0; i < mLanguages.Count; ++i)
            {
                if (i!=langIndex && 
                    (!skipDisabled || mLanguages[i].IsEnabled()) && 
                    (baseLanguage==null || !mLanguages[i].Code.StartsWith(baseLanguage, StringComparison.Ordinal)))
                {
                    Translation = termData.GetTranslation(i, overrideSpecialization, editMode: true);
                    if (!string.IsNullOrEmpty(Translation))
                        return true;
                }
            }
            Translation = null;
            return false;
        }

		public TermData AddTerm( string term )
		{
			return AddTerm (term, eTermType.Text);
		}

		public TermData GetTermData( string term, bool allowCategoryMistmatch = false)
		{
			if (string.IsNullOrEmpty(term))
				return null;
			
			if (mDictionary.Count==0)// != mTerms.Count)
				UpdateDictionary();

			TermData data;
            if (mDictionary.TryGetValue(term, out data))
                return data;

			TermData d = null;
			if (allowCategoryMistmatch)
			{
				var keyPart = GetKeyFromFullTerm (term);
				foreach (var kvp in mDictionary)
					if (kvp.Value.IsTerm (keyPart, true))
					{
						if (d == null)
							d = kvp.Value;
						else
							return null;
					}
			}
			return d;
		}

		public bool ContainsTerm(string term)
		{
			return GetTermData(term)!=null;
		}

		public List<string> GetTermsList ( string Category = null )
		{
			if (mDictionary.Count != mTerms.Count)
				UpdateDictionary();
			if (string.IsNullOrEmpty( Category ))
				return new List<string>( mDictionary.Keys );
			var terms = new List<string>();
			for (int i=0; i<mTerms.Count; ++i)
			{
				var term = mTerms[i];
				if (GetCategoryFromFullTerm( term.Term ) == Category)
					terms.Add( term.Term );
			}
			return terms;
		}

		public  TermData AddTerm( string NewTerm, eTermType termType, bool SaveSource = true )
		{
			ValidateFullTerm( ref NewTerm );
			NewTerm = NewTerm.Trim ();

			if (mLanguages.Count == 0) 
				AddLanguage ("English", "en");

			// Don't duplicate Terms
			TermData data = GetTermData(NewTerm);
			if (data==null) 
			{
				data = new TermData();
				data.Term = NewTerm;
				data.TermType = termType;
				data.Languages = new string[ mLanguages.Count ];
				data.Flags = new byte[ mLanguages.Count ];
				mTerms.Add (data);
				mDictionary.Add ( NewTerm, data);
				#if UNITY_EDITOR
				if (SaveSource)
				{
                    Editor_SetDirty();
					AssetDatabase.SaveAssets();
				}
				#endif
			}

			return data;
		}

		public void RemoveTerm( string term )
		{
			for (int i=0, imax=mTerms.Count; i<imax; ++i)
				if (mTerms[i].Term==term)
				{
					mTerms.RemoveAt(i);
					mDictionary.Remove(term);
					return;
				}
		}

		public static void ValidateFullTerm( ref string Term )
		{
			Term = Term.Replace('\\', '/');
			Term = Term.Trim();
			if (Term.StartsWith(EmptyCategory, StringComparison.Ordinal))
			{
				if (Term.Length>EmptyCategory.Length && Term[EmptyCategory.Length]=='/')
					Term = Term.Substring(EmptyCategory.Length+1);
			}
            Term = I2Utils.GetValidTermName(Term, true);
        }


        #endregion
    }
}