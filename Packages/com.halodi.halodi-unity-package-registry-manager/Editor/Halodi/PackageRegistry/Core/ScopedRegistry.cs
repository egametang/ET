using System;
using System.Collections.Generic;
using UnityEngine;

namespace Halodi.PackageRegistry.Core
{
    [System.Serializable]
    public class ScopedRegistry
    {
        public string name;
        public string url;
        public List<string> scopes = new List<string>();

        public bool auth;

        public string token;

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }

        public bool isValidCredential()
        {

            if( string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return false;
            }


            if(auth)
            {
                if(string.IsNullOrEmpty(token))
                {
                    return false;
                }
            }

            return true;
        }

        public bool isValid()
        {
            if(string.IsNullOrEmpty(name))
            {
                return false;
            }

            if(scopes.Count < 1)
            {
                return false;
            }

            scopes.RemoveAll(string.IsNullOrEmpty);
            
            foreach(string scope in scopes)
            {
                if(Uri.CheckHostName(scope) != UriHostNameType.Dns)
                {
                    Debug.LogWarning("Invalid scope " + scope);
                    return false;
                }
            }


            return isValidCredential();


        }
    }


}