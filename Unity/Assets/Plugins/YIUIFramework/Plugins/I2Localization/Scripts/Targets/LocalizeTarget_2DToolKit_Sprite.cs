#if TK2D

using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] 
    #endif

    public class LocalizeTarget_2DToolKit_Sprite : LocalizeTarget<tk2dBaseSprite>
    {
        static LocalizeTarget_2DToolKit_Sprite() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<tk2dBaseSprite, LocalizeTarget_2DToolKit_Sprite>() { Name = "2DToolKit Sprite", Priority = 100 }); }


        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.TK2dCollection; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.TK2dCollection; }

        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = (mTarget.CurrentSprite != null ? mTarget.CurrentSprite.name : string.Empty);
            secondaryTerm = (mTarget.Collection != null ? mTarget.Collection.spriteCollectionName : null);
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            if (string.IsNullOrEmpty(mainTranslation))
                return;

            //--[ Localize Atlas ]----------
            tk2dSpriteCollection newCollection = cmp.GetSecondaryTranslatedObj<tk2dSpriteCollection>(ref mainTranslation, ref secondaryTranslation);

            if (newCollection != null)
            {
                if (mTarget.CurrentSprite.name != mainTranslation || mTarget.Collection.name != secondaryTranslation)
                    mTarget.SetSprite(newCollection.spriteCollection, mainTranslation);
            }
            else
            {
                if (mTarget.CurrentSprite.name != mainTranslation)
                    mTarget.SetSprite(mainTranslation);
            }
        }
    }
}
#endif

