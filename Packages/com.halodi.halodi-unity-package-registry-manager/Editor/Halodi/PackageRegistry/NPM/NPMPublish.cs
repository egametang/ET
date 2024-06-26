using System;
using System.IO;
using System.Net;
using System.Text;
using Halodi.PackageRegistry.Core;
using UnityEngine;

namespace Halodi.PackageRegistry.NPM
{
public class NPMPublish
{
        public static void Publish(string packageFolder, string registry)
        {
            CredentialManager manager = new CredentialManager();
            if (!manager.HasRegistry(registry))
            {
                throw new System.IO.IOException("Credentials not set for registry " + registry);
            }

            string token = manager.GetCredential(registry).token;

            PublicationManifest manifest = new PublicationManifest(packageFolder, registry); ;




            using (var client = new ExpectContinueAware())
            {
                string upload = NPMLogin.UrlCombine(registry, manifest.name);


                client.Encoding = Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);


                // Headers set by the NPM client, but not by us. Option to try with compatibility issues.
                
                // client.Headers.Add("npm-in-ci", "false");
                // client.Headers.Add("npm-scope", "");
                // client.Headers.Add(HttpRequestHeader.UserAgent, "npm/6.14.4 node/v12.16.2 linux x64");
                // var random = new Random();
                // string a = String.Format("{0:X8}", random.Next(0x10000000, int.MaxValue)).ToLower();
                // string b = String.Format("{0:X8}", random.Next(0x10000000, int.MaxValue)).ToLower();

                // client.Headers.Add("npm-session", a + b);
                // client.Headers.Add("referer", "publish");


                try
                {
                    string responseString = client.UploadString(upload, WebRequestMethods.Http.Put, manifest.Request);

                    try
                    {
                        NPMResponse response = JsonUtility.FromJson<NPMResponse>(responseString);
                        if (!response.success && string.IsNullOrEmpty(response.ok))
                        {
                            throw new System.IO.IOException(responseString);
                        }
                    }
                    catch (Exception)
                    {
                        throw new System.IO.IOException(responseString);
                    }


                }
                catch (WebException e)
                {
                    throw new System.IO.IOException(WebExceptionParser.ParseWebException(e));
                }
            }

        }


}
}