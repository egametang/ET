using UnityEngine;

namespace ET
{
    public class AI_XunLuo: AAIHandler
    {
        public override int Check(AIComponent aiComponent, AIConfig aiConfig)
        {
            long sec = TimeHelper.ClientNow() / 1000 % 15;
            if (sec < 10)
            {
                return 0;
            }
            return 1;
        }

        public override async ETTask Execute(AIComponent aiComponent, AIConfig aiConfig, ETCancellationToken cancellationToken)
        {
            Scene zoneScene = aiComponent.DomainScene();

            Unit myUnit = UnitHelper.GetMyUnitFromZoneScene(zoneScene);
            if (myUnit == null)
            {
                return;
            }
            
            Log.Debug("开始巡逻");

            while (true)
            {
                XunLuoPathComponent xunLuoPathComponent = myUnit.GetComponent<XunLuoPathComponent>();
                Vector3 nextTarget = xunLuoPathComponent.GetCurrent();
                int ret = await myUnit.MoveToAsync(nextTarget, cancellationToken);
                if (ret != 0)
                {
                    return;
                }
                xunLuoPathComponent.MoveNext();
            }
        }
    }
}