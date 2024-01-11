using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace I2.Loc
{
	using TranslationDictionary = Dictionary<string, TranslationQuery>;

    public struct TranslationQuery
    {
        public string OrigText;
        public string Text;           // Text without Tags
        public string LanguageCode;
        public string[] TargetLanguagesCode;
        public string[] Results;			// This is filled google returns the translations
        public string[] Tags;               // This are the sections of the orignal text that shoudn't be translated
    }

    public static partial class GoogleTranslation
	{
        public static void CreateQueries(string text, string LanguageCodeFrom, string LanguageCodeTo, TranslationDictionary dict)
        {
            if (!text.Contains("[i2s_"))
            {
                CreateQueries_Plurals(text, LanguageCodeFrom, LanguageCodeTo, dict);
                return;
            }

            var variants = SpecializationManager.GetSpecializations(text);
            foreach (var kvp in variants)
            {
                CreateQueries_Plurals(kvp.Value, LanguageCodeFrom, LanguageCodeTo, dict);
            }
        }

        static void CreateQueries_Plurals(string text, string LanguageCodeFrom, string LanguageCodeTo, TranslationDictionary dict)
        {
            bool hasPluralParams = text.Contains("{[#");
            bool hasPluralTypes = text.Contains("[i2p_");
            if (!HasParameters(text) || !hasPluralParams && !hasPluralTypes)
            {
                AddQuery(text, LanguageCodeFrom, LanguageCodeTo, dict);
                return;
            }

            bool forcePluralParam = hasPluralParams;

            for (var i = (ePluralType)0; i <= ePluralType.Plural; ++i)
            {
                var pluralType = i.ToString();
                if (!GoogleLanguages.LanguageHasPluralType(LanguageCodeTo, pluralType))
                    continue;

                var newText = GetPluralText(text, pluralType);
                int testNumber = GoogleLanguages.GetPluralTestNumber(LanguageCodeTo, i);

                var parameter = GetPluralParameter(newText, forcePluralParam);
                if (!string.IsNullOrEmpty(parameter))
                    newText = newText.Replace(parameter, testNumber.ToString());

                //Debug.Log("Translate: " + newText);

                AddQuery(newText, LanguageCodeFrom, LanguageCodeTo, dict);
            }
        }

        public static void AddQuery(string text, string LanguageCodeFrom, string LanguageCodeTo, TranslationDictionary dict)
        {
            if (string.IsNullOrEmpty(text))
                return;


            if (!dict.ContainsKey(text))
            {
                var query = new TranslationQuery { OrigText = text, LanguageCode = LanguageCodeFrom, TargetLanguagesCode = new[] { LanguageCodeTo } };
                query.Text = text;
                ParseNonTranslatableElements(ref query);
                dict[text] = query;
            }
            else
            {
                var query = dict[text];
                if (Array.IndexOf(query.TargetLanguagesCode, LanguageCodeTo) < 0)
                {
                    query.TargetLanguagesCode = query.TargetLanguagesCode.Concat(new[] { LanguageCodeTo }).Distinct().ToArray();
                }
                dict[text] = query;
            }
        }

        static string GetTranslation(string text, string LanguageCodeTo, TranslationDictionary dict)
        {
            if (!dict.ContainsKey(text))
                return null;
            var query = dict[text];

            int langIdx = Array.IndexOf(query.TargetLanguagesCode, LanguageCodeTo);
            if (langIdx < 0)
                return "";

            if (query.Results == null)
                return "";
            return query.Results[langIdx];
        }

        static TranslationQuery FindQueryFromOrigText(string origText, TranslationDictionary dict)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value.OrigText == origText)
                    return kvp.Value;
            }
            return default(TranslationQuery);
        }

        public static bool HasParameters( string text )
        {
            int idx = text.IndexOf("{[", StringComparison.Ordinal);
            if (idx < 0) return false;
            return text.IndexOf("]}", idx, StringComparison.Ordinal) > 0;
        }

        public static string GetPluralParameter(string text, bool forceTag)  // force tag requires that the parameter has the form {[#param]}
        {
            // Try finding the "plural parameter" that has the form {[#name]}
            // this allows using the second parameter as plural:  "Player {[name1]} has {[#value]} points"
            int idx0 = text.IndexOf("{[#", StringComparison.Ordinal);
            if (idx0 < 0)
            {
                if (forceTag) return null;
                idx0 = text.IndexOf("{[", StringComparison.Ordinal); // fallback to the first paremeter if no one has the # tag
            }
            if (idx0 < 0)
                return null;

            int idx1 = text.IndexOf("]}", idx0+2, StringComparison.Ordinal);
            if (idx1 < 0)
                return null;     // no closing parameter tag

            return text.Substring(idx0, idx1 - idx0 + 2);
        }

        public static string GetPluralText( string text, string pluralType )
        {
            pluralType = "[i2p_" + pluralType + "]";
            int idx0 = text.IndexOf(pluralType, StringComparison.Ordinal);
            if (idx0>=0)
            {
                idx0 += pluralType.Length;
                int idx1 = text.IndexOf("[i2p_",idx0, StringComparison.Ordinal);
                if (idx1 < 0) idx1 = text.Length;

                return text.Substring(idx0, idx1 - idx0);
            }

            // PluralType not found, fallback to the first one
            idx0 = text.IndexOf("[i2p_", StringComparison.Ordinal);
            if (idx0 < 0)
                return text;                      // No plural tags:   "my text"

            if (idx0>0)
                return text.Substring(0, idx0);   // Case: "my text[i2p_zero]hello"

            // Case: "[i2p_plural]my text[i2p_zero]hello"
            idx0 = text.IndexOf("]", StringComparison.Ordinal);
            if (idx0 < 0) return text;  // starts like a plural, but has none

            idx0 += 1;
            int idx2 = text.IndexOf("[i2p_", idx0, StringComparison.Ordinal);
            if (idx2 < 0) idx2 = text.Length;
            return text.Substring(idx0, idx2 - idx0);
        }
         
        static int FindClosingTag(string tag, MatchCollection matches, int startIndex)
        {
            for (int i = startIndex, imax = matches.Count; i < imax; ++i)
            {
                var newTag = I2Utils.GetCaptureMatch(matches[i]);
                if (newTag[0]=='/' && tag.StartsWith(newTag.Substring(1), StringComparison.Ordinal))
                    return i;
            }
            return -1;
        }

	    static string GetGoogleNoTranslateTag(int tagNumber)
	    {
	        //return " I2NT" + tagNumber;
	        if (tagNumber < 70)
	            return "++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++".Substring(0, tagNumber+1);

	        string s = "";
	        for (int i = -1; i < tagNumber; ++i)
	            s += "+";
	        return s;
	    }


        static void ParseNonTranslatableElements( ref TranslationQuery query )
        {
            //\[i2nt].*\[\/i2nt]
            var matches = Regex.Matches(  query.Text, @"\{\[(.*?)]}|\[(.*?)]|\<(.*?)>");
            if (matches == null || matches.Count == 0)
                return;

            string finalText = query.Text;
            List<string> finalTags = new List<string>();
            for (int i=0, imax=matches.Count; i<imax; ++i)
            {
                var tag = I2Utils.GetCaptureMatch( matches[i] );
                int iClosingTag = FindClosingTag(tag, matches, i); //  find [/tag] or </tag>

                if (iClosingTag < 0)
                {
                    // Its not a tag, its a parameter
                    var fulltag = matches[i].ToString();
                    if (fulltag.StartsWith("{[", StringComparison.Ordinal) && fulltag.EndsWith("]}", StringComparison.Ordinal))
                    {
                        finalText = finalText.Replace(fulltag, GetGoogleNoTranslateTag(finalTags.Count)+" ");  //  0x2600 is the start of the UNICODE Miscellaneous Symbols table, so they are not going to be translated by google
                        //finalText = finalText.Replace(fulltag, /*"{[" + finalTags.Count + "]}"*/ ((char)(0x2600 + finalTags.Count)).ToString());  //  0x2600 is the start of the UNICODE Miscellaneous Symbols table, so they are not going to be translated by google
                        finalTags.Add(fulltag);
                    }
                    continue;
                }

                if (tag == "i2nt")
                {
                    var tag1 = query.Text.Substring(matches[i].Index, matches[iClosingTag].Index-matches[i].Index + matches[iClosingTag].Length);
                    finalText = finalText.Replace(tag1, GetGoogleNoTranslateTag(finalTags.Count)+" ");
                    //finalText = finalText.Replace(tag1, /*"{[" + finalTags.Count + "]}"*/ ((char)(0x2600 + finalTags.Count)).ToString());
                    
                    finalTags.Add(tag1);
                }
                else
                {
                    var tag1 = matches[i].ToString();
                    finalText = finalText.Replace(tag1, GetGoogleNoTranslateTag(finalTags.Count)+" ");
                    //finalText = finalText.Replace(tag1, /*"{[" + finalTags.Count + "]}"*/ ((char)(0x2600 + finalTags.Count)).ToString());
                    finalTags.Add(tag1);

                    var tag2 = matches[iClosingTag].ToString();
                    finalText = finalText.Replace(tag2, GetGoogleNoTranslateTag(finalTags.Count)+" ");
                    //finalText = finalText.Replace(tag2, /*"{[" + finalTags.Count + "]}"*/ ((char)(0x2600 + finalTags.Count)).ToString());
                    finalTags.Add(tag2);
                }
            }

            query.Text = finalText;
            query.Tags = finalTags.ToArray();
        }

        public static string GetQueryResult(string text, string LanguageCodeTo, TranslationDictionary dict)
        {
            if (!dict.ContainsKey(text))
                return null;

            var query = dict[text];
            if (query.Results == null || query.Results.Length < 0)
                return null;

            if (string.IsNullOrEmpty(LanguageCodeTo))
                return query.Results[0];

            int idx = Array.IndexOf(query.TargetLanguagesCode, LanguageCodeTo);
            if (idx < 0)
                return null;

            return query.Results[idx];
        }

        public static string RebuildTranslation(string text, TranslationDictionary dict, string LanguageCodeTo)
        {
            if (!text.Contains("[i2s_"))
            {
                return RebuildTranslation_Plural(text, dict, LanguageCodeTo);
            }

            var variants = SpecializationManager.GetSpecializations(text);
            var results = new Dictionary<string,string>(StringComparer.Ordinal);

            foreach (var kvp in variants)
            {
                results[kvp.Key] = RebuildTranslation_Plural(kvp.Value, dict, LanguageCodeTo);
            }
            return SpecializationManager.SetSpecializedText(results);
        }

        static string RebuildTranslation_Plural( string text, TranslationDictionary dict, string LanguageCodeTo )
		{
            bool hasPluralParams = text.Contains("{[#");
            bool hasPluralTypes = text.Contains("[i2p_");
            if (!HasParameters(text) || !hasPluralParams && !hasPluralTypes)
            {
				return GetTranslation (text, LanguageCodeTo, dict);
			}

            var sb = new StringBuilder();

            string pluralTranslation = null;
            bool forcePluralParam = hasPluralParams;


            for (var i = ePluralType.Plural; i >= 0; --i)
            {
                var pluralType = i.ToString();
                if (!GoogleLanguages.LanguageHasPluralType(LanguageCodeTo, pluralType))
                    continue;

                var newText = GetPluralText(text, pluralType);
                int testNumber = GoogleLanguages.GetPluralTestNumber(LanguageCodeTo, i);

                var parameter = GetPluralParameter(newText, forcePluralParam);
                if (!string.IsNullOrEmpty(parameter))
                    newText = newText.Replace(parameter, testNumber.ToString());

                var translation = GetTranslation(newText, LanguageCodeTo, dict);
                //Debug.LogFormat("from: {0}  ->  {1}", newText, translation);
                if (!string.IsNullOrEmpty(parameter))
                    translation = translation.Replace(testNumber.ToString(), parameter);

                if (i==ePluralType.Plural)
                {
                    pluralTranslation = translation;
                }
                else
                {
                    if (translation == pluralTranslation)
                        continue;
                    sb.AppendFormat("[i2p_{0}]", pluralType);
                }
                sb.Append(translation);
            }

 			return sb.ToString ();
		}



		public static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] a = s.ToLower().ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
		public static string TitleCase(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}

#if NETFX_CORE
			var sb = new StringBuilder(s);
			sb[0] = char.ToUpper(sb[0]);
			for (int i = 1, imax=s.Length; i<imax; ++i)
			{
				if (char.IsWhiteSpace(sb[i - 1]))
					sb[i] = char.ToUpper(sb[i]);
				else
					sb[i] = char.ToLower(sb[i]);
			}
			return sb.ToString();
#else
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
#endif
		}
	}
}

