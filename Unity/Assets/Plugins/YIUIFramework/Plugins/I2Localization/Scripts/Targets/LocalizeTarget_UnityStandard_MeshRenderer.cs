using UnityEditor;
using UnityEngine;

#pragma warning disable 618

namespace I2.Loc
{
    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_UnityStandard_MeshRenderer : LocalizeTarget<MeshRenderer>
    {
        static LocalizeTarget_UnityStandard_MeshRenderer() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<MeshRenderer, LocalizeTarget_UnityStandard_MeshRenderer> { Name = "MeshRenderer", Priority = 800 }); }

        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.Mesh; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Material; }
        public override bool CanUseSecondaryTerm() { return true; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            if (mTarget==null)
            {
                primaryTerm = secondaryTerm = null;
            }
            else
            {
                MeshFilter filter = mTarget.GetComponent<MeshFilter>();
                if (filter==null || filter.sharedMesh==null)
                {
                    primaryTerm = null;
                }
                else
                {
                    #if UNITY_EDITOR
                        primaryTerm = AssetDatabase.GetAssetPath(filter.sharedMesh);
                        I2Utils.RemoveResourcesPath(ref primaryTerm);
                    #else
                        primaryTerm = filter.sharedMesh.name;
                    #endif
                }
            }

            if (mTarget==null || mTarget.sharedMaterial==null)
            {
                secondaryTerm = null;
            }
            else
            {
                #if UNITY_EDITOR
                    secondaryTerm = AssetDatabase.GetAssetPath(mTarget.sharedMaterial);
                    I2Utils.RemoveResourcesPath(ref secondaryTerm);
                #else
                    secondaryTerm = mTarget.sharedMaterial.name;
                #endif
            }
        }

        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            //--[ Localize Material]----------
            Material newMat = cmp.GetSecondaryTranslatedObj<Material>(ref mainTranslation, ref secondaryTranslation);
            if (newMat != null && mTarget.sharedMaterial != newMat)
            {
                mTarget.material = newMat;
            }

            //--[ Localize Mesh ]----------
            Mesh newMesh = cmp.FindTranslatedObject<Mesh>( mainTranslation);
            MeshFilter filter = mTarget.GetComponent<MeshFilter>();
            if (newMesh != null && filter.sharedMesh != newMesh)
            {
                filter.mesh = newMesh;
            }
        }
    }
}