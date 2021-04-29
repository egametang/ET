using CommandLine;
using System;
using System.Collections.Generic;

namespace ET
{
    public enum ServerType
    {
        Game,
        Watcher,
    }
    
    public class Options
    {
        [Option("ServerType", Required = false, Default = ServerType.Game, HelpText = "serverType enum")]
        public ServerType ServerType { get; set; }

        [Option("Process", Required = false, Default = 1)]
        public int Process { get; set; } = 1;
        
        [Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; } = 0;

        [Option("LogLevel", Required = false, Default = 0)]
        public int LogLevel { get; set; } = 2;
    }
}