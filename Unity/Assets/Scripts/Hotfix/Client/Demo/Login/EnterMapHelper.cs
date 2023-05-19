using System;


namespace ET.Client
{
    public static partial class EnterMapHelper
    {
        public static async ETTask EnterMapAsync(Scene clientScene)
        {
            try
            {
                G2C_EnterMap g2CEnterMap = await clientScene.GetComponent<SessionComponent>().Session.Call(new C2G_EnterMap()) as G2C_EnterMap;
                
                // 等待场景切换完成
                await clientScene.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
                
                EventSystem.Instance.Publish(clientScene, new EventType.EnterMapFinish());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
        
        public static async ETTask Match(Scene clientScene)
        {
            try
            {
                G2C_Match g2CEnterMap = await clientScene.GetComponent<SessionComponent>().Session.Call(new C2G_Match()) as G2C_Match;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }	
        }
    }
}