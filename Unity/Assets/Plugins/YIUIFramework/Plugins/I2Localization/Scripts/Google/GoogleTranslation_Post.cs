using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace I2.Loc
{
	using TranslationDictionary = Dictionary<string, TranslationQuery>;


	public static partial class GoogleTranslation
	{
		static List<UnityWebRequest> mCurrentTranslations = new List<UnityWebRequest>();
        static List<TranslationJob> mTranslationJobs = new List<TranslationJob>();

        public delegate void fnOnTranslationReady(TranslationDictionary dict, string error);

#region Multiple Translations

		public static void Translate( TranslationDictionary requests, fnOnTranslationReady OnTranslationReady, bool usePOST = true )
		{
            //WWW www = GetTranslationWWW( requests, usePOST );
            //I2.Loc.CoroutineManager.Start(WaitForTranslation(www, OnTranslationReady, requests));
            AddTranslationJob( new TranslationJob_Main(requests, OnTranslationReady) );
        }

        public static bool ForceTranslate(TranslationDictionary requests, bool usePOST = true)
        {
            var job = new TranslationJob_Main(requests, null);
            while (true)
            {
                var state = job.GetState();
                if (state == TranslationJob.eJobState.Running)
                    continue;

                if (state == TranslationJob.eJobState.Failed)
                    return false;

                //TranslationJob.eJobState.Succeeded
                return true;
            }
        }

        public static List<string> ConvertTranslationRequest(TranslationDictionary requests, bool encodeGET)
        {
            List<string> results = new List<string>();
            var sb = new StringBuilder();

            foreach (var kvp in requests)
            {
                var request = kvp.Value;
                if (sb.Length > 0)
                    sb.Append("<I2Loc>");

                sb.Append(GoogleLanguages.GetGoogleLanguageCode(request.LanguageCode));
                sb.Append(":");
                for (int i = 0; i < request.TargetLanguagesCode.Length; ++i)
                {
                    if (i != 0) sb.Append(",");
                    sb.Append(GoogleLanguages.GetGoogleLanguageCode(request.TargetLanguagesCode[i]));
                }
                sb.Append("=");

                var text = TitleCase(request.Text) == request.Text ? request.Text.ToLowerInvariant() : request.Text;

                if (!encodeGET)
                {
                    sb.Append(text);
                }
                else
                {
                    sb.Append(Uri.EscapeDataString(text));
                    if (sb.Length > 4000)
                    {
                        results.Add(sb.ToString());
                        sb.Length = 0;
                    }
                }
            }
            results.Add(sb.ToString());
            return results;
        }

        static void AddTranslationJob( TranslationJob job )
        {
            mTranslationJobs.Add(job);
            if (mTranslationJobs.Count==1)
            {
                CoroutineManager.Start(WaitForTranslations());
            }
        }

        static IEnumerator WaitForTranslations()
        {
            while (mTranslationJobs.Count > 0)
            {
                var jobs = mTranslationJobs.ToArray();
                foreach (var job in jobs)
                {
                    if (job.GetState() != TranslationJob.eJobState.Running)
                        mTranslationJobs.Remove(job);
                }
                yield return null;
            }
        }



        public static string ParseTranslationResult( string html, TranslationDictionary requests )
		{
			//Debug.Log(html);
			// Handle google restricting the webservice to run
			if (html.StartsWith("<!DOCTYPE html>") || html.StartsWith("<HTML>"))
            {
                if (html.Contains("The script completed but did not return anything"))
                    return "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version.";
                if (html.Contains("Service invoked too many times in a short time"))
                    return ""; // ignore and try again
                return "There was a problem contacting the WebService. Please try again later\n" + html;
            }

			string[] texts = html.Split (new[]{"<I2Loc>"}, StringSplitOptions.None);
			string[] splitter = {"<i2>"};
			int i = 0;

			var Keys = requests.Keys.ToArray();
			foreach (var text in Keys)
			{
				var temp = FindQueryFromOrigText(text, requests);
                var fullText = texts[i++];
                if (temp.Tags != null)
                {
                    //for (int j = 0, jmax = temp.Tags.Length; j < jmax; ++j)
                    for (int j = temp.Tags.Length-1; j>=0; --j)
                    {
                            fullText = fullText.Replace(GetGoogleNoTranslateTag(j), temp.Tags[j]);
                        //fullText = fullText.Replace(  /*"{[" + j + "]}"*/ ((char)(0x2600+j)).ToString(), temp.Tags[j]);
                    }
                }

                temp.Results = fullText.Split (splitter, StringSplitOptions.None);

				// Google has problem translating this "This Is An Example"  but not this "this is an example"
				if (TitleCase(text)==text)
				{
					for (int j=0; j<temp.Results.Length; ++j)
						temp.Results[j] = TitleCase(temp.Results[j]);
				}
				requests[temp.OrigText] = temp;
			}
			return null;
		}


		public static bool IsTranslating()
		{
			return mCurrentTranslations.Count>0 || mTranslationJobs.Count > 0;
		}

		public static void CancelCurrentGoogleTranslations()
		{
			mCurrentTranslations.Clear ();
            foreach (var job in mTranslationJobs)
            {
                job.Dispose();
            }
            mTranslationJobs.Clear();
		}

#endregion
	}
}

