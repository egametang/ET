using CommandLine;
using System;
using System.Collections.Generic;

namespace ET
{
    public class Options: Singleton<Options>
    {
        [Option("SceneName", Required = false, Default = "Server", HelpText = "define in SceneType class")]
        public string SceneName { get; set; }

        [Option("StartConfig", Required = false, Default = "StartConfig/Localhost")]
        public string StartConfig { get; set; }

        [Option("Process", Required = false, Default = 1)]
        public int Process { get; set; }
        
        [Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; }

        [Option("LogLevel", Required = false, Default = 0)]
        public int LogLevel { get; set; }
        
        [Option("Console", Required = false, Default = 0)]
        public int Console { get; set; }
    }
}