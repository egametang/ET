using System;
using TrueSync;
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

            Vector3 unitPos = unit.Position.ToVector();
            const float speed = 6f;  
            
            if (unitPos != self.Position)
            {
                float distance = (unitPos - self.Position).magnitude;
                self.totalTime = distance / speed;
                self.t = 0;
                self.Position = unit.Position.ToVector();
                self.Rotation = unit.Rotation.ToQuaternion();
            }
            
            LSInput input = unit.GetComponent<LSInputComponent>().LSInput;
            if (input.V != TSVector2.zero)
            {
                self.GetComponent<LSAnimatorComponent>().SetFloatValue("Speed", speed);
            }
            else
            {
                self.GetComponent<LSAnimatorComponent>().SetFloatValue("Speed", 0);
            }

            self.t += Time.deltaTime;
            self.Transform.rotation = Quaternion.Lerp(self.Transform.rotation, self.Rotation, self.t / 1f);
            self.Transform.position = Vector3.Lerp(self.Transform.position, self.Position, self.t / self.totalTime);
        }

        private static LSUnit GetUnit(this LSUnitView self)
        {
            LSUnit unit = self.Unit;
            if (unit != null)
            {
                return unit;
            }

            self.Unit = (self.Domain as Room).GetComponent<LSWorld>().GetComponent<LSUnitComponent>().GetChild<LSUnit>(self.Id);
            return self.Unit;
        }
    }
}