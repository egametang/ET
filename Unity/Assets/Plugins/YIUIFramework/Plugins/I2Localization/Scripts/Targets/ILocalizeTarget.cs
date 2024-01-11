using UnityEngine;

namespace I2.Loc
{ 
    public abstract class ILocalizeTarget : ScriptableObject
    {
        public abstract bool IsValid(Localize cmp);
        public abstract void GetFinalTerms( Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm);
        public abstract void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation);

        public abstract bool CanUseSecondaryTerm();
        public abstract bool AllowMainTermToBeRTL();
        public abstract bool AllowSecondTermToBeRTL();
        public abstract eTermType GetPrimaryTermType(Localize cmp);
        public abstract eTermType GetSecondaryTermType(Localize cmp);
    }

    public abstract class LocalizeTarget<T> : ILocalizeTarget where T : Object
    {
        public T mTarget;

        public override bool IsValid(Localize cmp)
        {
            if (mTarget!=null)
            {
                var mTargetCmp = mTarget as Component;
                if (mTargetCmp != null && mTargetCmp.gameObject != cmp.gameObject)
                    mTarget = null;
            }
            if (mTarget==null)
                mTarget = cmp.GetComponent<T>();
            return mTarget!=null;
        }
	}
}

