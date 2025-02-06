using System.Collections.Generic;
using System.Threading;

namespace ET.Server
{
    public static partial class ConsoleMode
    {
        public const string ReloadDll = "R";
        public const string ReloadConfig = "C";
        public const string ShowMemory = "M";
        public const string Repl = "Repl";
        public const string Debugger = "Debugger";
        public const string CreateRobot = "CreateRobot";
        public const string Robot = "Robot";
    }

    [ComponentOf(typeof(Scene))]
    public class ConsoleComponent: Entity, IAwake
    {
        public CancellationTokenSource CancellationTokenSource;

    }
}