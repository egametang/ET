using UnityEditor;
using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_UnityStandard_AudioSource : LocalizeTarget<AudioSource>
    {
        static LocalizeTarget_UnityStandard_AudioSource() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<AudioSource, LocalizeTarget_UnityStandard_AudioSource> { Name = "AudioSource", Priority = 100 }); }

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.AudioClip; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Text; }
        public override bool CanUseSecondaryTerm() { return false; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            AudioClip clip = mTarget.clip;
            primaryTerm = clip ? clip.name : string.Empty;
            secondaryTerm = null;
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            bool bIsPlaying = (mTarget.isPlaying || mTarget.loop) && Application.isPlaying;
            AudioClip OldClip = mTarget.clip;
            AudioClip NewClip = cmp.FindTranslatedObject<AudioClip>(mainTranslation);
            if (OldClip != NewClip)
                mTarget.clip = NewClip;

            if (bIsPlaying && mTarget.clip)
                mTarget.Play();

            // If the old clip is not in the translatedObjects, then unload it as it most likely was loaded from Resources
            //if (!HasTranslatedObject(OldClip))
            //	Resources.UnloadAsset(OldClip);
        }
    }
}
