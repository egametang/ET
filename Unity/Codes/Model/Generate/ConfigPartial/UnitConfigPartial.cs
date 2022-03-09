using System.Numerics;

namespace ET
{
    public class TestVector3
    {
        public Vector3 Position;
    }
    
    public partial class UnitConfigCategory : ProtoObject
    {
        public UnitConfig GetUnitConfigByHeight(int height)
        {
            UnitConfig unitConfig = null;

            foreach (var info in this.dict.Values)
            {
                
                if (info.Height == height)
                {
                    unitConfig = info;
                    break;
                }
            }
            return unitConfig;
        }

        public override void AfterEndInit()
        {
            base.AfterEndInit();

            foreach (var info in this.dict.Values)
            {
                info.TestValue = new Vector2(info.Height,info.Weight);
                info.TestVector3 = new TestVector3(){Position = new Vector3(info.Position[0],info.Position[1],info.Position[2])};
            }
        }
    }

    public partial class UnitConfig: ProtoObject, IConfig
    {
        public Vector2 TestValue;
        public TestVector3 TestVector3;
    }
}