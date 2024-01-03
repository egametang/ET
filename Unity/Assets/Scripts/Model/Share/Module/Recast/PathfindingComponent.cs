using System;
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Detour;

namespace ET
{
    /// <summary>
    /// 同一块地图可能有多种寻路数据，玩家可以随时切换，怪物也可能跟玩家的寻路不一样，寻路组件应该挂在Unit上
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class PathfindingComponent: Entity, IAwake<string>, IDestroy
    {
        public const int MAX_POLYS = 256;
        
        public const int FindRandomNavPosMaxRadius = 15000;  // 随机找寻路点的最大半径
        
        public RcVec3f extents = new(15, 10, 15);
        
        public string Name;
        
        public DtNavMesh navMesh;
        
        public List<long> polys = new(MAX_POLYS);

        public IDtQueryFilter filter;
        
        public List<StraightPathItem> straightPath = new();

        public DtNavMeshQuery query;
    }
}