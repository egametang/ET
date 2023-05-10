using System;
using ET.Client;
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
                self.LSUpdate();
            }
        }

        public static void LSUpdate(this LSInputComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();

            TSVector2 v2 = self.LSInput.V * 6 * 50 / 1000;
            if (v2.LengthSquared() < 0.0001f)
            {
                return;
            }
            TSVector oldPos = unit.Position;
            unit.Position += new TSVector(v2.x, 0, v2.y);
            unit.Forward = unit.Position - oldPos;
        }
    }
}