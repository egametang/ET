using UnityEngine;

namespace ET
{
    public class AI_Attack: AAIHandler
    {
        public override int Check(AIComponent aiComponent, AIConfig aiConfig)
        {
            long sec = TimeHelper.ClientNow() / 1000 % 15;
            if (sec >= 10)
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

            // 停在当前位置
            zoneScene.GetComponent<SessionComponent>().Session.Send(new C2M_Stop());
            
            Log.Debug("开始攻击");

            for (int i = 0; i < 100000; ++i)
            {
                Log.Debug($"攻击: {i}次");

                // 因为协程可能被中断，任何协程都要传入cancellationToken，判断如果是中断则要返回
                bool timeRet = await TimerComponent.Instance.WaitAsync(1000, cancellationToken);
                if (!timeRet)
                {
                    return;
                }
            }
        }
    }
}