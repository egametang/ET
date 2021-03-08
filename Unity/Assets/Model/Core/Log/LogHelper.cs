using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !SERVER
namespace ET
{
    public static class LogHelper
    {
        public static void Debug(this Entity entity, string msg)
        {
            Log.Debug($"Zone:{entity.DomainZone()} | {msg}");
        }

        public static void Info(this Entity entity, string msg)
        {
            Log.Info($"Zone:{entity.DomainZone()} | {msg}");
        }

        public static void Error(this Entity entity, string msg)
        {
            Log.Info($"Zone:{entity.DomainZone()} | {msg}");
            Log.Error($"Zone:{entity.DomainZone()} | {msg}");
        }

        public static void Trace(this Entity entity, string msg)
        {
            Log.Trace($"Zone:{entity.DomainZone()} | {msg}");
        }

        public static void TraceInfo(this Entity entity, string msg)
        {
            Log.TraceInfo($"Zone:{entity.DomainZone()} | {msg}");
        }
    }
}
#endif
