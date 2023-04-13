using System;
using System.Collections.Generic;

namespace ET
{
    public static class BattleComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<BattleComponent>
        {
            protected override void Awake(BattleComponent self)
            {
            }
        }

        public static void InitUnit(this BattleComponent self, List<LockStepUnitInfo> unitInfos)
        {
            foreach (LockStepUnitInfo lockStepUnitInfo in unitInfos)
            {
                UnitFFactory.Init(self.LSScene, lockStepUnitInfo);
            }
        }
    }
}