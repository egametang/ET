#if SVG
using UnityEngine;

namespace I2.Loc
{
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad] 
    #endif

    public class LocalizeTarget_SVGImporter_Renderer : LocalizeTarget<SVGImporter.SVGRenderer>
    {
        static LocalizeTarget_SVGImporter_Renderer() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<SVGImporter.SVGRenderer, LocalizeTarget_SVGImporter_Renderer>() { Name = "SVG Renderer", Priority = 100 }); }

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.SVGAsset; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Material; }
        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = (mTarget.vectorGraphics != null ? mTarget.vectorGraphics.name : string.Empty);
            secondaryTerm = (mTarget.opaqueMaterial != null ? mTarget.opaqueMaterial.name : string.Empty);
        }


        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            var OldVectorG = mTarget.vectorGraphics;
            if (OldVectorG == null || OldVectorG.name != mainTranslation)
                mTarget.vectorGraphics = cmp.FindTranslatedObject<SVGImporter.SVGAsset>(mainTranslation);

            var OldMaterial = mTarget.opaqueMaterial;
            if (OldMaterial == null || OldMaterial.name != secondaryTranslation)
                mTarget.opaqueMaterial = cmp.FindTranslatedObject<Material>(secondaryTranslation);

            mTarget.SetAllDirty();
        }
    }
}
#endif
