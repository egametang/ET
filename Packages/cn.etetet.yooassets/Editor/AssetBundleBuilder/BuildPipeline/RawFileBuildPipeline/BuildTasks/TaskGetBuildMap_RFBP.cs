using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class TaskGetBuildMap_RFBP : TaskGetBuildMap, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = CreateBuildMap(buildParametersContext.Parameters);
            context.SetContextObject(buildMapContext);

            // 检测构建结果
            CheckBuildMapContent(buildMapContext);
        }

        /// <summary>
        /// 检测资源构建上下文
        /// </summary>
        private void CheckBuildMapContent(BuildMapContext buildMapContext)
        {
            // 注意：原生文件资源包只能包含一个原生文件
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                if (bundleInfo.MainAssets.Count != 1)
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.NotSupportMultipleRawAsset, $"The bundle does not support multiple raw asset : {bundleInfo.BundleName}");
                    throw new Exception(message);
                }
            }
        }
    }
}