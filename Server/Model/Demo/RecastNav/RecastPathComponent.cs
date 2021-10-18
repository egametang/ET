using System.Collections.Generic;
using ET;
using UnityEngine;

namespace ET
{
    public class RecastPathAwakeSystem: AwakeSystem<RecastPathComponent>
    {
        public override void Awake(RecastPathComponent self)
        {
            self.Awake();
        }
    }

    public class RecastPathComponent: Entity
    {
        /// <summary>
        /// 5v5地图的Nav数据路径
        /// </summary>
        public const string Moba5V5MapNavDataPath = "../Config/RecastNavData/solo_navmesh.bin";

        /// <summary>
        /// 寻路处理者（可用于拓展多线程，参考A*插件）
        /// key为地图id，value为具体处理者
        /// </summary>
        public Dictionary<int, RecastPathProcessor> m_RecastPathProcessorDic = new Dictionary<int, RecastPathProcessor>();

        /// <summary>
        /// 初始化寻路引擎
        /// </summary>
        public void Awake()
        {
            RecastInterface.Init();
            //TODO 先直接在这里强行初始化地图
            LoadMapNavData(10001, Moba5V5MapNavDataPath.ToCharArray());
            // // 读取体素数据
            // VoxelFile = new VoxelFile();
        }

        /// <summary>
        /// 寻路
        /// </summary>
        public void SearchPath(int mapId, Vector3 from, Vector3 to, List<Vector3> result)
        {
            GetRecastPathProcessor(mapId).CalculatePath(from, to, result);
        }

        public RecastPathProcessor GetRecastPathProcessor(int mapId)
        {
            if (this.m_RecastPathProcessorDic.TryGetValue(mapId, out var recastPathProcessor))
            {
                return recastPathProcessor;
            }
            else
            {
                Log.Error($"未找到地图id为{mapId}的recastPathProcessor");
                return null;
            }
        }

        /// <summary>
        /// 加载一个Map的数据
        /// </summary>
        public void LoadMapNavData(int mapId, char[] navDataPath)
        {
            if (m_RecastPathProcessorDic.ContainsKey(mapId))
            {
                Log.Warning($"已存在Id为{mapId}的地图Nav数据，请勿重复加载！");
                return;
            }

            if (RecastInterface.LoadMap(mapId, navDataPath))
            {
                RecastPathProcessor recastPathProcessor = this.AddChild<RecastPathProcessor>();
                recastPathProcessor.MapId = mapId;
                m_RecastPathProcessorDic[mapId] = recastPathProcessor;
                Log.Debug($"加载Id为{mapId}的地图Nav数据成功！");
            }
        }

        /// <summary>
        /// 卸载地图数据
        /// </summary>
        /// <param name="mapId">地图Id</param>
        public void UnLoadMapNavData(int mapId)
        {
            if (!m_RecastPathProcessorDic.ContainsKey(mapId))
            {
                Log.Warning($"不存在Id为{mapId}的地图Nav数据，无法进行卸载！");
                return;
            }

            m_RecastPathProcessorDic[mapId].Dispose();
            m_RecastPathProcessorDic.Remove(mapId);
            if (RecastInterface.FreeMap(mapId))
            {
                Log.Debug($"地图： {mapId}  释放成功");
            }
            else
            {
                Log.Debug($"地图： {mapId}  释放失败");
            }
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();
            RecastInterface.Fini();
        }
    }
}