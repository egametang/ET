using System.Collections.Generic;
using System.Diagnostics;

namespace ET
{
    public class LogMsg: Singleton<LogMsg>, ISingletonAwake
    {
        private readonly HashSet<ushort> ignore = new();
            //OuterMessage.C2G_Ping, 
            //OuterMessage.G2C_Ping, 
            //OuterMessage.C2G_Benchmark, 
            //OuterMessage.G2C_Benchmark,

        public void Awake()
        {
        }

        public void AddIgnore(ushort opcode)
        {
            this.ignore.Add(opcode);
        }

        [Conditional("DEBUG")]
        public void Debug(Fiber fiber, object msg)
        {
            ushort opcode = OpcodeType.Instance.GetOpcode(msg.GetType());
            if (this.ignore.Contains(opcode))
            {
                return;
            }
            fiber.Log.Debug(msg.ToString());
        }
    }
}