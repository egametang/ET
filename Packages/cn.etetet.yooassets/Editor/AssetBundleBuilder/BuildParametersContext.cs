using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
    public class BuildParametersContext : IContextObject
    {
        /// <summary>
        /// 构建参数
        /// </summary>
        public BuildParameters Parameters { private set; get; }


        public BuildParametersContext(BuildParameters parameters)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// 检测构建参数是否合法
        /// </summary>
        public void CheckBuildParameters()
        {
            Parameters.CheckBuildParameters();
        }

        /// <summary>
        /// 获取构建管线的输出目录
        /// </summary>
        /// <returns></returns>
        public string GetPipelineOutputDirectory()
        {
            return Parameters.GetPipelineOutputDirectory();
        }

        /// <summary>
        /// 获取本次构建的补丁输出目录
        /// </summary>
        public string GetPackageOutputDirectory()
        {
            return Parameters.GetPackageOutputDirectory();
        }

        /// <summary>
        /// 获取本次构建的补丁根目录
        /// </summary>
        public string GetPackageRootDirectory()
        {
            return Parameters.GetPackageRootDirectory();
        }

        /// <summary>
        /// 获取内置资源的根目录
        /// </summary>
        public string GetBuildinRootDirectory()
        {
            return Parameters.GetBuildinRootDirectory();
        }
    }
}