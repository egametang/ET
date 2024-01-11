using System.Collections.Generic;

namespace I2.Loc
{
    public static partial class LocalizationManager
    {

        #region Variables: Misc

        public static List<ILocalizeTargetDescriptor> mLocalizeTargets = new List<ILocalizeTargetDescriptor>();

        #endregion

        public static void RegisterTarget( ILocalizeTargetDescriptor desc )
        {
            if (mLocalizeTargets.FindIndex(x => x.Name == desc.Name) != -1)
                return;

            for (int i = 0; i < mLocalizeTargets.Count; ++i)
            {
                if (mLocalizeTargets[i].Priority > desc.Priority)
                {
                    mLocalizeTargets.Insert(i, desc);
                    return;
                }
            }
            mLocalizeTargets.Add(desc);
        }
    }
}
