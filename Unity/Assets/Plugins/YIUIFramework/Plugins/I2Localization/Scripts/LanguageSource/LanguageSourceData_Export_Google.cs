using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace I2.Loc
{
	public enum eSpreadsheetUpdateMode { None, Replace, Merge, AddNewTerms }

	public partial class LanguageSourceData
	{
		public UnityWebRequest Export_Google_CreateWWWcall( eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace )
		{
            #if UNITY_WEBPLAYER
			Debug.Log ("Contacting google translation is not yet supported on WebPlayer" );
			return null;
#else
            string Data = Export_Google_CreateData();

			WWWForm form = new WWWForm();
			form.AddField("key", Google_SpreadsheetKey);
			form.AddField("action", "SetLanguageSource");
			form.AddField("data", Data);
			form.AddField("updateMode", UpdateMode.ToString());

            #if UNITY_EDITOR
            form.AddField("password", Google_Password);
#endif


            UnityWebRequest www = UnityWebRequest.Post(LocalizationManager.GetWebServiceURL(this), form);
            I2Utils.SendWebRequest(www);
            return www;
			#endif
		}

		string Export_Google_CreateData()
		{
			List<string> Categories = GetCategories(true);
			StringBuilder Builder = new StringBuilder();
			
			bool bFirst = true;
			foreach (string category in Categories)
			{
				if (bFirst)
					bFirst = false;
				else
					Builder.Append("<I2Loc>");

                #if !UNITY_EDITOR
                    bool Spreadsheet_SpecializationAsRows = true;
                #endif

                string CSV = Export_I2CSV(category, specializationsAsRows:Spreadsheet_SpecializationAsRows);
				Builder.Append(category);
				Builder.Append("<I2Loc>");
				Builder.Append(CSV);
			}
			return Builder.ToString();
		}
	}
}