using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    public static partial class MoveHelper
    {
        // 可以多次调用，多次调用的话会取消上一次的协程
        public static async ETTask FindPathMoveToAsync(this Unit unit, float3 target)
        {
            float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
            if (speed < 0.01)
            {
                unit.SendStop(2);
                return;
            }

            M2C_PathfindingResult m2CPathfindingResult = new();
            unit.GetComponent<PathfindingComponent>().Find(unit.Position, target, m2CPathfindingResult.Points);

            if (m2CPathfindingResult.Points.Count < 2)
            {
                unit.SendStop(3);
                return;
            }
                
            // 广播寻路路径
            m2CPathfindingResult.Id = unit.Id;
            MapMessageHelper.Broadcast(unit, m2CPathfindingResult);

            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
            
            bool ret = await moveComponent.MoveToAsync(m2CPathfindingResult.Points, speed);
            if (ret) // 如果返回false，说明被其它移动取消了，这时候不需要通知客户端stop
            {
                unit.SendStop(0);
            }
        }

        public static void Stop(this Unit unit, int error)
        {
            unit.GetComponent<MoveComponent>().Stop(error == 0);
            unit.SendStop(error);
        }

        // error: 0表示协程走完正常停止
        public static void SendStop(this Unit unit, int error)
        {
            MapMessageHelper.Broadcast(unit, new M2C_Stop()
            {
                Error = error,
                Id = unit.Id, 
                Position = unit.Position,
                Rotation = unit.Rotation,
            });
        }
    }
}