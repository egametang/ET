using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace I2.Loc
{
    using TranslationDictionary = Dictionary<string, TranslationQuery>;

    public class TranslationJob_WEB : TranslationJob_WWW
    {
        TranslationDictionary _requests;
        GoogleTranslation.fnOnTranslationReady _OnTranslationReady;
        public string mErrorMessage;

        string mCurrentBatch_ToLanguageCode;
        string mCurrentBatch_FromLanguageCode;
        List<string> mCurrentBatch_Text;

        List<KeyValuePair<string, string>> mQueries;

        public TranslationJob_WEB(TranslationDictionary requests, GoogleTranslation.fnOnTranslationReady OnTranslationReady)
        {
            _requests = requests;
            _OnTranslationReady = OnTranslationReady;

            FindAllQueries();
            ExecuteNextBatch();
        }

        void FindAllQueries()
        {
            mQueries = new List<KeyValuePair<string, string>>();
            foreach (var kvp in _requests)
            {
                foreach (var langCode in kvp.Value.TargetLanguagesCode)
                {
                    mQueries.Add(new KeyValuePair<string, string>(kvp.Value.OrigText, kvp.Value.LanguageCode+":"+langCode));
                }
            }

            mQueries.Sort((a, b) => a.Value.CompareTo(b.Value));
        }

        void ExecuteNextBatch()
        {
            if (mQueries.Count==0)
            {
                mJobState = eJobState.Succeeded;
                return;
            }
            mCurrentBatch_Text = new List<string>();

            string lastLangCode = null;
            int maxLength = 200;

            var sb = new StringBuilder();
            int i;
            for (i=0; i<mQueries.Count; ++i)
            {
                var text = mQueries[i].Key;
                var langCode = mQueries[i].Value;

                if (lastLangCode==null || langCode==lastLangCode)
                {
                    if (i != 0)
                        sb.Append("|||");
                    sb.Append(text);

                    mCurrentBatch_Text.Add(text);
                    lastLangCode = langCode;
                }
                if (sb.Length > maxLength)
                    break;
            }
            mQueries.RemoveRange(0, i);

            var fromtoLang = lastLangCode.Split(':');
            mCurrentBatch_FromLanguageCode = fromtoLang[0];
            mCurrentBatch_ToLanguageCode = fromtoLang[1];

            string url = string.Format ("http://www.google.com/translate_t?hl=en&vi=c&ie=UTF8&oe=UTF8&submit=Translate&langpair={0}|{1}&text={2}", mCurrentBatch_FromLanguageCode, mCurrentBatch_ToLanguageCode, Uri.EscapeUriString( sb.ToString() ));
            Debug.Log(url);

            www = UnityWebRequest.Get(url);
            I2Utils.SendWebRequest(www);
        }

        public override eJobState GetState()
        {
            if (www != null && www.isDone)
            {
                ProcessResult(www.downloadHandler.data, www.error);
                www.Dispose();
                www = null;
            }

            if (www == null)
            {
                ExecuteNextBatch();
            }

            return mJobState;
        }

        public void ProcessResult(byte[] bytes, string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
            {
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                var result = ParseTranslationResult(wwwText, "aab");
                //errorMsg = GoogleTranslation.ParseTranslationResult(wwwText, _requests);
                Debug.Log(result);

                if (string.IsNullOrEmpty(errorMsg))
                {
                    if (_OnTranslationReady != null)
                        _OnTranslationReady(_requests, null);
                    return;
                }
            }
            
            mJobState = eJobState.Failed;
            mErrorMessage = errorMsg;
        }

        string ParseTranslationResult( string html, string OriginalText )
        {
            try
            {
                // This is a Hack for reading Google Translation while Google doens't change their response format
                int iStart = html.IndexOf("TRANSLATED_TEXT='", StringComparison.Ordinal) + "TRANSLATED_TEXT='".Length;
                int iEnd = html.IndexOf("';var", iStart, StringComparison.Ordinal);

                string Translation = html.Substring( iStart, iEnd-iStart);

                // Convert to normalized HTML
                Translation = Regex.Replace(Translation, @"\\x([a-fA-F0-9]{2})",
                                                            match => char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)));

                // Convert ASCII Characters
                Translation = Regex.Replace(Translation, @"&#(\d+);",
                                                            match => char.ConvertFromUtf32(int.Parse(match.Groups[1].Value)));

                Translation = Translation.Replace("<br>", "\n");

                if (OriginalText.ToUpper()==OriginalText)
                    Translation = Translation.ToUpper();
                else
                    if (GoogleTranslation.UppercaseFirst(OriginalText)==OriginalText)
                        Translation = GoogleTranslation.UppercaseFirst(Translation);
                else
                    if (GoogleTranslation.TitleCase(OriginalText)==OriginalText)
                        Translation = GoogleTranslation.TitleCase(Translation);

                return Translation;
            }
            catch (Exception ex) 
            { 
                Debug.LogError(ex.Message); 
                return string.Empty;
            }
        }
    }

 }