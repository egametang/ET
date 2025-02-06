using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class ScriptableBuildPipeline : IBuildPipeline
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
                    new TaskPrepare_SBP(),
                    new TaskGetBuildMap_SBP(),
                    new TaskBuilding_SBP(),
                    new TaskVerifyBuildResult_SBP(),
                    new TaskEncryption_SBP(),
                    new TaskUpdateBundleInfo_SBP(),
                    new TaskCreateManifest_SBP(),
                    new TaskCreateReport_SBP(),
                    new TaskCreatePackage_SBP(),
                    new TaskCopyBuildinFiles_SBP(),
                };
            return pipeline;
        }
    }
}