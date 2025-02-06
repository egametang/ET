using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskCopyBuildinFiles_RFBP : TaskCopyBuildinFiles, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;
            var manifestContext = context.GetContextObject<ManifestContext>();

            if (buildParameters.BuildMode != EBuildMode.SimulateBuild)
            {
                if (buildParameters.BuildinFileCopyOption != EBuildinFileCopyOption.None)
                {
                    CopyBuildinFilesToStreaming(buildParametersContext, manifestContext.Manifest);
                }
            }
        }
    }
}