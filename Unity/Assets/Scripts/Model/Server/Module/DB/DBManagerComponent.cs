using System;

namespace ET.Server
{
    [ComponentOf(typeof(Fiber))]
    public class DBManagerComponent: SingletonEntity<DBManagerComponent>, IAwake
    {
        public DBComponent[] DBComponents = new DBComponent[IdGenerater.MaxZone];
    }
}