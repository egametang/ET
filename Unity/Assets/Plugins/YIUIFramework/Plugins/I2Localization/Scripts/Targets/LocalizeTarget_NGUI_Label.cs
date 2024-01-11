#if NGUI

using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_NGUI_Label : LocalizeTarget<UILabel>
    {
        static LocalizeTarget_NGUI_Label() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<UILabel, LocalizeTarget_NGUI_Label> { Name = "NGUI Label", Priority = 100 }); }

        NGUIText.Alignment mAlignment_RTL = NGUIText.Alignment.Right;
        NGUIText.Alignment mAlignment_LTR = NGUIText.Alignment.Left;
        bool mAlignmentWasRTL;
        bool mInitializeAlignment = true;

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Text; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.UIFont; }
        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return true; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = mTarget ? mTarget.text : null;
            secondaryTerm = mTarget.ambigiousFont != null ? mTarget.ambigiousFont.name : string.Empty;
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            //--[ Localize Font Object ]----------
            Font newFont = cmp.GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
            if (newFont != null)
            {
                if (newFont != mTarget.ambigiousFont)
                    mTarget.ambigiousFont = newFont;
            }
            if (newFont==null)
            {
                UIFont newUIFont = cmp.GetSecondaryTranslatedObj<UIFont>(ref mainTranslation, ref secondaryTranslation);
                if (newUIFont != null && mTarget.ambigiousFont != newUIFont)
                    mTarget.ambigiousFont = newUIFont;
            }
            if (newFont == null)
            {
                NGUIFont newUIFont = cmp.GetSecondaryTranslatedObj<NGUIFont>(ref mainTranslation, ref secondaryTranslation);
                if (newUIFont != null && mTarget.ambigiousFont != newUIFont)
                    mTarget.ambigiousFont = newUIFont;
            }


            if (mInitializeAlignment)
            {
                mInitializeAlignment = false;
                mAlignment_LTR = mAlignment_RTL = mTarget.alignment;

                if (LocalizationManager.IsRight2Left && mAlignment_RTL == NGUIText.Alignment.Right)
                    mAlignment_LTR = NGUIText.Alignment.Left;
                if (!LocalizationManager.IsRight2Left && mAlignment_LTR == NGUIText.Alignment.Left)
                    mAlignment_RTL = NGUIText.Alignment.Right;
            }

            UIInput input = NGUITools.FindInParents<UIInput>(mTarget.gameObject);
            if (input != null && input.label == mTarget)
            {
                if (mainTranslation != null && input.defaultText != mainTranslation)
                {
                    if (cmp.CorrectAlignmentForRTL && (input.label.alignment == NGUIText.Alignment.Left || input.label.alignment == NGUIText.Alignment.Right))
                        input.label.alignment = LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR;

                    input.defaultText = mainTranslation;
                }
            }
            else
            {
                if (mainTranslation != null && mTarget.text != mainTranslation)
                {
                    if (cmp.CorrectAlignmentForRTL && (mTarget.alignment == NGUIText.Alignment.Left || mTarget.alignment == NGUIText.Alignment.Right))
                        mTarget.alignment = LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR;

                    mTarget.text = mainTranslation;
                }
            }
        }
    }
}
#endif

