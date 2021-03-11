using UnityEngine;

namespace ET
{
    public static class UnitPathComponentSystem
    {
        private static async ETTask StartMove_Internal(this UnitPathComponent self, ETCancellationToken cancellationToken)
        {
            for (int i = 0; i < self.Path.Count; ++i)
            {
                Vector3 v = self.Path[i];

                float speed = 5;

                if (i == 0)
                {
                    // 矫正移动速度
                    Vector3 clientPos = self.GetParent<Unit>().Position;
                    float serverf = (self.ServerPos - v).magnitude;
                    if (serverf > 0.1f)
                    {
                        float clientf = (clientPos - v).magnitude;
                        speed = clientf / serverf * speed;
                    }
                }

                self.Parent.GetComponent<TurnComponent>().Turn(v);
                await self.Parent.GetComponent<MoveComponent>().MoveToAsync(v, speed, cancellationToken);
            }
        }

        public static async ETVoid StartMove(this UnitPathComponent self, M2C_PathfindingResult message)
        {
            // 取消之前的移动协程
            self.ETCancellationToken?.Cancel();
            self.ETCancellationToken = new ETCancellationToken();

            self.Path.Clear();
            for (int i = 0; i < message.Xs.Count; ++i)
            {
                self.Path.Add(new Vector3(message.Xs[i], message.Ys[i], message.Zs[i]));
            }

            self.ServerPos = new Vector3(message.X, message.Y, message.Z);

            await self.StartMove_Internal(self.ETCancellationToken);
            self.ETCancellationToken = null;
        }
    }
}