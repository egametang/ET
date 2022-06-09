using System;

namespace ET
{
    /// <summary>
    /// 同一块地图可能有多种寻路数据，玩家可以随时切换，怪物也可能跟玩家的寻路不一样，寻路组件应该挂在Unit上
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class PathfindingComponent: Entity, IAwake<string>, IDestroy
    {
        public static int FindRandomNavPosMaxRadius = 15000;  // 随机找寻路点的最大半径
        
        public static float[] extents = {15, 10, 15};
        
        public string Name;
        
        public long NavMesh;

        [NoMemoryCheck]
        public float[] StartPos = new float[3];

        [NoMemoryCheck]
        public float[] EndPos = new float[3];

        [NoMemoryCheck]
        public float[] Result = new float[Recast.MAX_POLYS * 3];
    }
}