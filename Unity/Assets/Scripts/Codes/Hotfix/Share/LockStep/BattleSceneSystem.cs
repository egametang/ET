using System;
using System.Collections.Generic;

namespace ET
{
    public static class BattleSceneSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<BattleScene>
        {
            protected override void Awake(BattleScene self)
            {
            }
        }

        public static void InitUnit(this BattleScene self, List<LockStepUnitInfo> unitInfos)
        {
            foreach (LockStepUnitInfo lockStepUnitInfo in unitInfos)
            {
                UnitFFactory.Init(self.LSScene, lockStepUnitInfo);
            }
        }
    }
}