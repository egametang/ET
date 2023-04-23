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
                LSWorld lsWorld = self.GetParent<Room>().LSWorld;
                foreach (LSUnitView child in self.Children.Values)
                {
                    LSUnit unit = lsWorld.Get(child.Id) as LSUnit;

                    Vector3 pos = child.Transform.position;
                    Vector3 to = unit.Position.ToVector();
                    float t = (to - pos).magnitude / 9f;
                    
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