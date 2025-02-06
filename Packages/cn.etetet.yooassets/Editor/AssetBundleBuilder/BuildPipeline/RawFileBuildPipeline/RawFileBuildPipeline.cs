using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
    public class RawFileBuildPipeline : IBuildPipeline
    {
        public BuildResult Run(BuildParameters buildParameters, bool enableLog)
        {
            AssetBundleBuilder builder = new AssetBundleBuilder();
            return builder.Run(buildParameters, GetDefaultBuildPipeline(), enableLog);
        }

        /// <summary>
        /// 获取默认的构建流程
        /// </summary>
        private List<IBuildTask> GetDefaultBuildPipeline()
        {
            List<IBuildTask> pipeline = new List<IBuildTask>
                {
                    new TaskPrepare_RFBP(),
                    new TaskGetBuildMap_RFBP(),
                    new TaskBuilding_RFBP(),
                    new TaskUpdateBundleInfo_RFBP(),
                    new TaskCreateManifest_RFBP(),
                    new TaskCreateReport_RFBP(),
                    new TaskCreatePackage_RFBP(),
                    new TaskCopyBuildinFiles_RFBP(),
                };
            return pipeline;
        }
    }
}