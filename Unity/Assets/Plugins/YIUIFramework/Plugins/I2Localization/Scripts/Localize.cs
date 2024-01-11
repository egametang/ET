using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor.Events;
using UnityEditor;
#endif

namespace I2.Loc
{
    [AddComponentMenu("I2/Localization/I2 Localize")]
    public class Localize : MonoBehaviour
    {
        #region Variables: Term
        public string Term
        {
            get { return mTerm; }
            set { SetTerm(value); }
        }
        public string SecondaryTerm
        {
            get { return mTermSecondary; }
            set { SetTerm(null, value); }
        }

        public string mTerm = string.Empty,           // if Target is a Label, this will be the text,  if sprite, this will be the spriteName, etc
                      mTermSecondary = string.Empty; // if Target is a Label, this will be the font Name,  if sprite, this will be the Atlas name, etc

        // This are the terms actually used (will be mTerm/mSecondaryTerm or will get them from the objects if those are missing. e.g. Labels' text and font name)
        // This are set when the component starts
        [NonSerialized] public string FinalTerm, FinalSecondaryTerm;

        public enum TermModification { DontModify, ToUpper, ToLower, ToUpperFirst, ToTitle/*, CustomRange*/}
        public TermModification PrimaryTermModifier = TermModification.DontModify,
                                SecondaryTermModifier = TermModification.DontModify;
        public string TermPrefix, TermSuffix;

        public bool LocalizeOnAwake = true;

        string LastLocalizedLanguage;   // Used to avoid Localizing everytime the object is Enabled

#if UNITY_EDITOR
        public ILanguageSource Source;   // Source used while in the Editor to preview the Terms (can be of type LanguageSource or LanguageSourceAsset)
#endif

        #endregion

        #region Variables: Target

        public bool IgnoreRTL;	// If false, no Right To Left processing will be done
		public int  MaxCharactersInRTL;     // If the language is RTL, the translation will be split in lines not longer than this amount and the RTL fix will be applied per line
		public bool IgnoreNumbersInRTL = true; // If the language is RTL, the translation will not convert numbers (will preserve them like: e.g. 123)

		public bool CorrectAlignmentForRTL = true;	// If true, when Right To Left language, alignment will be set to Right

        public bool AddSpacesToJoinedLanguages; // Some languages (e.g. Chinese, Japanese and Thai) don't add spaces to their words (all characters are placed toguether), making this variable true, will add spaces to all characters to allow wrapping long texts into multiple lines.
        public bool AllowLocalizedParameters=true;
        public bool AllowParameters=true;

        #endregion

        #region Variables: References

        public List<Object> TranslatedObjects = new List<Object>();  // For targets that reference objects (e.g. AudioSource, UITexture,etc) 
                                                                    // this keeps a reference to the possible options.
                                                                    // If the value is not the name of any of this objects then it will try to load the object from the Resources

        
        [NonSerialized] public Dictionary<string, Object> mAssetDictionary = new Dictionary<string, Object>(StringComparer.Ordinal); //This is used to overcome the issue with Unity not serializing Dictionaries

        #endregion

        #region Variable Translation Modifiers


        public UnityEvent LocalizeEvent = new UnityEvent();             // This allows scripts to modify the translations :  e.g. "Player {0} wins"  ->  "Player Red wins"	


        public static string MainTranslation, SecondaryTranslation;		// The callback should use and modify this variables
		public static string CallBackTerm, CallBackSecondaryTerm;		// during the callback, this will hold the FinalTerm and FinalSecondary  to know what terms are originating the translation
		public static Localize CurrentLocalizeComponent;				// while in the LocalizeCallBack, this points to the Localize calling the callback

		public bool AlwaysForceLocalize;			// Force localization when the object gets enabled (useful for callbacks and parameters that change the localization even through the language is the same as in the previous time it was localized)

        [SerializeField] public EventCallback LocalizeCallBack = new EventCallback();    //LocalizeCallBack is deprecated. Please use LocalizeEvent instead.

        #endregion

        #region Variables: Editor Related
        public bool mGUI_ShowReferences;
		public bool mGUI_ShowTems = true;
		public bool mGUI_ShowCallback;
        #endregion

        #region Variables: Runtime (LocalizeTarget)

        public ILocalizeTarget mLocalizeTarget;
        public string mLocalizeTargetName; // Used to resolve multiple targets in a prefab

        #endregion

        #region Localize

