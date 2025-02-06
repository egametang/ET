using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class TaskCreateManifest_BBP : TaskCreateManifest, IBuildTask
    {
        private TaskBuilding_BBP.BuildResultContext _buildResultContext = null;

        void IBuildTask.Run(BuildContext context)
        {
            CreateManifestFile(context);
        }

        protected override string[] GetBundleDepends(BuildContext context, string bundleName)
        {
            if (_buildResultContext == null)
                _buildResultContext = context.GetContextObject<TaskBuilding_BBP.BuildResultContext>();

            return _buildResultContext.UnityManifest.GetAllDependencies(bundleName);
        }
    }
}