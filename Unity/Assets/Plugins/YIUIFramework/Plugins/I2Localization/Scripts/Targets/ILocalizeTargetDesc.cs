using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace I2.Loc
{
    public abstract class ILocalizeTargetDescriptor
    {
        public string Name;
        public int Priority;
        public abstract bool CanLocalize(Localize cmp);
        public abstract ILocalizeTarget CreateTarget(Localize cmp);
        public abstract Type GetTargetType();
    }

    public abstract class LocalizeTargetDesc<T> : ILocalizeTargetDescriptor where T : ILocalizeTarget
    {
        public override ILocalizeTarget CreateTarget(Localize cmp) { return ScriptableObject.CreateInstance<T>(); }
        public override Type GetTargetType() { return typeof(T); }
    }



    public class LocalizeTargetDesc_Type<T,G> : LocalizeTargetDesc<G> where T: Object 
                                                                      where G: LocalizeTarget<T>
    {
        public override bool CanLocalize(Localize cmp)  { return cmp.GetComponent<T>() != null; }
        public override ILocalizeTarget CreateTarget(Localize cmp)
        {
            var target = cmp.GetComponent<T>();
            if (target == null)
                return null;

            var locTarget = ScriptableObject.CreateInstance<G>();
            locTarget.mTarget = target;
            return locTarget;
        }
    }

}

