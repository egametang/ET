using System;

namespace ET.Server
{
    [ComponentOf(typeof(Fiber))]
    public class DBManagerComponent: Entity, IAwake
    {
        public DBComponent[] DBComponents = new DBComponent[IdGenerater.MaxZone];
    }
}