using System.Collections.Generic;

namespace ET
{
    public static class UnitHelper
    {
        public static UnitInfo CreateUnitInfo(Unit unit)
        {
            UnitInfo unitInfo = new UnitInfo();
            NumericComponent nc = unit.GetComponent<NumericComponent>();
            unitInfo.X = unit.Position.x;
            unitInfo.Y = unit.Position.y;
            unitInfo.Z = unit.Position.z;
            unitInfo.UnitId = unit.Id;
            unitInfo.ConfigId = unit.ConfigId;

            foreach ((int key, long value) in nc.NumericDic)
            {
                unitInfo.Ks.Add(key);
                unitInfo.Vs.Add(value);
            }

            return unitInfo;
        }
        
        // 获取看见unit的玩家，主要用于广播
        public static Dictionary<long, AOIEntity> GetBeSeePlayers(this Unit self)
        {
            return self.GetComponent<AOIEntity>().GetBeSeePlayers();
        }
        
        public static void NoticeUnitAdd(Unit unit, Unit sendUnit)
        {
            M2C_CreateUnits createUnits = new M2C_CreateUnits();
            createUnits.Units.Add(CreateUnitInfo(sendUnit));
            MessageHelper.SendToClient(unit, createUnits);
        }
        
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit)
        {
            M2C_RemoveUnits removeUnits = new M2C_RemoveUnits();
            removeUnits.Units.Add(sendUnit.Id);
            MessageHelper.SendToClient(unit, removeUnits);
        }
    }
}