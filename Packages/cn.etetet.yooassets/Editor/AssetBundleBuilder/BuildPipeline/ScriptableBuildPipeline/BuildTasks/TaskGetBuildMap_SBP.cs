using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskGetBuildMap_SBP : TaskGetBuildMap, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = CreateBuildMap(buildParametersContext.Parameters);
            context.SetContextObject(buildMapContext);
        }
    }
}