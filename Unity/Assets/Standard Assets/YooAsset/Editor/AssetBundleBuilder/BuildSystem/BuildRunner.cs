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
					var taskAttribute = task.GetType().GetCustomAttribute<TaskAttribute>();
					if (taskAttribute != null)
						BuildLogger.Log($"---------------------------------------->{taskAttribute.Desc}<---------------------------------------");
					task.Run(context);
					_buildWatch.Stop();

					// 统计耗时
					int seconds = GetBuildSeconds();
					TotalSeconds += seconds;
					if (taskAttribute != null)
						BuildLogger.Log($"{taskAttribute.Desc}耗时：{seconds}秒");
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
			BuildLogger.Log($"构建过程总计耗时：{TotalSeconds}秒");
			return buildResult;
		}

		private static int GetBuildSeconds()
		{
			float seconds = _buildWatch.ElapsedMilliseconds / 1000f;
			return (int)seconds;
		}
	}
}