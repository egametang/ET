#if NGUI

using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_NGUI_Texture : LocalizeTarget<UITexture>
    {
        static LocalizeTarget_NGUI_Texture() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<UITexture, LocalizeTarget_NGUI_Texture> { Name = "NGUI UITexture", Priority = 100 }); }

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Texture; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Text; }
        public override bool CanUseSecondaryTerm() { return false; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = mTarget!=null && mTarget.mainTexture!=null ? mTarget.mainTexture.name : null;
            secondaryTerm = null;
        }

        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            Texture Old = mTarget.mainTexture;
            if (Old == null || Old.name != mainTranslation)
            {
                mTarget.mainTexture = cmp.FindTranslatedObject<Texture>(mainTranslation);
                mTarget.MakePixelPerfect();
            }
        }
    }
}
#endif

