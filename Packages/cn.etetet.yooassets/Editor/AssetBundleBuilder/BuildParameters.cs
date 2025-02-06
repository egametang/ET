using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建参数
    /// </summary>
    public abstract class BuildParameters
    {
        /// <summary>
        /// 构建输出的根目录
        /// </summary>
        public string BuildOutputRoot;

        /// <summary>
        /// 内置文件的根目录
        /// </summary>
        public string BuildinFileRoot;

        /// <summary>
        /// 构建管线
        /// </summary>
        public string BuildPipeline;

        /// <summary>
        /// 构建的平台
        /// </summary>
        public BuildTarget BuildTarget;

        /// <summary>
        /// 构建模式
        /// </summary>
        public EBuildMode BuildMode;

        /// <summary>
        /// 构建的包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 构建的包裹版本
        /// </summary>
        public string PackageVersion;


        /// <summary>
        /// 是否启用共享资源打包
        /// </summary>
        public bool EnableSharePackRule = false;

        /// <summary>
        /// 验证构建结果
        /// </summary>
        public bool VerifyBuildingResult = false;

        /// <summary>
        /// 资源包名称样式
        /// </summary>
        public EFileNameStyle FileNameStyle;

        /// <summary>
        /// 内置文件的拷贝选项
        /// </summary>
        public EBuildinFileCopyOption BuildinFileCopyOption;

        /// <summary>
        /// 内置文件的拷贝参数
        /// </summary>
        public string BuildinFileCopyParams;

        /// <summary>
        /// 资源包加密服务类
        /// </summary>
        public IEncryptionServices EncryptionServices;


        private string _pipelineOutputDirectory = string.Empty;
        private string _packageOutputDirectory = string.Empty;
        private string _packageRootDirectory = string.Empty;
        private string _buildinRootDirectory = string.Empty;

        /// <summary>
        /// 检测构建参数是否合法
        /// </summary>
        public virtual void CheckBuildParameters()
        {
            // 检测当前是否正在构建资源包
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.ThePipelineIsBuiding, "The pipeline is buiding, please try again after finish !");
                throw new Exception(message);
            }

            // 检测是否有未保存场景
            if (BuildMode != EBuildMode.SimulateBuild)
            {
                if (EditorTools.HasDirtyScenes())
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.FoundUnsavedScene, "Found unsaved scene !");
                    throw new Exception(message);
                }
            }

            // 检测构建参数合法性
            if (BuildTarget == BuildTarget.NoTarget)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.NoBuildTarget, "Please select the build target platform !");
                throw new Exception(message);
            }
            if (string.IsNullOrEmpty(PackageName))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.PackageNameIsNullOrEmpty, "Package name is null or empty !");
                throw new Exception(message);
            }
            if (string.IsNullOrEmpty(PackageVersion))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.PackageVersionIsNullOrEmpty, "Package version is null or empty !");
                throw new Exception(message);
            }
            if (string.IsNullOrEmpty(BuildOutputRoot))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildOutputRootIsNullOrEmpty, "Build output root is null or empty !");
                throw new Exception(message);
            }
            if (string.IsNullOrEmpty(BuildinFileRoot))
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildinFileRootIsNullOrEmpty, "Buildin file root is null or empty !");
                throw new Exception(message);
            }

            // 强制构建删除包裹目录
            if (BuildMode == EBuildMode.ForceRebuild)
            {
                string packageRootDirectory = GetPackageRootDirectory();
                if (EditorTools.DeleteDirectory(packageRootDirectory))
                {
                    BuildLogger.Log($"Delete package root directory: {packageRootDirectory}");
                }
            }

            // 检测包裹输出目录是否存在
            if (BuildMode != EBuildMode.SimulateBuild)
            {
                string packageOutputDirectory = GetPackageOutputDirectory();
                if (Directory.Exists(packageOutputDirectory))
                {
                    string message = BuildLogger.GetErrorMessage(ErrorCode.PackageOutputDirectoryExists, $"Package outout directory exists: {packageOutputDirectory}");
                    throw new Exception(message);
                }
            }

            // 如果输出目录不存在
            string pipelineOutputDirectory = GetPipelineOutputDirectory();
            if (EditorTools.CreateDirectory(pipelineOutputDirectory))
            {
                BuildLogger.Log($"Create pipeline output directory: {pipelineOutputDirectory}");
            }
        }


        /// <summary>
        /// 获取构建管线的输出目录
        /// </summary>
        /// <returns></returns>
        public virtual string GetPipelineOutputDirectory()
        {
            if (string.IsNullOrEmpty(_pipelineOutputDirectory))
            {
                _pipelineOutputDirectory = $"{BuildOutputRoot}/{BuildTarget}/{PackageName}/{YooAssetSettings.OutputFolderName}";
            }
            return _pipelineOutputDirectory;
        }

        /// <summary>
        /// 获取本次构建的补丁输出目录
        /// </summary>
        public virtual string GetPackageOutputDirectory()
        {
            if (string.IsNullOrEmpty(_packageOutputDirectory))
            {
                _packageOutputDirectory = $"{BuildOutputRoot}/{BuildTarget}/{PackageName}/{PackageVersion}";
            }
            return _packageOutputDirectory;
        }

        /// <summary>
        /// 获取本次构建的补丁根目录
        /// </summary>
        public virtual string GetPackageRootDirectory()
        {
            if (string.IsNullOrEmpty(_packageRootDirectory))
            {
                _packageRootDirectory = $"{BuildOutputRoot}/{BuildTarget}/{PackageName}";
            }
            return _packageRootDirectory;
        }

        /// <summary>
        /// 获取内置资源的根目录
        /// </summary>
        public virtual string GetBuildinRootDirectory()
        {
            if (string.IsNullOrEmpty(_buildinRootDirectory))
            {
                _buildinRootDirectory = $"{BuildinFileRoot}/{PackageName}";
            }
            return _buildinRootDirectory;
        }
    }
}