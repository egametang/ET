using System;
using System.Collections.Generic;
using System.Linq;

namespace I2.Loc
{
	public partial class LanguageSourceData
	{
		#region Language

		public int GetLanguageIndex( string language, bool AllowDiscartingRegion = true, bool SkipDisabled = true)
		{
			// First look for an exact match
			for (int i=0, imax=mLanguages.Count; i<imax; ++i)
				if ((!SkipDisabled || mLanguages[i].IsEnabled()) && string.Compare(mLanguages[i].Name, language, StringComparison.OrdinalIgnoreCase)==0)
					return i;

			// Then allow matching "English (Canada)" to "english"
			if (AllowDiscartingRegion)
			{
				int MostSimilar = -1;
				int BestSimilitud = 0;
				for (int i=0, imax=mLanguages.Count; i<imax; ++i)
					if (!SkipDisabled || mLanguages[i].IsEnabled())
					{
						int commonWords = GetCommonWordInLanguageNames(mLanguages[i].Name, language);
						if (commonWords>BestSimilitud)
						{
							BestSimilitud = commonWords;
							MostSimilar = i;
						}
						//if (AreTheSameLanguage(mLanguages[i].Name, language))
						//	return i;
					}
				if (MostSimilar>=0)
					return MostSimilar;
			}
			return -1;
		}

        public LanguageData GetLanguageData(string language, bool AllowDiscartingRegion = true)
        {
            int idx = GetLanguageIndex(language, AllowDiscartingRegion, false);
            return idx < 0 ? null : mLanguages[idx];
        }

        // TODO: Fix IsCurrentLanguage when current=English  and there are two variants in the source (English Canada, English US)
        public bool IsCurrentLanguage( int languageIndex )
        {
            return LocalizationManager.CurrentLanguage == mLanguages[languageIndex].Name;
        }

        public int GetLanguageIndexFromCode( string Code, bool exactMatch=true, bool ignoreDisabled = false)
		{
            for (int i = 0, imax = mLanguages.Count; i < imax; ++i)
            {
                if (ignoreDisabled && !mLanguages[i].IsEnabled())
                    continue;

                if (string.Compare(mLanguages[i].Code, Code, StringComparison.OrdinalIgnoreCase) == 0)
                    return i;
            }

			if (!exactMatch)
			{
                // Find any match without using the Regions
                for (int i = 0, imax = mLanguages.Count; i < imax; ++i)
                {
                    if (ignoreDisabled && !mLanguages[i].IsEnabled())
                        continue;

                    if (string.Compare(mLanguages[i].Code, 0, Code, 0, 2, StringComparison.OrdinalIgnoreCase) == 0)
                        return i;
                }
			}

			return -1;
		}

		public static int GetCommonWordInLanguageNames(string Language1, string Language2)
		{
			if (string.IsNullOrEmpty (Language1) || string.IsNullOrEmpty (Language2))
					return 0;
			var separators = "( )-/\\".ToCharArray();
			string[] Words1 = Language1.ToLower().Split(separators);
			string[] Words2 = Language2.ToLower().Split(separators);

			int similitud = 0;
			foreach (var word in Words1)
				if (!string.IsNullOrEmpty(word) && Words2.Contains(word))
					similitud++;

			foreach (var word in Words2)
				if (!string.IsNullOrEmpty(word) && Words1.Contains(word))
					similitud++;

			return similitud;
		}

		public static bool AreTheSameLanguage(string Language1, string Language2)
		{
			Language1 = GetLanguageWithoutRegion(Language1);
			Language2 = GetLanguageWithoutRegion(Language2);
			return string.Compare(Language1, Language2, StringComparison.OrdinalIgnoreCase)==0;
		}

		public static string GetLanguageWithoutRegion(string Language)
		{
			int Index = Language.IndexOfAny("(/\\[,{".ToCharArray());
			if (Index<0)
				return Language;
			return Language.Substring(0, Index).Trim();
		}

        public void AddLanguage(string LanguageName)
        {
            AddLanguage(LanguageName, GoogleLanguages.GetLanguageCode(LanguageName));
        }

        public void AddLanguage( string LanguageName, string LanguageCode )
		{
			if (GetLanguageIndex(LanguageName, false)>=0)
				return;

			LanguageData Lang = new LanguageData();
				Lang.Name = LanguageName;
				Lang.Code = LanguageCode;
			mLanguages.Add (Lang);

			int NewSize = mLanguages.Count;
			for (int i=0, imax=mTerms.Count; i<imax; ++i)
			{
				Array.Resize(ref mTerms[i].Languages, NewSize);
				Array.Resize(ref mTerms[i].Flags, NewSize);
			}
            Editor_SetDirty();
        }

		public void RemoveLanguage( string LanguageName )
		{
			int LangIndex = GetLanguageIndex(LanguageName, false, false);
			if (LangIndex<0)
				return;

			int nLanguages = mLanguages.Count;
			for (int i=0, imax=mTerms.Count; i<imax; ++i)
			{
				for (int j=LangIndex+1; j<nLanguages; ++j)
				{
					mTerms[i].Languages[j-1] = mTerms[i].Languages[j];
					mTerms[i].Flags[j-1] = mTerms[i].Flags[j];
				}
				Array.Resize(ref mTerms[i].Languages, nLanguages-1);
				Array.Resize(ref mTerms[i].Flags, nLanguages-1);
			}
			mLanguages.RemoveAt(LangIndex);
            Editor_SetDirty();

        }

