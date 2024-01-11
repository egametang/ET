#if TK2D

using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] 
    #endif

    public class LocalizeTarget_2DToolKit_Label : LocalizeTarget<tk2dTextMesh>
    {
        static LocalizeTarget_2DToolKit_Label() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<tk2dTextMesh, LocalizeTarget_2DToolKit_Label>() { Name = "2DToolKit Label", Priority = 100 }); }

        TextAnchor mOriginalAlignment = TextAnchor.MiddleCenter;
        bool mInitializeAlignment = true;

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Text; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.TK2dFont; }

        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return true; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = mTarget ? mTarget.text : null;
            secondaryTerm = (mTarget.font != null ? mTarget.font.name : string.Empty);
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            //--[ Localize Font Object ]----------
            tk2dFont newFont = cmp.GetSecondaryTranslatedObj<tk2dFont>(ref mainTranslation, ref secondaryTranslation);
            if (newFont != null && mTarget.font != newFont)
            {
                mTarget.font = newFont.data;
            }

            if (mInitializeAlignment)
            {
                mInitializeAlignment = false;
                mOriginalAlignment = mTarget.anchor;
            }

            if (mainTranslation != null &&  mTarget.text != mainTranslation)
            {
                if (Localize.CurrentLocalizeComponent.CorrectAlignmentForRTL)
                {
                    int align = (int)mTarget.anchor;

                    if (align % 3 == 0)
                        mTarget.anchor = LocalizationManager.IsRight2Left ? mTarget.anchor + 2 : mOriginalAlignment;
                    else
                    if (align % 3 == 2)
                        mTarget.anchor = LocalizationManager.IsRight2Left ? mTarget.anchor - 2 : mOriginalAlignment;
                }
                mTarget.text = mainTranslation;
            }
        }
    }
}
#endif

