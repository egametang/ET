using System;
using System.Collections.Generic;
using UnityEngine;

namespace I2.Loc
{
	public enum eTermType 
	{ 
		Text, Font, Texture, AudioClip, GameObject, Sprite, Material, Child, Mesh,
		#if NGUI
			UIAtlas, UIFont,
		#endif
		#if TK2D
			TK2dFont, TK2dCollection,
		#endif
		#if TextMeshPro
			TextMeshPFont,
		#endif
		#if SVG
			SVGAsset,
		#endif
		Object,
		Video
	}

	public enum TranslationFlag : byte
	{
		Normal = 1,
		AutoTranslated = 2
	}


    [Serializable]
	public class TermData
	{
		public string 			Term 			= string.Empty;
		public eTermType		TermType 		= eTermType.Text;
		
		#if !UNITY_EDITOR
		[NonSerialized]
		#endif
		public string 			Description;
		
        public string[]         Languages = Array.Empty<string>();
        public byte[]			Flags 	  = Array.Empty<byte>();  // flags for each translation

        [SerializeField] private string[] Languages_Touch;      // TO BE REMOVED IN A FUTURE RELEASE

        public string GetTranslation ( int idx, string specialization=null, bool editMode=false )
		{
            string text = Languages[idx];
            if (text != null)
            {
                text = SpecializationManager.GetSpecializedText(text, specialization);
                if (!editMode)
                {
                    text = text.Replace("[i2nt]", "").Replace("[/i2nt]", "");
                }
            }
            return text;
		}

        public void SetTranslation( int idx, string translation, string specialization = null)
        {
            Languages[idx] = SpecializationManager.SetSpecializedText( Languages[idx], translation, specialization);
        }

        public void RemoveSpecialization(string specialization)
        {
            for (int i = 0; i < Languages.Length; ++i)
                RemoveSpecialization(i, specialization);
        }


        public void RemoveSpecialization( int idx, string specialization )
        {
            var text = Languages[idx];
            if (specialization == "Any" || !text.Contains("[i2s_" + specialization + "]"))
            {
                return;
            }

            var dict = SpecializationManager.GetSpecializations(text);
            dict.Remove(specialization);
            Languages[idx] = SpecializationManager.SetSpecializedText(dict);
        }

        public bool IsAutoTranslated( int idx, bool IsTouch )
		{
			return (Flags[idx] & (byte)TranslationFlag.AutoTranslated) > 0;
		}

		public void Validate ()
		{
			int nLanguages = Mathf.Max(Languages.Length, Flags.Length);

			if (Languages.Length != nLanguages) 		Array.Resize(ref Languages, nLanguages);
			if (Flags.Length!=nLanguages) 				Array.Resize(ref Flags, nLanguages);

            if (Languages_Touch != null)
            {
                for (int i = 0; i < Mathf.Min(Languages_Touch.Length, nLanguages); ++i)
                {
                    if (string.IsNullOrEmpty(Languages[i]) && !string.IsNullOrEmpty(Languages_Touch[i]))
                    {
                        Languages[i] = Languages_Touch[i];
                        Languages_Touch[i] = null;
                    }
                }
                Languages_Touch = null;
            }
        }
        
		public bool IsTerm( string name, bool allowCategoryMistmatch)
		{
			if (!allowCategoryMistmatch)
				return name == Term;

			return name == LanguageSourceData.GetKeyFromFullTerm (Term);
		}

        public bool HasSpecializations()
        {
            for (int i = 0; i < Languages.Length; ++i)
            {
                if (!string.IsNullOrEmpty(Languages[i]) && Languages[i].Contains("[i2s_"))
                    return true;
            }
            return false;
        }

        public List<string> GetAllSpecializations()
        {
            List<string> values = new List<string>();
            for (int i = 0; i < Languages.Length; ++i)
                SpecializationManager.AppendSpecializations(Languages[i], values);
            return values;
        }
    }

    public class TermsPopup : PropertyAttribute
    {
        public TermsPopup(string filter = "")
        {
            Filter = filter;
        }

        public string Filter { get; private set; }
    }
}