		public List<string> GetLanguages( bool skipDisabled = true)
		{
			List<string> Languages = new List<string>();
			for (int j = 0, jmax = mLanguages.Count; j < jmax; ++j)
			{
				if (!skipDisabled || mLanguages[j].IsEnabled())
					Languages.Add(mLanguages[j].Name);
			}
			return Languages;
		}

		public List<string> GetLanguagesCode(bool allowRegions = true, bool skipDisabled = true)
		{
			List<string> Languages = new List<string>();
			for (int j = 0, jmax = mLanguages.Count; j < jmax; ++j)
			{
				if (skipDisabled && !mLanguages[j].IsEnabled())
					continue;

				var code = mLanguages[j].Code;

				if (!allowRegions && code != null && code.Length > 2)
					code = code.Substring(0, 2);

				if (!string.IsNullOrEmpty(code) && !Languages.Contains(code))
					Languages.Add(code);
			}
			return Languages;
		}

		public bool IsLanguageEnabled(string Language)
		{
			int idx = GetLanguageIndex(Language, false);
			return idx >= 0 && mLanguages[idx].IsEnabled();
		}

        public void EnableLanguage(string Language, bool bEnabled)
        {
            int idx = GetLanguageIndex(Language, false, false);
            if (idx >= 0)
                mLanguages[idx].SetEnabled(bEnabled);
        }

        #endregion

        #region Save/Load Language

        public bool AllowUnloadingLanguages()
        {
            #if UNITY_EDITOR
                return _AllowUnloadingLanguages==eAllowUnloadLanguages.EditorAndDevice;
            #else
                return _AllowUnloadingLanguages!=eAllowUnloadLanguages.Never;
            #endif
        }

        string GetSavedLanguageFileName(int languageIndex)
        {
            if (languageIndex < 0)
                return null;

            return "LangSource_" + GetSourcePlayerPrefName() + "_" + mLanguages[languageIndex].Name + ".loc";
        }
        public void LoadLanguage( int languageIndex, bool UnloadOtherLanguages, bool useFallback, bool onlyCurrentSpecialization, bool forceLoad )
		{
            if (!AllowUnloadingLanguages())
                return;

            // Some consoles don't allow IO access
            if (!PersistentStorage.CanAccessFiles())
                return;

            if (languageIndex >= 0 && (forceLoad || !mLanguages[languageIndex].IsLoaded()))
            {
                var tempPath = GetSavedLanguageFileName(languageIndex);
                var langData = PersistentStorage.LoadFile(PersistentStorage.eFileType.Temporal, tempPath, false);

                if (!string.IsNullOrEmpty(langData))
                {
                    Import_Language_from_Cache(languageIndex, langData, useFallback, onlyCurrentSpecialization);
                    mLanguages[languageIndex].SetLoaded(true);
                }
            }
            if (UnloadOtherLanguages && I2Utils.IsPlaying())
            {
                for (int lan = 0; lan < mLanguages.Count; ++lan)
                {
                    if (lan != languageIndex)
                        UnloadLanguage(lan);
                }
            }
        }

        // if forceLoad, then the language is loaded from the cache even if its already loaded
        // this is needed to cleanup fallbacks
        public void LoadAllLanguages(bool forceLoad=false)
        {
            for (int i = 0; i < mLanguages.Count; ++i)
            {
                LoadLanguage(i, false, false, false, forceLoad);
            }
        }

        public void UnloadLanguage(int languageIndex)
        {
            if (!AllowUnloadingLanguages())
                return;

            // Some consoles don't allow IO access
            if (!PersistentStorage.CanAccessFiles())
                return;

            if (!I2Utils.IsPlaying() ||
                !mLanguages[languageIndex].IsLoaded() ||
                !mLanguages[languageIndex].CanBeUnloaded() ||
                IsCurrentLanguage(languageIndex) ||
                !PersistentStorage.HasFile(PersistentStorage.eFileType.Temporal, GetSavedLanguageFileName(languageIndex)))
            {
                return;
            }

            foreach (var termData in mTerms)
            {
                termData.Languages[languageIndex] = null;
            }
            mLanguages[languageIndex].SetLoaded(false);
        }

        public void SaveLanguages( bool unloadAll, PersistentStorage.eFileType fileLocation = PersistentStorage.eFileType.Temporal)
        {
            if (!AllowUnloadingLanguages())
                return;

            // Some consoles don't allow IO access
            if (!PersistentStorage.CanAccessFiles())
                return;

            for (int i = 0; i < mLanguages.Count; ++i)
            {
                var data = Export_Language_to_Cache(i, IsCurrentLanguage(i));
                if (string.IsNullOrEmpty(data))
                    continue;

                PersistentStorage.SaveFile(PersistentStorage.eFileType.Temporal, GetSavedLanguageFileName(i), data);
            }

            if (unloadAll)
            {
                for (int i = 0; i < mLanguages.Count; ++i)
                {
                    if (unloadAll && !IsCurrentLanguage(i))
                        UnloadLanguage(i);
                }
            }
        }

        public bool HasUnloadedLanguages()
        {
            for (int i = 0; i < mLanguages.Count; ++i)
            {
                if (!mLanguages[i].IsLoaded())
                    return true;
            }
            return false;

        }
#endregion
    }
}