        void Awake()
		{
            #if UNITY_EDITOR
            if (BuildPipeline.isBuildingPlayer)
                return;
            #endif

            UpdateAssetDictionary();
            FindTarget();

            if (LocalizeOnAwake)
                OnLocalize();
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (LocalizeCallBack.HasCallback())
            {
                try
                {
                    var methodInfo = UnityEventBase.GetValidMethodInfo(LocalizeCallBack.Target, LocalizeCallBack.MethodName, Array.Empty<Type>());

                    if (methodInfo != null)
                    {
                        UnityAction methodDelegate = Delegate.CreateDelegate(typeof(UnityAction), LocalizeCallBack.Target, methodInfo, false) as UnityAction;
                        if (methodDelegate != null)
                            UnityEventTools.AddPersistentListener(LocalizeEvent, methodDelegate);
                    }
                }
                catch(Exception)
                {}

                LocalizeCallBack.Target = null;
                LocalizeCallBack.MethodName = null;
            }
        }
        #endif

        void OnEnable()
		{
            OnLocalize ();
		}

        public bool HasCallback()
        {
            if (LocalizeCallBack.HasCallback())
                return true;
            return LocalizeEvent.GetPersistentEventCount() > 0;
        }

		public void OnLocalize( bool Force = false )
		{
			if (!Force && (!enabled || gameObject==null || !gameObject.activeInHierarchy))
				return;

			if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
				return;

			if (!AlwaysForceLocalize && !Force && !HasCallback() && LastLocalizedLanguage==LocalizationManager.CurrentLanguage)
				return;
			LastLocalizedLanguage = LocalizationManager.CurrentLanguage;

			// These are the terms actually used (will be mTerm/mSecondaryTerm or will get them from the objects if those are missing. e.g. Labels' text and font name)
			if (string.IsNullOrEmpty(FinalTerm) || string.IsNullOrEmpty(FinalSecondaryTerm))
				GetFinalTerms( out FinalTerm, out FinalSecondaryTerm );


			bool hasCallback = I2Utils.IsPlaying() && HasCallback();

			if (!hasCallback && string.IsNullOrEmpty (FinalTerm) && string.IsNullOrEmpty (FinalSecondaryTerm))
				return;

			CurrentLocalizeComponent = this;
			CallBackTerm = FinalTerm;
			CallBackSecondaryTerm = FinalSecondaryTerm;
			MainTranslation = string.IsNullOrEmpty(FinalTerm) || FinalTerm=="-" ? null : LocalizationManager.GetTranslation (FinalTerm, false);
			SecondaryTranslation = string.IsNullOrEmpty(FinalSecondaryTerm) || FinalSecondaryTerm == "-" ? null : LocalizationManager.GetTranslation (FinalSecondaryTerm, false);

			if (!hasCallback && /*string.IsNullOrEmpty (MainTranslation)*/ string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty (SecondaryTranslation))
				return;

			{
				LocalizeCallBack.Execute (this);  // This allows scripts to modify the translations :  e.g. "Player {0} wins"  ->  "Player Red wins"
                LocalizeEvent.Invoke();
                if (AllowParameters)
					LocalizationManager.ApplyLocalizationParams (ref MainTranslation, gameObject, AllowLocalizedParameters);
			}

			if (!FindTarget())
				return;
            bool applyRTL = LocalizationManager.IsRight2Left && !IgnoreRTL;

            if (MainTranslation != null)
            {
                switch (PrimaryTermModifier)
                {
                    case TermModification.ToUpper:      MainTranslation = MainTranslation.ToUpper(); break;
                    case TermModification.ToLower:      MainTranslation = MainTranslation.ToLower(); break;
                    case TermModification.ToUpperFirst: MainTranslation = GoogleTranslation.UppercaseFirst(MainTranslation); break;
                    case TermModification.ToTitle:      MainTranslation = GoogleTranslation.TitleCase(MainTranslation); break;
                }
                if (!string.IsNullOrEmpty(TermPrefix))
                    MainTranslation = applyRTL ? MainTranslation + TermPrefix : TermPrefix + MainTranslation;
                if (!string.IsNullOrEmpty(TermSuffix))
                    MainTranslation = applyRTL ? TermSuffix + MainTranslation : MainTranslation + TermSuffix;

                if (AddSpacesToJoinedLanguages && LocalizationManager.HasJoinedWords && !string.IsNullOrEmpty(MainTranslation))
                {
                    var sb = new StringBuilder();
                    sb.Append(MainTranslation[0]);
                    for (int i = 1, imax = MainTranslation.Length; i < imax; ++i)
                    {
                        sb.Append(' ');
                        sb.Append(MainTranslation[i]);
                    }

                    MainTranslation = sb.ToString();
                }
                if (applyRTL && mLocalizeTarget.AllowMainTermToBeRTL() && !string.IsNullOrEmpty(MainTranslation))
                    MainTranslation = LocalizationManager.ApplyRTLfix(MainTranslation, MaxCharactersInRTL, IgnoreNumbersInRTL);

            }

            if (SecondaryTranslation != null)
            {
                switch (SecondaryTermModifier)
                {
                    case TermModification.ToUpper:      SecondaryTranslation = SecondaryTranslation.ToUpper(); break;
                    case TermModification.ToLower:      SecondaryTranslation = SecondaryTranslation.ToLower(); break;
                    case TermModification.ToUpperFirst: SecondaryTranslation = GoogleTranslation.UppercaseFirst(SecondaryTranslation); break;
                    case TermModification.ToTitle:      SecondaryTranslation = GoogleTranslation.TitleCase(SecondaryTranslation); break;
                }
                if (applyRTL && mLocalizeTarget.AllowSecondTermToBeRTL() && !string.IsNullOrEmpty(SecondaryTranslation))
                        SecondaryTranslation = LocalizationManager.ApplyRTLfix(SecondaryTranslation);
            }

            if (LocalizationManager.HighlightLocalizedTargets)
            {
                MainTranslation = "LOC:" + FinalTerm;
            }

            mLocalizeTarget.DoLocalize( this, MainTranslation, SecondaryTranslation );

			CurrentLocalizeComponent = null;
		}

