using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskCreateReport_SBP : TaskCreateReport, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParameters = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var manifestContext = context.GetContextObject<ManifestContext>();

            var buildMode = buildParameters.Parameters.BuildMode;
            if (buildMode != EBuildMode.SimulateBuild)
            {
                CreateReportFile(buildParameters, buildMapContext, manifestContext);
            }
        }
    }
}