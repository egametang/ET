using System;


namespace ET
{
    public static class EnterMapHelper
    {
        public static async ETTask EnterMapAsync(Scene zoneScene)
        {
            try
            {
                G2C_EnterMap g2CEnterMap = await zoneScene.GetComponent<SessionComponent>().Session.Call(new C2G_EnterMap()) as G2C_EnterMap;

                UnitComponent unitComponent = zoneScene.GetComponent<UnitComponent>();
                unitComponent.MyId = g2CEnterMap.MyId;
                
                Game.EventSystem.Publish(new EventType.EnterMapFinish() {ZoneScene = zoneScene});
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
    }
}