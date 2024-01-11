using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
    [CustomPropertyDrawer (typeof (TermsPopup))]
	public class TermsPopup_Drawer : PropertyDrawer 
	{
        GUIContent[] mTerms_Context;
        int nFramesLeftBeforeUpdate;
        string mPrevFilter;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var filter = ((TermsPopup)attribute).Filter;
            ShowGUICached(position, property, label, null, filter, ref mTerms_Context, ref nFramesLeftBeforeUpdate, ref mPrevFilter);
        }

        public static bool ShowGUI(Rect position, SerializedProperty property, GUIContent label, LanguageSourceData source, string filter = "")
        {
            GUIContent[] terms=null;
            int framesLeftBeforeUpdate=0;
            string prevFilter = null;

            return ShowGUICached(position, property, label, source, filter, ref terms, ref framesLeftBeforeUpdate, ref prevFilter);
        }

        public static bool ShowGUICached(Rect position, SerializedProperty property, GUIContent label, LanguageSourceData source, string filter, ref GUIContent[] terms_Contexts, ref int framesBeforeUpdating, ref string prevFilter)
		{
            UpdateTermsCache(source, filter, ref terms_Contexts, ref framesBeforeUpdating, ref prevFilter);

            label = EditorGUI.BeginProperty(position, label, property);

			EditorGUI.BeginChangeCheck ();

            var index = property.stringValue == "-" || property.stringValue == "" ? terms_Contexts.Length - 1 : 
                property.stringValue == " " ? terms_Contexts.Length - 2 :
                GetTermIndex(terms_Contexts, property.stringValue);
            var newIndex = EditorGUI.Popup(position, label, index, terms_Contexts);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = newIndex < 0 || newIndex == terms_Contexts.Length - 1 ? string.Empty : terms_Contexts[newIndex].text;
                if (newIndex == terms_Contexts.Length - 1)
                    property.stringValue = "-";
                else
                if (newIndex < 0 || newIndex == terms_Contexts.Length - 2)
                    property.stringValue = string.Empty;
                else
                    property.stringValue = terms_Contexts[newIndex].text;

                EditorGUI.EndProperty();
                return true;
            }

            EditorGUI.EndProperty();
            return false;
		}

        static int GetTermIndex(GUIContent[] terms_Contexts, string term )
        {
            for (int i = 0; i < terms_Contexts.Length; ++i)
                if (terms_Contexts[i].text == term)
                    return i;
            return -1;
        }


        static void UpdateTermsCache(LanguageSourceData source, string filter, ref GUIContent[] terms_Contexts, ref int framesBeforeUpdating, ref string prevFilter)
        {
            framesBeforeUpdating--;
            if (terms_Contexts!=null && framesBeforeUpdating>0 && filter==prevFilter)
            {
                return;
            }
            framesBeforeUpdating = 60;
            prevFilter = filter;

            var Terms = source == null ? LocalizationManager.GetTermsList() : source.GetTermsList();

            if (string.IsNullOrEmpty(filter) == false)
            {
                Terms = Filter(Terms, filter);
            }

            Terms.Sort(StringComparer.OrdinalIgnoreCase);
            Terms.Add("");
            Terms.Add("<inferred from text>");
            Terms.Add("<none>");

            terms_Contexts = DisplayOptions(Terms);
        }

        private static List<string> Filter(List<string> terms, string filter)
        {
            var filtered = new List<string>();
            for (var i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                if (term.Contains(filter))
                {
                    filtered.Add(term);
                }
            }

            return filtered;
        }

        private static GUIContent[] DisplayOptions(IList<string> terms)
        {
            var options = new GUIContent[terms.Count];
            for (var i = 0; i < terms.Count; i++)
            {
                options[i] = new GUIContent(terms[i]);
            }

            return options;
        }
	}

    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringDrawer : PropertyDrawer
    {
        GUIContent[] mTerms_Context;
        int nFramesLeftBeforeUpdate;
        string mPrevFilter;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var termRect = rect;    termRect.xMax -= 50;
            var termProp = property.FindPropertyRelative("mTerm");
            TermsPopup_Drawer.ShowGUICached(termRect, termProp, label, null, "", ref mTerms_Context, ref nFramesLeftBeforeUpdate, ref mPrevFilter);

            var maskRect = rect;    maskRect.xMin = maskRect.xMax - 30;
            var termIgnoreRTL       = property.FindPropertyRelative("mRTL_IgnoreArabicFix");
            var termConvertNumbers = property.FindPropertyRelative("mRTL_ConvertNumbers");
            var termDontLocalizeParams = property.FindPropertyRelative("m_DontLocalizeParameters");
            int mask = (termIgnoreRTL.boolValue ? 0 : 1) + 
                       (termConvertNumbers.boolValue ? 0 : 2) +
                       (termDontLocalizeParams.boolValue ? 0 : 4);

            int newMask = EditorGUI.MaskField(maskRect, mask, new[] { "Arabic Fix", "Ignore Numbers in RTL", "Localize Parameters" });
            if (newMask != mask)
            {
                termIgnoreRTL.boolValue      = (newMask & 1) == 0;
                termConvertNumbers.boolValue = (newMask & 2) == 0;
                termDontLocalizeParams.boolValue = (newMask & 4) == 0;
            }

            var showRect = rect;    showRect.xMin = termRect.xMax; showRect.xMax=maskRect.xMin;
			bool enabled = GUI.enabled;
			GUI.enabled = enabled & (!string.IsNullOrEmpty (termProp.stringValue) && termProp.stringValue!="-");
			if (GUI.Button (showRect, "?")) 
			{
				var source = LocalizationManager.GetSourceContaining(termProp.stringValue);
				LocalizationEditor.mKeyToExplore = termProp.stringValue;
				Selection.activeObject = source.ownerObject;
			}
			GUI.enabled = enabled;
        }
    }
}
