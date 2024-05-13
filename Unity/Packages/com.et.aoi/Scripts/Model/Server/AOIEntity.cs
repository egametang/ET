using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class AOIEntity: Entity, IAwake<int, float3>, IDestroy
    {
        public Unit Unit => this.GetParent<Unit>();

        public int ViewDistance;

        private EntityRef<Cell> cell;

        public Cell Cell
        {
            get
            {
                return this.cell;
            }
            set
            {
                this.cell = value;
            }
        }

        // 观察进入视野的Cell
        public HashSet<long> SubEnterCells = new HashSet<long>();

        // 观察离开视野的Cell
        public HashSet<long> SubLeaveCells = new HashSet<long>();
        
        // 观察进入视野的Cell
        public HashSet<long> enterHashSet = new HashSet<long>();

        // 观察离开视野的Cell
        public HashSet<long> leaveHashSet = new HashSet<long>();

        // 我看的见的Unit
        public Dictionary<long, EntityRef<AOIEntity>> SeeUnits = new Dictionary<long, EntityRef<AOIEntity>>();
        
        // 看见我的Unit
        public Dictionary<long, EntityRef<AOIEntity>> BeSeeUnits = new Dictionary<long, EntityRef<AOIEntity>>();
        
        // 我看的见的Player
        public Dictionary<long, EntityRef<AOIEntity>> SeePlayers = new Dictionary<long, EntityRef<AOIEntity>>();

        // 看见我的Player单独放一个Dict，用于广播
        public Dictionary<long, EntityRef<AOIEntity>> BeSeePlayers = new Dictionary<long, EntityRef<AOIEntity>>();
    }
}