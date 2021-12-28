using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class MoveHelper
    {
        // 可以多次调用，多次调用的话会取消上一次的协程
        public static async ETTask<int> MoveToAsync(this Unit unit, Vector3 targetPos, ETCancellationToken cancellationToken = null)
        {
            C2M_PathfindingResult msg = new C2M_PathfindingResult() {X = targetPos.x, Y = targetPos.y, Z = targetPos.z};
            unit.Domain.GetComponent<SessionComponent>().Session.Send(msg);

            ObjectWait objectWait = unit.GetComponent<ObjectWait>();
            
            // 要取消上一次的移动协程
            objectWait.Notify(new WaitType.Wait_UnitStop() { Error = WaitTypeError.Cancel });
            
            // 一直等到unit发送stop
            WaitType.Wait_UnitStop waitUnitStop = await objectWait.Wait<WaitType.Wait_UnitStop>(cancellationToken);
            return waitUnitStop.Error;
        }
        
        public static async ETTask<bool> MoveToAsync(this Unit unit, List<Vector3> path)
        {
            float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
            bool ret = await moveComponent.MoveToAsync(path, speed);
            return ret;
        }
    }
}