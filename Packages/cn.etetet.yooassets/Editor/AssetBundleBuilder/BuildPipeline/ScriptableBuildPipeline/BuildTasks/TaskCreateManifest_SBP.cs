using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
    public class TaskCreateManifest_SBP : TaskCreateManifest, IBuildTask
    {
        private TaskBuilding_SBP.BuildResultContext _buildResultContext = null;

        void IBuildTask.Run(BuildContext context)
        {
            CreateManifestFile(context);
        }

        protected override string[] GetBundleDepends(BuildContext context, string bundleName)
        {
            if (_buildResultContext == null)
                _buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();

            if (_buildResultContext.Results.BundleInfos.ContainsKey(bundleName) == false)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.NotFoundUnityBundleInBuildResult, $"Not found bundle in engine build result : {bundleName}");
                throw new Exception(message);
            }
            return _buildResultContext.Results.BundleInfos[bundleName].Dependencies;
        }
    }
}