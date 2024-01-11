using System;
using System.Collections.Generic;

namespace I2.Loc
{
    public class BaseSpecializationManager
    {
        public string[] mSpecializations;
        public Dictionary<string, string> mSpecializationsFallbacks;

        public virtual void InitializeSpecializations()
        {
            mSpecializations = new[] { "Any", "PC", "Touch", "Controller", "VR",
                                              "XBox", "PS4", "OculusVR", "ViveVR", "GearVR", "Android", "IOS" };
            mSpecializationsFallbacks = new Dictionary<string, string>(System.StringComparer.Ordinal)
            {
                { "XBox", "Controller" }, { "PS4", "Controller" },
                { "OculusVR", "VR" },   { "ViveVR", "VR" }, { "GearVR", "VR" },
                { "Android", "Touch" }, { "IOS", "Touch" }
            };
        }

        public virtual string GetCurrentSpecialization()
        {
            if (mSpecializations == null)
                InitializeSpecializations();

            #if UNITY_ANDROID
                return "Android";
            #elif UNITY_IOS
                return "IOS";
            #elif UNITY_PS4
                return "PS4";
            #elif UNITY_XBOXONE
                return "XBox";
            #elif UNITY_STANDALONE || UNITY_WEBGL
                return "PC";
            #else
                return (Input.touchSupported ? "Touch" : "PC");
            #endif
        }

        public virtual string GetFallbackSpecialization(string specialization)
        {
            if (mSpecializationsFallbacks == null)
                InitializeSpecializations();

            string fallback;
            if (mSpecializationsFallbacks.TryGetValue(specialization, out fallback))
                return fallback;
            return "Any";
        }
    }
    public class SpecializationManager : BaseSpecializationManager
    {
        public static SpecializationManager Singleton = new SpecializationManager();

        private SpecializationManager()
        {
            InitializeSpecializations();
        }

        public static string GetSpecializedText(string text, string specialization = null)
        {
            var idxFirst = text.IndexOf("[i2s_", StringComparison.Ordinal);
            if (idxFirst < 0)
                return text;

            if (string.IsNullOrEmpty(specialization))
                specialization = Singleton.GetCurrentSpecialization();

            while (!string.IsNullOrEmpty(specialization) && specialization != "Any")
            {
                var tag = "[i2s_" + specialization + "]";
                int idx = text.IndexOf(tag, StringComparison.Ordinal);
                if (idx < 0)
                {
                    specialization = Singleton.GetFallbackSpecialization(specialization);
                    continue;
                }

                idx += tag.Length;
                var idxEnd = text.IndexOf("[i2s_", idx, StringComparison.Ordinal);
                if (idxEnd < 0) idxEnd = text.Length;

                return text.Substring(idx, idxEnd - idx);
            }

            return text.Substring(0, idxFirst);
        }

        public static string SetSpecializedText(string text, string newText, string specialization)
        {
            if (string.IsNullOrEmpty(specialization))
                specialization = "Any";
            if ((text==null || !text.Contains("[i2s_")) && specialization=="Any")
            {
                return newText;
            }

            var dict = GetSpecializations(text);
            dict[specialization] = newText;

            return SetSpecializedText(dict);
        }

        public static string SetSpecializedText( Dictionary<string,string> specializations )
        {
            string text;
            if (!specializations.TryGetValue("Any", out text))
                text = string.Empty;

            foreach (var kvp in specializations)
            {
                if (kvp.Key != "Any" && !string.IsNullOrEmpty(kvp.Value))
                    text += "[i2s_" + kvp.Key + "]" + kvp.Value;
            }
            return text;
        }

        public static Dictionary<string, string> GetSpecializations(string text, Dictionary<string, string> buffer = null)
        {
            if (buffer == null)
                buffer = new Dictionary<string, string>(StringComparer.Ordinal);
            else
                buffer.Clear();

            if (text==null)
            {
                buffer["Any"] = "";
                return buffer;
            }

            var idxFirst = 0;
            var idxEnd = text.IndexOf("[i2s_", StringComparison.Ordinal);
            if (idxEnd < 0)
                idxEnd=text.Length;

            buffer["Any"] = text.Substring(0, idxEnd);
            idxFirst = idxEnd;

            while (idxFirst<text.Length)
            {
                idxFirst += "[i2s_".Length;
                int idx = text.IndexOf(']', idxFirst);
                if (idx < 0) break;
                var tag = text.Substring(idxFirst, idx - idxFirst);
                idxFirst = idx+1; // ']'

                idxEnd = text.IndexOf("[i2s_", idxFirst, StringComparison.Ordinal);
                if (idxEnd < 0) idxEnd = text.Length;
                var value = text.Substring(idxFirst, idxEnd - idxFirst);

                buffer[tag] = value;
                idxFirst = idxEnd;
            }
            return buffer;
        }
        public static void AppendSpecializations(string text, List<string> list=null)
        {
            if (text == null)
                return;

            if (list == null)
                list = new List<string>();

            if (!list.Contains("Any"))
                list.Add("Any");

            var idxFirst = 0;
            while (idxFirst<text.Length)
            {
                idxFirst = text.IndexOf("[i2s_", idxFirst, StringComparison.Ordinal);
                if (idxFirst < 0)
                    break;

                idxFirst += "[i2s_".Length;
                int idx = text.IndexOf(']', idxFirst);
                if (idx < 0)
                    break;

                var tag = text.Substring(idxFirst, idx - idxFirst);
                if (!list.Contains(tag))
                    list.Add(tag);
            }
        }
    }
}