using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace I2.Loc
{
    using TranslationDictionary = Dictionary<string, TranslationQuery>;


    public class TranslationJob : IDisposable
    {
        public eJobState mJobState = eJobState.Running;

        public enum eJobState { Running, Succeeded, Failed }

        public virtual eJobState GetState() { return mJobState; }

        public virtual void Dispose() { }

    }

    public class TranslationJob_WWW : TranslationJob
    {
        public UnityWebRequest www;

        public override void Dispose()
        {
            if (www!=null)
                www.Dispose();
            www = null;
        }

    }
}