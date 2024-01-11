using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_UnityUI_RawImage : LocalizeTarget<RawImage>
    {
        static LocalizeTarget_UnityUI_RawImage() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<RawImage, LocalizeTarget_UnityUI_RawImage> { Name = "RawImage", Priority = 100 }); }

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Texture; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Text; }
        public override bool CanUseSecondaryTerm() { return false; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }


        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = mTarget.mainTexture ? mTarget.mainTexture.name : "";
            secondaryTerm = null;
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            Texture Old = mTarget.texture;
            if (Old == null || Old.name != mainTranslation)
                mTarget.texture = cmp.FindTranslatedObject<Texture>(mainTranslation);

            // If the old value is not in the translatedObjects, then unload it as it most likely was loaded from Resources
            //if (!HasTranslatedObject(Old))
            //	Resources.UnloadAsset(Old);

            // In the editor, sometimes unity "forgets" to show the changes
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(mTarget);
            #endif
        }
    }
}