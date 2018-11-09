using System.Threading;
using Microsoft.CodeAnalysis.Scripting;

namespace ETModel
{
    public class ReplComponent: Component
    {
        public ScriptOptions ScriptOptions;
        public ScriptState ScriptState;
        public CancellationTokenSource CancellationTokenSource;
    }
}