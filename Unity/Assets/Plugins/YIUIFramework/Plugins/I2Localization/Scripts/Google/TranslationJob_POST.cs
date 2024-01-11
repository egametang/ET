using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace I2.Loc
{
    using TranslationDictionary = Dictionary<string, TranslationQuery>;

    public class TranslationJob_POST : TranslationJob_WWW
    {
        TranslationDictionary _requests;
        GoogleTranslation.fnOnTranslationReady _OnTranslationReady;

        public TranslationJob_POST(TranslationDictionary requests, GoogleTranslation.fnOnTranslationReady OnTranslationReady)
        {
            _requests = requests;
            _OnTranslationReady = OnTranslationReady;

            var data = GoogleTranslation.ConvertTranslationRequest(requests, false);

            WWWForm form = new WWWForm();
            form.AddField("action", "Translate");
            form.AddField("list", data[0]);

            www = UnityWebRequest.Post(LocalizationManager.GetWebServiceURL(), form);
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

            return mJobState;
        }

        public void ProcessResult(byte[] bytes, string errorMsg)
        {
            if (!string.IsNullOrEmpty(errorMsg))
            {
                // check for 
                //if (errorMsg.Contains("rewind"))  // "necessary data rewind wasn't possible"
                mJobState = eJobState.Failed;                    
            }
            else
            {
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                errorMsg = GoogleTranslation.ParseTranslationResult(wwwText, _requests);
                if (_OnTranslationReady!=null)
                    _OnTranslationReady(_requests, errorMsg);
                mJobState = eJobState.Succeeded;
            }
        }
    }
}