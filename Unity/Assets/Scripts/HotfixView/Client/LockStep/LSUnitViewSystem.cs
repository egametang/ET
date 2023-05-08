using System;
using UnityEngine;

namespace ET.Client
{
    public static class LSUnitViewSystem
    {
        public class AwakeSystem: AwakeSystem<LSUnitView, GameObject>
        {
            protected override void Awake(LSUnitView self, GameObject go)
            {
                self.GameObject = go;
                self.Transform = go.transform;
            }
        }
        
        public class UpdateSystem: UpdateSystem<LSUnitView>
        {
            protected override void Update(LSUnitView self)
            {
                self.Update();
            }
        }

        private static void Update(this LSUnitView self)
        {
            LSUnit unit = self.GetUnit();
                
            Vector3 pos = self.Transform.position;
            Vector3 to = unit.Position.ToVector();
            float distance = (to - pos).magnitude;
            //if (distance < 0.5)
            //{
            //    return;
            //}
            float t = distance / 9f;
            self.Transform.position = Vector3.Lerp(pos, unit.Position.ToVector(), Time.deltaTime / t);
        }

        public static LSUnit GetUnit(this LSUnitView self)
        {
            LSUnit unit = self.Unit;
            if (unit != null)
            {
                return unit;
            }

            self.Unit = (self.Domain as Room).LSWorld.GetComponent<LSUnitComponent>().GetChild<LSUnit>(self.Id);
            return self.Unit;
        }
    }
}