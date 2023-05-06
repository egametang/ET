using System;
using TrueSync;

namespace ET
{
    public static class LSInputComponentSystem
    {
        [ObjectSystem]
        public class LSUpdateSystem: LSUpdateSystem<LSInputComponent>
        {
            protected override void LSUpdate(LSInputComponent self)
            {
                LSUnit unit = self.GetParent<LSUnit>();

                TSVector2 v2 = self.LSInput.V * 6 * 50 / 1000;
                unit.Position += new TSVector(v2.x, 0, v2.y);
            }
        }
    }
}