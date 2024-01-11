using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {
        #region Variables: Misc

        public static List<ILocalizationParamsManager> ParamManagers = new List<ILocalizationParamsManager>();

        
        // returns true if this replaces the normal ApplyLocalizationParams
        // returns false if after running this function the manager should also run the default ApplyLocalizationParams to replace parameters 
        public delegate bool FnCustomApplyLocalizationParams(ref string translation, _GetParam getParam, bool allowLocalizedParameters);
        public static FnCustomApplyLocalizationParams CustomApplyLocalizationParams;
        #endregion

        #region Parameters

        public delegate object _GetParam(string param);

        public static void AutoLoadGlobalParamManagers()
        {
            foreach (var manager in Object.FindObjectsOfType<LocalizationParamsManager>())
            {
                if (manager._IsGlobalManager && !ParamManagers.Contains(manager))
                {
                    Debug.Log(manager);
                    ParamManagers.Add(manager);
                }
            }
        }

        public static void ApplyLocalizationParams(ref string translation, bool allowLocalizedParameters = true)
        {
            ApplyLocalizationParams(ref translation, p => GetLocalizationParam(p, null), allowLocalizedParameters);
        }


        public static void ApplyLocalizationParams(ref string translation, GameObject root, bool allowLocalizedParameters = true)
        {
            ApplyLocalizationParams(ref translation, p => GetLocalizationParam(p, root), allowLocalizedParameters);
        }

        public static void ApplyLocalizationParams(ref string translation, Dictionary<string, object> parameters, bool allowLocalizedParameters = true)
        {
            ApplyLocalizationParams(ref translation, p => {
                    object o = null;
                    if (parameters.TryGetValue(p, out o))
                        return o;
                    return null;
                }, allowLocalizedParameters);
        }


        public static void ApplyLocalizationParams(ref string translation, _GetParam getParam, bool allowLocalizedParameters=true)
        {
            if (translation == null)
                return;

            bool skip_processing = CustomApplyLocalizationParams!=null && CustomApplyLocalizationParams.Invoke(ref translation, getParam, allowLocalizedParameters);
            if (skip_processing) return;
            
            string pluralType=null;
            int idx0 = 0;
            int idx1 = translation.Length;

            int index = 0;
            while (index>=0 && index<translation.Length)
            {
                int iParamStart = translation.IndexOf("{[", index, StringComparison.Ordinal);
                if (iParamStart < 0) break;

                int iParamEnd = translation.IndexOf("]}", iParamStart, StringComparison.Ordinal);
                if (iParamEnd < 0) break;

                // there is a sub param, so, skip this one:   "this {[helo{[hi]} end"
                int isubParam = translation.IndexOf("{[", iParamStart+1, StringComparison.Ordinal);
                if (isubParam>0 && isubParam<iParamEnd)
                {
                    index = isubParam;
                    continue;
                }

                // Check that some plural parameters can have the form: {[#name]}
                var offset = translation[iParamStart + 2] == '#' ? 3 : 2;
                var param = translation.Substring(iParamStart + offset, iParamEnd - iParamStart - offset);
                var result = (string)getParam(param);
                if (result != null)
                {
                    if (allowLocalizedParameters)
                    {
                        // check if Param is Localized
                        LanguageSourceData source;
                        var termData = GetTermData(result, out source);
                        if (termData != null)
                        {
                            int idx = source.GetLanguageIndex(CurrentLanguage);
                            if (idx >= 0)
                            {
                                result = termData.GetTranslation(idx);
                            }
                        }
                    }

                    var paramTag = translation.Substring(iParamStart, iParamEnd - iParamStart + 2);
                    translation = translation.Replace(paramTag, result);

                    int amount = 0;
                    if (int.TryParse(result, out amount))
                    {
                        pluralType = GoogleLanguages.GetPluralType(CurrentLanguageCode, amount).ToString();
                    }

                    index = iParamStart + result.Length;
                }
                else
                {
                    index = iParamEnd + 2;
                }
            }

            if (pluralType != null)
            {
                var tag = "[i2p_" + pluralType + "]";
                idx0 = translation.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
                if (idx0 < 0) idx0 = 0;
                else idx0 += tag.Length;

                idx1 = translation.IndexOf("[i2p_", idx0 + 1, StringComparison.OrdinalIgnoreCase);
                if (idx1 < 0) idx1 = translation.Length;

                translation = translation.Substring(idx0, idx1 - idx0);
            }
        }

        internal static string GetLocalizationParam(string ParamName, GameObject root)
        {
            string result = null;
            if (root)
            {
                var components = root.GetComponents<MonoBehaviour>();
                for (int i = 0, imax = components.Length; i < imax; ++i)
                {
                    var manager = components[i] as ILocalizationParamsManager;
                    if (manager != null && components[i].enabled)
                    {
                        result = manager.GetParameterValue(ParamName);
                        if (result != null)
                            return result;
                    }
                }
            }

            for (int i = 0, imax = ParamManagers.Count; i < imax; ++i)
            {
                result = ParamManagers[i].GetParameterValue(ParamName);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion

        #region Plural

        private static string GetPluralType( MatchCollection matches, string langCode, _GetParam getParam)
		{
			for (int i = 0, nMatches = matches.Count; i < nMatches; ++i)
			{
				var match = matches[i];
				var param = match.Groups[match.Groups.Count - 1].Value;
				var result = (string)getParam(param);
				if (result == null)
					continue;
				
				int amount = 0;
				if (!int.TryParse (result, out amount))
					continue;

				var pluralType = GoogleLanguages.GetPluralType(langCode, amount);
				return pluralType.ToString ();
			}
			return null;
		}

        #endregion
    }
}
