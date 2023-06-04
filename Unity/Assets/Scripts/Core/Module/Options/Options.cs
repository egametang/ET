using CommandLine;
using System;
using System.Collections.Generic;

namespace ET
{
    // 应用程序类型
    public enum AppType
    {
        Server,           // 服务器类型
        Watcher,          // 每台物理机一个守护进程，用来启动该物理机上的所有进程
        GameTool,         // 游戏工具类型
        ExcelExporter,    // Excel导出器类型
        Proto2CS,         // Proto转CS类型
        BenchmarkClient,  // 基准测试客户端类型
        BenchmarkServer,  // 基准测试服务器类型
    }
    
    public class Options: Singleton<Options>
    {
        /// <summary>
        /// 应用程序类型，不是必需的，默认值为AppType.Server
        /// </summary>
        [Option("AppType", Required = false, Default = AppType.Server, HelpText = "AppType enum")]
        public AppType AppType { get; set; }

        /// <summary>
        /// 启动配置文件路径，不是必需的，默认值为"StartConfig/Localhost"
        /// </summary>
        [Option("StartConfig", Required = false, Default = "StartConfig/Localhost")]
        public string StartConfig { get; set; }

        /// <summary>
        /// / 进程编号，整数类型，不是必需的，默认值为1
        /// </summary>
        [Option("Process", Required = false, Default = 1)]
        public int Process { get; set; }

        /// <summary>
        /// 开发模式，整数类型，不是必需的，默认值为0
        /// </summary>
        [Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; }

        /// <summary>
        /// 日志等级，整数类型，不是必需的，默认值为2
        /// </summary>
        [Option("LogLevel", Required = false, Default = 2)]
        public int LogLevel { get; set; }

        /// <summary>
        /// 是否启用控制台，整数类型，不是必需的，默认值为0
        /// </summary>
        [Option("Console", Required = false, Default = 0)]
        public int Console { get; set; }

        /// <summary>
        /// 进程启动是否创建该进程的scenes
        /// 是否创建场景，整数类型，不是必需的，默认值为1
        /// </summary>
        [Option("CreateScenes", Required = false, Default = 1)]
        public int CreateScenes { get; set; }
    }
}