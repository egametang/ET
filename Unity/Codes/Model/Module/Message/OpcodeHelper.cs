using System.Collections.Generic;

namespace ET
{
    public static class OpcodeHelper
    {
        private static readonly HashSet<ushort> ignoreDebugLogMessageSet = new HashSet<ushort>
        {
            OuterOpcode.C2G_Ping,
            OuterOpcode.G2C_Ping,
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
            return opcode >= 20000;
        }

        public static bool IsInnerMessage(ushort opcode)
        {
            return opcode < 20000;
        }

        public static void LogMsg(int zone, ushort opcode, object message)
        {
            if (!IsNeedLogMessage(opcode))
            {
                return;
            }
            
            Game.ILog.Debug("zone: {0} {1}", zone, message);
        }
        
        public static void LogMsg(ushort opcode, long actorId, object message)
        {
            if (!IsNeedLogMessage(opcode))
            {
                return;
            }
            
            Game.ILog.Debug("actorId: {0} {1}", actorId, message);
        }
    }
}