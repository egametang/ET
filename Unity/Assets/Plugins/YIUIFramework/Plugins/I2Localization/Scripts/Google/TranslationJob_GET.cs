using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace I2.Loc
{
    using TranslationDictionary = Dictionary<string, TranslationQuery>;

    public class TranslationJob_GET : TranslationJob_WWW
    {
        TranslationDictionary _requests;
        GoogleTranslation.fnOnTranslationReady _OnTranslationReady;
        List<string> mQueries;
        public string mErrorMessage;

        public TranslationJob_GET(TranslationDictionary requests, GoogleTranslation.fnOnTranslationReady OnTranslationReady)
        {
            _requests = requests;
            _OnTranslationReady = OnTranslationReady;

            mQueries = GoogleTranslation.ConvertTranslationRequest(requests, true);

            GetState();
        }

        void ExecuteNextQuery()
        {
            if (mQueries.Count == 0)
            {
                mJobState = eJobState.Succeeded;
                return;
            }

            int lastQuery = mQueries.Count - 1;
            var query = mQueries[lastQuery];
            mQueries.RemoveAt(lastQuery);

            string url = $"{LocalizationManager.GetWebServiceURL()}?action=Translate&list={query}";
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

            if (www==null)
            {
                ExecuteNextQuery();
            }

            return mJobState;
        }

        public void ProcessResult(byte[] bytes, string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg))
            {
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                errorMsg = GoogleTranslation.ParseTranslationResult(wwwText, _requests);

                if (string.IsNullOrEmpty(errorMsg))
                {
                    if (_OnTranslationReady!=null)
                        _OnTranslationReady(_requests, null);
                    return;
                }
            }

            mJobState = eJobState.Failed;
            mErrorMessage = errorMsg;
        }
    }
}