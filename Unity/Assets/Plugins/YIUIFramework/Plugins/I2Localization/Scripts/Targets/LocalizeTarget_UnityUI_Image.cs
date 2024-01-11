using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_UnityUI_Image : LocalizeTarget<Image>
	{
        static LocalizeTarget_UnityUI_Image() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<Image, LocalizeTarget_UnityUI_Image> { Name = "Image", Priority = 100 }); }

		public override bool CanUseSecondaryTerm () { return false; }
		public override bool AllowMainTermToBeRTL () { return false; }
		public override bool AllowSecondTermToBeRTL () { return false; }
        public override eTermType GetPrimaryTermType(Localize cmp)
        {
            return mTarget.sprite == null ? eTermType.Texture : eTermType.Sprite;
        }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Text; }


        public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm )
		{
            primaryTerm = mTarget.mainTexture ? mTarget.mainTexture.name : "";
            if (mTarget.sprite!=null && mTarget.sprite.name!=primaryTerm)
                primaryTerm += "." + mTarget.sprite.name;

			secondaryTerm = null;
		}


		public override void DoLocalize ( Localize cmp, string mainTranslation, string secondaryTranslation )
		{
            Sprite Old = mTarget.sprite;
			if (Old==null || Old.name!=mainTranslation)
				mTarget.sprite = cmp.FindTranslatedObject<Sprite>( mainTranslation );

			// If the old value is not in the translatedObjects, then unload it as it most likely was loaded from Resources
			//if (!HasTranslatedObject(Old))
			//	Resources.UnloadAsset(Old);

			// In the editor, sometimes unity "forgets" to show the changes
#if UNITY_EDITOR
			if (!Application.isPlaying)
				EditorUtility.SetDirty( mTarget );
#endif
		}
	}
}
