using System;

namespace ET.Server
{
    
    public class DBManagerComponent: Entity, IAwake, IDestroy
    {
        [ThreadStatic]
        [StaticField]
        public static DBManagerComponent Instance;
        
        public DBComponent[] DBComponents = new DBComponent[IdGenerater.MaxZone];
    }
}