using UnityEditor;
using UnityEngine;

#pragma warning disable 618

namespace I2.Loc
{
    public class LocalizeTargetDesc_Prefab : LocalizeTargetDesc<LocalizeTarget_UnityStandard_Prefab>
    {
        public override bool CanLocalize(Localize cmp) { return true; }
    }

    #if UNITY_EDITOR
    [InitializeOnLoad] 
    #endif

    public class LocalizeTarget_UnityStandard_Prefab : LocalizeTarget<GameObject>
    {
        static LocalizeTarget_UnityStandard_Prefab() { AutoRegister(); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void AutoRegister() { LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Prefab { Name = "Prefab", Priority = 250 }); }

        public override bool IsValid(Localize cmp) { return true; }
        public override eTermType GetPrimaryTermType(Localize cmp) { return eTermType.GameObject; }
        public override eTermType GetSecondaryTermType(Localize cmp) { return eTermType.Text; }
        public override bool CanUseSecondaryTerm() { return false; }
        public override bool AllowMainTermToBeRTL() { return false; }
        public override bool AllowSecondTermToBeRTL() { return false; }

        public override void GetFinalTerms ( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
        {
            primaryTerm = cmp.name;
            secondaryTerm = null;
        }

        public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
        {
            if (string.IsNullOrEmpty(mainTranslation))
                return;

            if (mTarget && mTarget.name == mainTranslation)
                return;

            Transform locTr = cmp.transform;

            var objName = mainTranslation;
            var idx = mainTranslation.LastIndexOfAny(LanguageSourceData.CategorySeparators);
            if (idx >= 0)
                objName = objName.Substring(idx + 1);

            Transform mNew = InstantiateNewPrefab(cmp, mainTranslation);
            if (mNew == null)
                return;
            mNew.name = objName;

            for (int i = locTr.childCount - 1; i >= 0; --i)
            {
                var child = locTr.GetChild(i);
                if (child!=mNew)
                {
                    #if UNITY_EDITOR
                        if (Application.isPlaying)
                            Destroy(child.gameObject);
                        else
                            DestroyImmediate(child.gameObject);
                    #else
				        Object.Destroy (child.gameObject);
                    #endif
                }
            }
        }

        Transform InstantiateNewPrefab(Localize cmp, string mainTranslation)
        {
            GameObject NewPrefab = cmp.FindTranslatedObject<GameObject>(mainTranslation);
            if (NewPrefab == null)
                return null;

            GameObject current = mTarget;

            mTarget = Instantiate(NewPrefab);
            if (mTarget == null)
                return null;

            Transform locTr = cmp.transform;
            Transform mNew = mTarget.transform;
            mNew.SetParent(locTr);

            Transform bBase = current ? current.transform : locTr;
            //mNew.localScale = bBase.localScale;
            mNew.rotation = bBase.rotation;
            mNew.position = bBase.position;

            return mNew;
        }
    }
}
