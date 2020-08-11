using System;
using UnityEngine;

namespace ET
{
    public class OperaComponentAwakeSystem : AwakeSystem<OperaComponent>
    {
        public override void Awake(OperaComponent self)
        {
            self.mapMask = LayerMask.GetMask("Map");
        }
    }

    public class OperaComponentUpdateSystem : UpdateSystem<OperaComponent>
    {
        public override void Update(OperaComponent self)
        {
            self.Update();
        }
    }
    
    public static class OperaComponentSystem
    {
        public static void Update(this OperaComponent self)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000, self.mapMask))
                {
                    self.ClickPoint = hit.point;
                    self.frameClickMap.X = self.ClickPoint.x;
                    self.frameClickMap.Y = self.ClickPoint.y;
                    self.frameClickMap.Z = self.ClickPoint.z;
                    self.ZoneScene().GetComponent<SessionComponent>().Session.Send(self.frameClickMap);

                    // 测试actor rpc消息
                    self.TestActor().Coroutine();
                }
            }
        }

        public static async ETVoid TestActor(this OperaComponent self)
        {
            try
            {
                M2C_TestActorResponse response = (M2C_TestActorResponse)await self.ZoneScene().GetComponent<SessionComponent>().Session.Call(
                    new C2M_TestActorRequest() { Info = "actor rpc request" });
                Log.Info(response.Info);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}