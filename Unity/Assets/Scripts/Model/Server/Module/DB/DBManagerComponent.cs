using System;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class DBManagerComponent: Entity, IAwake
    {
        public DBComponent[] DBComponents = new DBComponent[IdGenerater.MaxZone];
    }
}