		#endregion

		#region Finding Target

		public bool FindTarget()
		{
            if (mLocalizeTarget != null && mLocalizeTarget.IsValid(this))
                return true;

            if (mLocalizeTarget!=null)
            {
                DestroyImmediate(mLocalizeTarget);
                mLocalizeTarget = null;
                mLocalizeTargetName = null;
            }

            if (!string.IsNullOrEmpty(mLocalizeTargetName))
            {
                foreach (var desc in LocalizationManager.mLocalizeTargets)
                {
                    if (mLocalizeTargetName == desc.GetTargetType().ToString())
                    {
                        if (desc.CanLocalize(this))
                            mLocalizeTarget = desc.CreateTarget(this);
                        if (mLocalizeTarget!=null)
                            return true;
                    }
                }
            }

            foreach (var desc in LocalizationManager.mLocalizeTargets)
            {
                if (!desc.CanLocalize(this))
                    continue;
                mLocalizeTarget = desc.CreateTarget(this);
                mLocalizeTargetName = desc.GetTargetType().ToString();
                if (mLocalizeTarget != null)
                    return true;
            }

			return false;
		}

		#endregion

		#region Finding Term
		
		// Returns the term that will actually be translated
		// its either the Term value in this class or the text of the label if there is no term
		public void GetFinalTerms( out string primaryTerm, out string secondaryTerm )
		{
			primaryTerm 	= string.Empty;
			secondaryTerm 	= string.Empty;

			if (!FindTarget())
				return;


			// if either the primary or secondary term is missing, get them. (e.g. from the label's text and font name)
            if (mLocalizeTarget != null)
            {
                mLocalizeTarget.GetFinalTerms(this, mTerm, mTermSecondary, out primaryTerm, out secondaryTerm);
                primaryTerm = I2Utils.GetValidTermName(primaryTerm);
            }

            // If there are values already set, go with those
            if (!string.IsNullOrEmpty(mTerm))
				primaryTerm = mTerm;

			if (!string.IsNullOrEmpty(mTermSecondary))
				secondaryTerm = mTermSecondary;

			if (primaryTerm != null)
				primaryTerm = primaryTerm.Trim();
			if (secondaryTerm != null)
				secondaryTerm = secondaryTerm.Trim();
		}

		public string GetMainTargetsText()
		{
			string primary = null, secondary = null;

			if (mLocalizeTarget!=null)
				mLocalizeTarget.GetFinalTerms( this, null, null, out primary, out secondary );

			return string.IsNullOrEmpty(primary) ? mTerm : primary;
		}
		
		public void SetFinalTerms( string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII )
		{
			primaryTerm = RemoveNonASCII ? I2Utils.GetValidTermName(Main) : Main;
			secondaryTerm = Secondary;
		}
		
		#endregion

		#region Misc

		public void SetTerm (string primary)
		{
			if (!string.IsNullOrEmpty(primary))
				FinalTerm = mTerm = primary;

			OnLocalize (true);
		}

		public void SetTerm(string primary, string secondary )
		{
			if (!string.IsNullOrEmpty(primary))
				FinalTerm = mTerm = primary;
			FinalSecondaryTerm = mTermSecondary = secondary;

			OnLocalize(true);
		}

