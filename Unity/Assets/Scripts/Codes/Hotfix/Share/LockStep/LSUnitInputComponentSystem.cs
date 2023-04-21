using System;

namespace ET
{
    public static class LSUnitInputComponentSystem
    {
        [ObjectSystem]
        public class LSUpdateSystem: LSUpdateSystem<LSUnitInputComponent>
        {
            protected override void LSUpdate(LSUnitInputComponent self)
            {
                
            }
        }
    }
}