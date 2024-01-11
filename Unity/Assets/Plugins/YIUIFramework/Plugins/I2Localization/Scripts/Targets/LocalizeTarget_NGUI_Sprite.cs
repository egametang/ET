#if NGUI

using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] 
    #endif

	public class LocalizeTarget_NGUI_Sprite : LocalizeTarget<UISprite>
	{
        static LocalizeTarget_NGUI_Sprite() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<UISprite, LocalizeTarget_NGUI_Sprite>() { Name = "NGUI UISprite", Priority = 100 }); }

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Sprite; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.UIAtlas; }
        public override bool CanUseSecondaryTerm () { return true; }
		public override bool AllowMainTermToBeRTL () { return false; }
		public override bool AllowSecondTermToBeRTL () { return false; }

		public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm )
		{
			primaryTerm = mTarget ? mTarget.spriteName : null;
			secondaryTerm = (mTarget.atlas as UIAtlas ? (mTarget.atlas as UIAtlas).name : string.Empty);
        }


        public override void DoLocalize ( Localize cmp, string mainTranslation, string secondaryTranslation )
		{
            if (mTarget.spriteName == mainTranslation)
                return;

            //--[ Localize Atlas ]----------
            UIAtlas newAtlas = cmp.GetSecondaryTranslatedObj<UIAtlas>(ref mainTranslation, ref secondaryTranslation);
            bool bChanged = false;
            if (newAtlas != null && ((mTarget.atlas as UIAtlas) != newAtlas))
            {
                mTarget.atlas = newAtlas;
                bChanged = true;
            }

            if (newAtlas==null)
            {
                NGUIAtlas newNGUIAtlas = cmp.GetSecondaryTranslatedObj<NGUIAtlas>(ref mainTranslation, ref secondaryTranslation);
                if (newAtlas != null && ((mTarget.atlas as NGUIAtlas) != newNGUIAtlas))
                {
                    mTarget.atlas = newAtlas;
                    bChanged = true;
                }

            }

            if (mTarget.spriteName != mainTranslation && mTarget.atlas.GetSprite(mainTranslation) != null)
            {
                mTarget.spriteName = mainTranslation;
                bChanged = true;
            }
            if (bChanged)
                mTarget.MakePixelPerfect();
        }
	}
}
#endif

