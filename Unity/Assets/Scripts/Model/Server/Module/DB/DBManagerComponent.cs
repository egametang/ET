using System;

namespace ET.Server
{
    [ComponentOf(typeof(VProcess))]
    public class DBManagerComponent: SingletonEntity<DBManagerComponent>, IAwake
    {
        public DBComponent[] DBComponents = new DBComponent[IdGenerater.MaxZone];
    }
}