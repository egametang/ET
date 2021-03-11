using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ET
{
    public static class UnitPathComponentHelper
    {
        public static async ETTask MoveAsync(this UnitPathComponent self, List<Vector3> path)
        {
            if (path.Count == 0)
            {
                return;
            }
            // 第一个点是unit的当前位置，所以不用发送
            for (int i = 1; i < path.Count; ++i)
            {
                // 每移动3个点发送下3个点给客户端
                if (i % 3 == 1)
                {
                    self.BroadcastPath(path, i, 3);
                }
                Vector3 v3 = path[i];
                await self.Parent.GetComponent<MoveComponent>().MoveToAsync(v3, self.CancellationToken);
            }
            await ETTask.CompletedTask;
        }
        
        public static async ETVoid MoveTo(this UnitPathComponent self, Vector3 target)
        {
            if ((self.Target - target).magnitude < 0.1f)
            {
                return;
            }

            self.Target = target;

            Unit unit = self.GetParent<Unit>();

            RecastPathComponent recastPathComponent = self.Domain.GetComponent<RecastPathComponent>();
            RecastPath recastPath = EntityFactory.Create<RecastPath>((Entity)self.Domain);
            recastPath.StartPos = unit.Position;
            recastPath.EndPos = new Vector3(target.x, target.y, target.z);
            self.RecastPath = recastPath;
            //TODO 因为目前阶段只有一张地图，所以默认mapId为10001
            recastPathComponent.SearchPath(10001, self.RecastPath);
            //Log.Debug($"------start Pos: {self.RecastPath.StartPos}\n------end Pos: {self.RecastPath.EndPos}\n------find result: {self.RecastPath.Results.ListToString()}");

            self.CancellationToken?.Cancel();
            self.CancellationToken = new ETCancellationToken();
            await self.MoveAsync(self.RecastPath.Results);
            self.CancellationToken = null;
        }

        // 从index找接下来3个点，广播
        public static void BroadcastPath(this UnitPathComponent self, List<Vector3> path, int index, int offset)
        {
            Unit unit = self.GetParent<Unit>();
            Vector3 unitPos = unit.Position;
            M2C_PathfindingResult m2CPathfindingResult = new M2C_PathfindingResult();
            m2CPathfindingResult.X = unitPos.x;
            m2CPathfindingResult.Y = unitPos.y;
            m2CPathfindingResult.Z = unitPos.z;
            m2CPathfindingResult.Id = unit.Id;

            for (int i = 0; i < offset; ++i)
            {
                if (index + i >= self.RecastPath.Results.Count)
                {
                    break;
                }
                Vector3 v = self.RecastPath.Results[index + i];
                m2CPathfindingResult.Xs.Add(v.x);
                m2CPathfindingResult.Ys.Add(v.y);
                m2CPathfindingResult.Zs.Add(v.z);
            }
            MessageHelper.Broadcast(self.GetParent<Unit>(), m2CPathfindingResult) ;
        }
    }
}