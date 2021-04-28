using CommandLine;

namespace ET
{
    public enum ServerType
    {
        Game,
        Watcher,
    }

    public class Options
    {
        //[Option("StartConfig", Required = true)]
        //public string StartConfig { get; set; }

        [Option("ServerType", Required = false, Default = ServerType.Game, HelpText = "serverType enum")]
        public ServerType ServerType { get; set; }

        [Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; }

        [Option("Process", Required = false, Default = 1)]
        public int Process { get; set; }

        [Option("CreateScenes", Required = false, Default = 1)]
        public int CreateScenes { get; set; }

        [Option("Console", Required = false, Default = 0)]
        public int Console { get; set; }

        [Option("LogLevel", Required = false, Default = 0)]
        public int LogLevel { get; set; }
    }
}