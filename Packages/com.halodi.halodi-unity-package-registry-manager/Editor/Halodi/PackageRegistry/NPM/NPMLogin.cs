
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace Halodi.PackageRegistry.NPM
{
    [System.Serializable]
    internal class NPMLoginRequest
    {
        public string name;
        public string password;
    }


    public class ExpectContinueAware : System.Net.WebClient
    {
        protected override System.Net.WebRequest GetWebRequest(Uri address)
        {
            System.Net.WebRequest request = base.GetWebRequest(address);
            if (request is System.Net.HttpWebRequest)
            {
                var hwr = request as System.Net.HttpWebRequest;
                hwr.ServicePoint.Expect100Continue = false;
                hwr.AllowAutoRedirect = false;
            }
            return request;
        }
    }


    public class NPMLogin
    {
        internal static string UrlCombine(string start, string more)
        {
            if (string.IsNullOrEmpty(start))
            {
                return more;
            }
            else if (string.IsNullOrEmpty(more))
            {
                return start;
            }

            return start.TrimEnd('/') + "/" + more.TrimStart('/');
        }

        public static string GetBintrayToken(string user, string apiKey)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + apiKey));
        }

        public static NPMResponse GetLoginToken(string url, string user, string password)
        {
            using (var client = new WebClient())
            {
                string loginUri = UrlCombine(url, "/-/user/org.couchdb.user:" + user);
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + password)));

                NPMLoginRequest request = new NPMLoginRequest();
                request.name = user;
                request.password = password;

                string requestString = JsonUtility.ToJson(request);

                try
                {
                    string responseString = client.UploadString(loginUri, WebRequestMethods.Http.Put, requestString);
                    NPMResponse response = JsonUtility.FromJson<NPMResponse>(responseString);
                    return response;
                }
                catch (WebException e)
                {
                    NPMResponse response = new NPMResponse();
                    response.error = WebExceptionParser.ParseWebException(e);
                    return response;

                }
            }
        }




    }

}
