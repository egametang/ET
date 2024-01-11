#if SVG
using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] 
    #endif

    public class LocalizeTarget_SVGImporter_Image : LocalizeTarget<SVGImporter.SVGImage>
    {
        static LocalizeTarget_SVGImporter_Image() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<SVGImporter.SVGImage, LocalizeTarget_SVGImporter_Image>() { Name = "SVG Image", Priority = 100 }); }

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.SVGAsset; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Material; }
        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = (mTarget.vectorGraphics != null ? mTarget.vectorGraphics.name : string.Empty);
            secondaryTerm = (mTarget.material != null ? mTarget.material.name : null);
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            var OldVectorG = mTarget.vectorGraphics;
            if (OldVectorG == null || OldVectorG.name != mainTranslation)
                mTarget.vectorGraphics = cmp.FindTranslatedObject<SVGImporter.SVGAsset>(mainTranslation);

            var OldMaterial = mTarget.material;
            if (OldMaterial == null || OldMaterial.name != secondaryTranslation)
                mTarget.material = cmp.FindTranslatedObject<Material>(secondaryTranslation);

            mTarget.SetAllDirty();
        }
    }
}
#endif
