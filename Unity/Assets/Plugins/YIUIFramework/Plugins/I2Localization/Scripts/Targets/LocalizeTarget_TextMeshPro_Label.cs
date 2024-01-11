using System;
using TMPro;
using UnityEditor;
using UnityEngine;

#if TextMeshPro
namespace I2.Loc
{
    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_TextMeshPro_Label : LocalizeTarget<TextMeshPro>
    {
        static LocalizeTarget_TextMeshPro_Label() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<TextMeshPro, LocalizeTarget_TextMeshPro_Label> { Name = "TextMeshPro Label", Priority = 100 }); }

        TextAlignmentOptions mAlignment_RTL = TextAlignmentOptions.Right;
        TextAlignmentOptions mAlignment_LTR = TextAlignmentOptions.Left;
        bool mAlignmentWasRTL;
        bool mInitializeAlignment = true;

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Text; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Font; }
        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return true; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = mTarget ? mTarget.text : null;
            secondaryTerm = mTarget.font != null ? mTarget.font.name : string.Empty;
        }

        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            //--[ Localize Font Object ]----------
            {
                TMP_FontAsset newFont = cmp.GetSecondaryTranslatedObj<TMP_FontAsset>(ref mainTranslation, ref secondaryTranslation);

                if (newFont != null)
                {
                    SetFont(mTarget, newFont);
                }
                else
                {
                    //--[ Localize Font Material ]----------
                    Material newMat = cmp.GetSecondaryTranslatedObj<Material>(ref mainTranslation, ref secondaryTranslation);
                    if (newMat != null && mTarget.fontMaterial != newMat)
                    {
                        if (!newMat.name.StartsWith(mTarget.font.name, StringComparison.Ordinal))
                        {
                            newFont = GetTMPFontFromMaterial(cmp, secondaryTranslation.EndsWith(newMat.name, StringComparison.Ordinal) ? secondaryTranslation : newMat.name);
                            if (newFont != null)
                                SetFont(mTarget, newFont);
                        }
                        SetMaterial(mTarget, newMat); 
                    }
                           
                }
            }
            if (mInitializeAlignment)
            {
                mInitializeAlignment = false;
                mAlignmentWasRTL = LocalizationManager.IsRight2Left;
                InitAlignment_TMPro(mAlignmentWasRTL, mTarget.alignment, out mAlignment_LTR, out mAlignment_RTL);
            }
            else
            {
                TextAlignmentOptions alignRTL, alignLTR;
                InitAlignment_TMPro(mAlignmentWasRTL, mTarget.alignment, out alignLTR, out alignRTL);

                if (mAlignmentWasRTL && mAlignment_RTL != alignRTL ||
                    !mAlignmentWasRTL && mAlignment_LTR != alignLTR)
                {
                    mAlignment_LTR = alignLTR;
                    mAlignment_RTL = alignRTL;
                }
                mAlignmentWasRTL = LocalizationManager.IsRight2Left;
            }

            if (mainTranslation != null && mTarget.text != mainTranslation)
            {
                if (cmp.CorrectAlignmentForRTL)
                {
                    mTarget.alignment = LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR;
                }

                mTarget.isRightToLeftText = LocalizationManager.IsRight2Left;
                if (LocalizationManager.IsRight2Left) mainTranslation = I2Utils.ReverseText(mainTranslation);
                
                mTarget.text = mainTranslation;
            }
        }

        #region Tools
        internal static TMP_FontAsset GetTMPFontFromMaterial(Localize cmp, string matName)
        {
            string splitChars = " .\\/-[]()";
            for (int i = matName.Length - 1; i > 0;)
            {
                // Find first valid character
                while (i > 0 && splitChars.IndexOf(matName[i]) >= 0)
                    i--;

                if (i <= 0) break;

                var fontName = matName.Substring(0, i + 1);
                var obj = cmp.GetObject<TMP_FontAsset>(fontName);
                if (obj != null)
                    return obj;

                // skip this word
                while (i > 0 && splitChars.IndexOf(matName[i]) < 0)
                    i--;
            }

            return null;
        }

        internal static void InitAlignment_TMPro(bool isRTL, TextAlignmentOptions alignment, out TextAlignmentOptions alignLTR, out TextAlignmentOptions alignRTL)
        {
            alignLTR = alignRTL = alignment;

            if (isRTL)
            {
                switch (alignment)
                {
                    case TextAlignmentOptions.TopRight: alignLTR = TextAlignmentOptions.TopLeft; break;
                    case TextAlignmentOptions.Right: alignLTR = TextAlignmentOptions.Left; break;
                    case TextAlignmentOptions.BottomRight: alignLTR = TextAlignmentOptions.BottomLeft; break;
                    case TextAlignmentOptions.BaselineRight: alignLTR = TextAlignmentOptions.BaselineLeft; break;
                    case TextAlignmentOptions.MidlineRight: alignLTR = TextAlignmentOptions.MidlineLeft; break;
                    case TextAlignmentOptions.CaplineRight: alignLTR = TextAlignmentOptions.CaplineLeft; break;

                    case TextAlignmentOptions.TopLeft: alignLTR = TextAlignmentOptions.TopRight; break;
                    case TextAlignmentOptions.Left: alignLTR = TextAlignmentOptions.Right; break;
                    case TextAlignmentOptions.BottomLeft: alignLTR = TextAlignmentOptions.BottomRight; break;
                    case TextAlignmentOptions.BaselineLeft: alignLTR = TextAlignmentOptions.BaselineRight; break;
                    case TextAlignmentOptions.MidlineLeft: alignLTR = TextAlignmentOptions.MidlineRight; break;
                    case TextAlignmentOptions.CaplineLeft: alignLTR = TextAlignmentOptions.CaplineRight; break;

                }
            }
            else
            {
                switch (alignment)
                {
                    case TextAlignmentOptions.TopRight: alignRTL = TextAlignmentOptions.TopLeft; break;
                    case TextAlignmentOptions.Right: alignRTL = TextAlignmentOptions.Left; break;
                    case TextAlignmentOptions.BottomRight: alignRTL = TextAlignmentOptions.BottomLeft; break;
                    case TextAlignmentOptions.BaselineRight: alignRTL = TextAlignmentOptions.BaselineLeft; break;
                    case TextAlignmentOptions.MidlineRight: alignRTL = TextAlignmentOptions.MidlineLeft; break;
                    case TextAlignmentOptions.CaplineRight: alignRTL = TextAlignmentOptions.CaplineLeft; break;

                    case TextAlignmentOptions.TopLeft: alignRTL = TextAlignmentOptions.TopRight; break;
                    case TextAlignmentOptions.Left: alignRTL = TextAlignmentOptions.Right; break;
                    case TextAlignmentOptions.BottomLeft: alignRTL = TextAlignmentOptions.BottomRight; break;
                    case TextAlignmentOptions.BaselineLeft: alignRTL = TextAlignmentOptions.BaselineRight; break;
                    case TextAlignmentOptions.MidlineLeft: alignRTL = TextAlignmentOptions.MidlineRight; break;
                    case TextAlignmentOptions.CaplineLeft: alignRTL = TextAlignmentOptions.CaplineRight; break;
                }
            }
        }

        internal static void SetFont(TMP_Text label, TMP_FontAsset newFont)
        {
            if (label.font != newFont)
            {
                label.font = newFont;
            }
            if (label.linkedTextComponent != null)
            {
                SetFont(label.linkedTextComponent, newFont);
            }
        }
        internal static void SetMaterial(TMP_Text label, Material newMat)
        {
            if (label.fontSharedMaterial != newMat)
            {
                label.fontSharedMaterial = newMat;
            }
            if (label.linkedTextComponent != null)
            {
                SetMaterial(label.linkedTextComponent, newMat);
            }
        }
        #endregion
    }
}
#endif