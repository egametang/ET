using System.Collections.Generic;
using System.Diagnostics;

namespace ET
{
    public static class OpcodeHelper
    {
        [StaticField]
        private static readonly HashSet<ushort> ignoreDebugLogMessageSet = new HashSet<ushort>
        {
            OuterMessage.C2G_Ping,
            OuterMessage.G2C_Ping,
            OuterMessage.C2G_Benchmark,
            OuterMessage.G2C_Benchmark,
            LockStepOuter.OneFrameInputs,
            LockStepOuter.FrameMessage,
            LockStepOuter.C2Room_CheckHash,
            ushort.MaxValue, // ActorResponse
        };

        private static bool IsNeedLogMessage(ushort opcode)
        {
            if (ignoreDebugLogMessageSet.Contains(opcode))
            {
                return false;
            }

            return true;
        }

        public static bool IsOuterMessage(ushort opcode)
        {
            return opcode < OpcodeRangeDefine.OuterMaxOpcode;
        }

        public static bool IsInnerMessage(ushort opcode)
        {
            return opcode >= OpcodeRangeDefine.InnerMinOpcode;
        }

        [Conditional("DEBUG")]
        public static void LogMsg(this Entity entity, object message)
        {
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            if (!IsNeedLogMessage(opcode))
            {
                return;
            }
            
            Logger.Instance.Debug($"{entity.Domain.SceneType} {message}");
        }
    }
}