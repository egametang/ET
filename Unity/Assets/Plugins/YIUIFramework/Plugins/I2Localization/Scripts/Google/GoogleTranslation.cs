using System.Collections.Generic;

namespace I2.Loc
{
	using TranslationDictionary = Dictionary<string, TranslationQuery>;

	public static partial class GoogleTranslation
	{
        public delegate void fnOnTranslated(string Translation, string Error);

        public static bool CanTranslate ()
		{
			return LocalizationManager.Sources.Count > 0 && 
			       !string.IsNullOrEmpty (LocalizationManager.GetWebServiceURL());
		}


        // LanguageCodeFrom can be "auto"
        // After the translation is returned from Google, it will call OnTranslationReady(TranslationResult, ErrorMsg)
        // TranslationResult will be null if translation failed
        public static void Translate( string text, string LanguageCodeFrom, string LanguageCodeTo, fnOnTranslated OnTranslationReady )
		{
            LocalizationManager.InitializeIfNeeded();
            if (!CanTranslate())
            {
                OnTranslationReady(null, "WebService is not set correctly or needs to be reinstalled");
                return;
            }
            //LanguageCodeTo = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeTo);

            if (LanguageCodeTo==LanguageCodeFrom)
            {
                OnTranslationReady(text, null);
                return;
            }

            TranslationDictionary queries = new TranslationDictionary(System.StringComparer.Ordinal);


            // Unsupported language
            if (string.IsNullOrEmpty(LanguageCodeTo))
            {
                OnTranslationReady(string.Empty, null);
                return;
            }
            CreateQueries(text, LanguageCodeFrom, LanguageCodeTo, queries);   // can split plurals and specializations into several queries

			Translate(queries, (results,error)=>
			{
					if (!string.IsNullOrEmpty(error) || results.Count==0)
					{
						OnTranslationReady(null, error);
						return;
					}

					string result = RebuildTranslation( text, queries, LanguageCodeTo);				// gets the result from google and rebuilds the text from multiple queries if its is plurals
					OnTranslationReady( result, null );
			});
		}

        // Query google for the translation and waits until google returns
        // On some Unity versions (e.g. 2017.1f1) unity doesn't handle well waiting for WWW in the main thread, so this call can fail
        // In those cases, its advisable to use the Async version  (GoogleTranslation.Translate(....))
        public static string ForceTranslate ( string text, string LanguageCodeFrom, string LanguageCodeTo )
        {
            TranslationDictionary dict = new TranslationDictionary(System.StringComparer.Ordinal);
            AddQuery(text, LanguageCodeFrom, LanguageCodeTo, dict);

            var job = new TranslationJob_Main(dict, null);
            while (true)
            {
                var state = job.GetState();
                if (state == TranslationJob.eJobState.Running)
                    continue;

                if (state == TranslationJob.eJobState.Failed)
                    return null;

                //TranslationJob.eJobState.Succeeded
                return GetQueryResult( text, "", dict);
            }
        }

	}
}

