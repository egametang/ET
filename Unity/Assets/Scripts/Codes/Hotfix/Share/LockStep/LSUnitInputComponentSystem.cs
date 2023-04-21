using System;
using TrueSync;

namespace ET
{
    public static class LSUnitInputComponentSystem
    {
        [ObjectSystem]
        public class LSUpdateSystem: LSUpdateSystem<LSUnitInputComponent>
        {
            protected override void LSUpdate(LSUnitInputComponent self)
            {
                LSUnit unit = self.GetParent<LSUnit>();

                TSVector2 v2 = self.LSInputInfo.V * 6 * 50 / 1000;
                unit.Position += new TSVector(v2.x, 0, v2.y);
            }
        }
    }
}