using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    [FriendOf(typeof(PathfindingComponent))]
    public static class PathfindingComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<PathfindingComponent, string>
        {
            protected override void Awake(PathfindingComponent self, string name)
            {
                self.Name = name;
                self.NavMesh = NavmeshComponent.Instance.Get(name);

                if (self.NavMesh == 0)
                {
                    throw new Exception($"nav load fail: {name}");
                }
            }
        }

        [ObjectSystem]
        public class DestroySystem: DestroySystem<PathfindingComponent>
        {
            protected override void Destroy(PathfindingComponent self)
            {
                self.Name = string.Empty;
                self.NavMesh = 0;
            }
        }
        
        public static void Find(this PathfindingComponent self, float3 start, float3 target, List<float3> result)
        {
            if (self.NavMesh == 0)
            {
                Log.Debug("寻路| Find 失败 pathfinding ptr is zero");
                throw new Exception($"pathfinding ptr is zero: {self.DomainScene().Name}");
            }

            self.StartPos[0] = -start.x;
            self.StartPos[1] = start.y;
            self.StartPos[2] = start.z;

            self.EndPos[0] = -target.x;
            self.EndPos[1] = target.y;
            self.EndPos[2] = target.z;
            //Log.Debug($"start find path: {self.GetParent<Unit>().Id}");
            int n = Recast.RecastFind(self.NavMesh, PathfindingComponent.extents, self.StartPos, self.EndPos, self.Result);
            for (int i = 0; i < n; ++i)
            {
                int index = i * 3;
                result.Add(new float3(-self.Result[index], self.Result[index + 1], self.Result[index + 2]));
            }
            //Log.Debug($"finish find path: {self.GetParent<Unit>().Id} {result.ListToString()}");
        }

        public static void FindWithAdjust(this PathfindingComponent self, float3 start, float3 target, List<float3> result,float adjustRaduis)
        {
            self.Find(start, target, result);
            for (int i = 0; i < result.Count; i++)
            {
                float3 adjust = self.FindRandomPointWithRaduis(result[i], adjustRaduis);
                result[i] = adjust;
            }
        }
        
        public static float3 FindRandomPointWithRaduis(this PathfindingComponent self, float3 pos, float raduis)
        {
            if (self.NavMesh == 0)
            {
                throw new Exception($"pathfinding ptr is zero: {self.DomainScene().Name}");
            }

            if (raduis > PathfindingComponent.FindRandomNavPosMaxRadius * 0.001f)
            {
                throw new Exception($"pathfinding raduis is too large，cur: {raduis}, max: {PathfindingComponent.FindRandomNavPosMaxRadius}");
            }
            
            int degrees = RandomGenerator.RandomNumber(0, 360);
            float r = RandomGenerator.RandomNumber(0, (int) (raduis * 1000)) / 1000f;

            float x = r * math.cos(math.radians(degrees)); 
            float z = r * math.sin(math.radians(degrees));

            float3 findpos = new float3(pos.x + x, pos.y, pos.z + z);

            return self.RecastFindNearestPoint(findpos);
        }
        
        /// <summary>
        /// 以pos为中心各自在宽和高的左右 前后两个方向延伸
        /// </summary>
        /// <param name="self"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static float3 FindRandomPointWithRectangle(this PathfindingComponent self, float3 pos, int width, int height)
        {
            if (self.NavMesh == 0)
            {
                throw new Exception($"pathfinding ptr is zero: {self.DomainScene().Name}");
            }

            if (width > PathfindingComponent.FindRandomNavPosMaxRadius * 0.001f || height > PathfindingComponent.FindRandomNavPosMaxRadius * 0.001f)
            {
                throw new Exception($"pathfinding rectangle is too large，width: {width} height: {height}, max: {PathfindingComponent.FindRandomNavPosMaxRadius}");
            }
            
            float x = RandomGenerator.RandomNumber(-width, width);
            float z = RandomGenerator.RandomNumber(-height, height);

            float3 findpos = new float3(pos.x + x, pos.y, pos.z + z);

            return self.RecastFindNearestPoint(findpos);
        }
        
        public static float3 FindRandomPointWithRaduis(this PathfindingComponent self, float3 pos, float minRadius, float maxRadius)
        {
            if (self.NavMesh == 0)
            {
                throw new Exception($"pathfinding ptr is zero: {self.DomainScene().Name}");
            }

            if (maxRadius > PathfindingComponent.FindRandomNavPosMaxRadius * 0.001f)
            {
                throw new Exception($"pathfinding raduis is too large，cur: {maxRadius}, max: {PathfindingComponent.FindRandomNavPosMaxRadius}");
            }
            
            int degrees = RandomGenerator.RandomNumber(0, 360);
            float r = RandomGenerator.RandomNumber((int) (minRadius * 1000), (int) (maxRadius * 1000)) / 1000f;

            float x = r * math.cos(math.radians(degrees));
            float z = r * math.sin(math.radians(degrees));

            float3 findpos = new float3(pos.x + x, pos.y, pos.z + z);

            return self.RecastFindNearestPoint(findpos);
        }

        public static float3 RecastFindNearestPoint(this PathfindingComponent self, float3 pos)
        {
            if (self.NavMesh == 0)
            {
                throw new Exception($"pathfinding ptr is zero: {self.DomainScene().Name}");
            }

            self.StartPos[0] = -pos.x;
            self.StartPos[1] = pos.y;
            self.StartPos[2] = pos.z;

            int ret = Recast.RecastFindNearestPoint(self.NavMesh, PathfindingComponent.extents, self.StartPos, self.EndPos);
            if (ret == 0)
            {
                throw new Exception($"RecastFindNearestPoint fail, 可能是位置配置有问题: sceneName:{self.DomainScene().Name} {pos} {self.Name} {self.GetParent<Unit>().Id} {self.GetParent<Unit>().Config.Id} {self.EndPos.ArrayToString()}");
            }
            
            return new float3(-self.EndPos[0], self.EndPos[1], self.EndPos[2]);
        }
    }
}