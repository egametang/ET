using System;
using UnityEngine;

namespace ET
{
    public class MoveComponentUpdateSystem: UpdateSystem<MoveComponent>
    {
        public override void Update(MoveComponent self)
        {
            if (self.moveTcs == null)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            long timeNow = TimeHelper.ClientNow();

            if (timeNow - self.StartTime >= self.needTime)
            {
                unit.Position = self.Target;
                ETTaskCompletionSource tcs = self.moveTcs;
                self.moveTcs = null;
                tcs.SetResult();
                return;
            }

            float amount = (timeNow - self.StartTime) * 1f / self.needTime;
            unit.Position = Vector3.Lerp(self.StartPos, self.Target, amount);
        }
    }

    public static class MoveComponentSystem
    {
        public static async ETTask MoveToAsync(this MoveComponent self,Vector3 target, float speedValue, ETCancellationToken cancellationToken)
        {
            Unit unit = self.GetParent<Unit>();

            if ((target - self.Target).magnitude < 0.1f)
            {
                await ETTask.CompletedTask;
            }

            self.Target = target;

            self.StartPos = unit.Position;
            self.StartTime = TimeHelper.ClientNow();
            float distance = (self.Target - self.StartPos).magnitude;
            if (Math.Abs(distance) < 0.1f)
            {
                await ETTask.CompletedTask;
            }

            self.needTime = (long) (distance / speedValue * 1000);

            self.moveTcs = new ETTaskCompletionSource();

            cancellationToken.Add(() => { self.moveTcs = null; });
            await self.moveTcs.Task;
        }
    }
}