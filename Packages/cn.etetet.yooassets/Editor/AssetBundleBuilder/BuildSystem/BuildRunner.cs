using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;

namespace YooAsset.Editor
{
    public class BuildRunner
    {
        private static Stopwatch _buildWatch;

        /// <summary>
        /// 总耗时
        /// </summary>
        public static int TotalSeconds = 0;

        /// <summary>
        /// 执行构建流程
        /// </summary>
        /// <returns>如果成功返回TRUE，否则返回FALSE</returns>
        public static BuildResult Run(List<IBuildTask> pipeline, BuildContext context)
        {
            if (pipeline == null)
                throw new ArgumentNullException("pipeline");
            if (context == null)
                throw new ArgumentNullException("context");

            BuildResult buildResult = new BuildResult();
            buildResult.Success = true;
            TotalSeconds = 0;
            for (int i = 0; i < pipeline.Count; i++)
            {
                IBuildTask task = pipeline[i];
                try
                {
                    _buildWatch = Stopwatch.StartNew();
                    string taskName = task.GetType().Name.Split('_')[0];
                    BuildLogger.Log($"--------------------------------------------->{taskName}<--------------------------------------------");
                    task.Run(context);
                    _buildWatch.Stop();

                    // 统计耗时
                    int seconds = GetBuildSeconds();
                    TotalSeconds += seconds;
                    BuildLogger.Log($"{taskName} It takes {seconds} seconds in total");
                }
                catch (Exception e)
                {
                    EditorTools.ClearProgressBar();
                    buildResult.FailedTask = task.GetType().Name;
                    buildResult.ErrorInfo = e.ToString();
                    buildResult.Success = false;
                    break;
                }
            }

            // 返回运行结果
            BuildLogger.Log($"Total build process time: {TotalSeconds} seconds");
            return buildResult;
        }

        private static int GetBuildSeconds()
        {
            float seconds = _buildWatch.ElapsedMilliseconds / 1000f;
            return (int)seconds;
        }
    }
}