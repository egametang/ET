using Unity.Mathematics;

namespace ET.Client
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
            Scene clientScene = aiComponent.DomainScene();

            Unit myUnit = UnitHelper.GetMyUnitFromClientScene(clientScene);
            if (myUnit == null)
            {
                return;
            }
            
            Log.Debug("开始巡逻");

            while (true)
            {
                XunLuoPathComponent xunLuoPathComponent = myUnit.GetComponent<XunLuoPathComponent>();
                float3 nextTarget = xunLuoPathComponent.GetCurrent();
                await myUnit.MoveToAsync(nextTarget, cancellationToken);
                if (cancellationToken.IsCancel())
                {
                    return;
                }
                xunLuoPathComponent.MoveNext();
            }
        }
    }
}