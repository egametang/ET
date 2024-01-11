using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace I2.Loc 
{ 
    #if UNITY_EDITOR 
    [InitializeOnLoad]
    #endif 
    public class LocalizeTarget_UnityStandard_VideoPlayer : LocalizeTarget<VideoPlayer> 
    { 
        static LocalizeTarget_UnityStandard_VideoPlayer() { AutoRegister(); } 
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] 
        static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<VideoPlayer, LocalizeTarget_UnityStandard_VideoPlayer> { Name = "VideoPlayer", Priority = 100 }); } 
        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Video; } 
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Text; } 
        public override bool CanUseSecondaryTerm() { return false; } 
        public override bool AllowMainTermToBeRTL() { return false; } 
        public override bool AllowSecondTermToBeRTL() { return false; } 
        public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm) 
        {
            VideoClip clip = mTarget.clip;
            primaryTerm = clip != null ? clip.name: string.Empty; 
            secondaryTerm = null; 
        } 
        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation) 
        { 
            VideoClip Old = mTarget.clip; 
            if (Old == null || Old.name != mainTranslation) 
                mTarget.clip = cmp.FindTranslatedObject<VideoClip>(mainTranslation); 
        } 
    } 
}