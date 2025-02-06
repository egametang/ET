using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public class BuiltinBuildPipeline : IBuildPipeline
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
                    new TaskPrepare_BBP(),
                    new TaskGetBuildMap_BBP(),
                    new TaskBuilding_BBP(),
                    new TaskVerifyBuildResult_BBP(),
                    new TaskEncryption_BBP(),
                    new TaskUpdateBundleInfo_BBP(),
                    new TaskCreateManifest_BBP(),
                    new TaskCreateReport_BBP(),
                    new TaskCreatePackage_BBP(),
                    new TaskCopyBuildinFiles_BBP(),
                };
            return pipeline;
        }
    }
}