		internal T GetSecondaryTranslatedObj<T>( ref string mainTranslation, ref string secondaryTranslation ) where T: Object
		{
			string newMain, newSecond;

			//--[ Allow main translation to override Secondary ]-------------------
			DeserializeTranslation(mainTranslation, out newMain, out newSecond);

			T obj = null;

			if (!string.IsNullOrEmpty(newSecond))
			{
				obj = GetObject<T>(newSecond);
				if (obj != null)
				{
					mainTranslation = newMain;
					secondaryTranslation = newSecond;
				}
			}

			if (obj == null)
				obj = GetObject<T>(secondaryTranslation);

			return obj;
		}

        public void UpdateAssetDictionary()
        {
            TranslatedObjects.RemoveAll(x => x == null);
            mAssetDictionary = TranslatedObjects.Distinct()
                                                .GroupBy(o => o.name)
                                                .ToDictionary(g => g.Key, g => g.First());
        }

        internal T GetObject<T>( string Translation) where T: Object
		{
			if (string.IsNullOrEmpty (Translation))
				return null;
			T obj = GetTranslatedObject<T>(Translation);
			
			//if (obj==null)
			//{
				// Remove path and search by name
				//int Index = Translation.LastIndexOfAny("/\\".ToCharArray());
				//if (Index>=0)
				//{
				//	Translation = Translation.Substring(Index+1);
				//	obj = GetTranslatedObject<T>(Translation);
				//}
			//}
			return obj;
		}

		T GetTranslatedObject<T>( string Translation) where T: Object
		{
			T Obj = FindTranslatedObject<T>(Translation);
			/*if (Obj == null) 
				return null;
			
			if ((Obj as T) != null) 
				return Obj as T;
			
			// If the found Obj is not of type T, then try finding a component inside
			if (Obj as Component != null)
				return (Obj as Component).GetComponent(typeof(T)) as T;
			
			if (Obj as GameObject != null)
				return (Obj as GameObject).GetComponent(typeof(T)) as T;
			*/
			return Obj;
		}


		// translation format: "[secondary]value"   [secondary] is optional
		void DeserializeTranslation( string translation, out string value, out string secondary )
		{
			if (!string.IsNullOrEmpty(translation) && translation.Length>1 && translation[0]=='[')
			{
				int Index = translation.IndexOf(']');
				if (Index>0)
				{
					secondary = translation.Substring(1, Index-1);
					value = translation.Substring(Index+1);
					return;
				}
			}
			value = translation;
			secondary = string.Empty;
		}
		
		public T FindTranslatedObject<T>( string value) where T : Object
		{
			if (string.IsNullOrEmpty(value))
				return null;

            if (mAssetDictionary == null || mAssetDictionary.Count != TranslatedObjects.Count)
            {
                UpdateAssetDictionary();
            }
 
            foreach (var kvp in mAssetDictionary)
            { 
				if (kvp.Value is T && value.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
				{
					// Check if the value is just the name or has a path
					if (string.Compare(value, kvp.Key, StringComparison.OrdinalIgnoreCase)==0)
						return (T) kvp.Value;

					// Check if the path matches
					//Resources.get TranslatedObjects[i].
				}
			}

			T obj = LocalizationManager.FindAsset(value) as T;
			if (obj)
				return obj;

			obj = ResourceManager.pInstance.GetAsset<T>(value);
			
			/*#if UNITY_EDITOR
			if (obj == null && I2Utils.IsPlaying())
			{
				obj = (T)LoadFrmEditor(value,typeof(T));
				Debug.LogError($"{gameObject.name}Localize obj == null {value} ",this);
			}
			#endif*/

			return obj;
		}

		/*
		public Object LoadFrmEditor(string path, Type assetType)
		{
			var info = YooAsset.YooAssets.GetAssetInfo(path);
			if (info == null) 
			{
				Debug.LogError($"没有这个资源信息 无法使用编辑器加载 {path}");
				return null;
			}
            
			Debug.LogError(info.AssetPath);
            
			var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(info.AssetPath,assetType);
			if (asset == null)
			{
				Debug.LogError($"没有加载到这个资源 {info.AssetPath}");
				return null;
			}
			return asset;
		}*/
		
		
		public bool HasTranslatedObject( Object Obj )
		{
			if (TranslatedObjects.Contains(Obj)) 
				return true;
			return ResourceManager.pInstance.HasAsset(Obj);

		}

		public void AddTranslatedObject( Object Obj )
		{
            if (TranslatedObjects.Contains(Obj))
                return;
			TranslatedObjects.Add(Obj);
            UpdateAssetDictionary();
		}

		#endregion
	
		#region Utilities
		// This can be used to set the language when a button is clicked
		public void SetGlobalLanguage( string Language )
		{
			LocalizationManager.CurrentLanguage = Language;
		}

		#endregion
	}
}