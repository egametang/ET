using System;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(LSUnitViewComponent))]
    public static class LSUnitViewComponentSystem
    {
        public class AwakeSystem: AwakeSystem<LSUnitViewComponent>
        {
            protected override void Awake(LSUnitViewComponent self)
            {
                self.MyId = self.Parent.GetParent<Scene>().GetComponent<PlayerComponent>().MyId;
            }
        }
        
        public class UpdateSystem: UpdateSystem<LSUnitViewComponent>
        {
            protected override void Update(LSUnitViewComponent self)
            {
                Room room = self.GetParent<Room>();
                LSWorld lsWorld = room.LSWorld;
                foreach (long playerId in room.PlayerIds)
                {
                    LSUnit unit = lsWorld.LSUnitComponent.GetChild<LSUnit>(playerId);
                    LSUnitView child = self.GetChild<LSUnitView>(playerId);
                    Vector3 pos = child.Transform.position;
                    Vector3 to = unit.Position.ToVector();
                    float distance = (to - pos).magnitude;
                    if (distance < 0.5)
                    {
                        continue;
                    }
                    float t = distance / 9f;
                    child.Transform.position = Vector3.Lerp(pos, unit.Position.ToVector(), Time.deltaTime / t);
                }
            }
        }

        public static LSUnitView GetMyLsUnitView(this LSUnitViewComponent self)
        {
            return self.GetChild<LSUnitView>(self.MyId);
        }
    }
}