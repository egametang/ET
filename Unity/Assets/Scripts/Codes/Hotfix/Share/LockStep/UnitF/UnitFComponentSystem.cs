using System;

namespace ET
{
    public static class UnitFComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<UnitFComponent>
        {
            protected override void Awake(UnitFComponent self)
            {
            }
        }
    }
}