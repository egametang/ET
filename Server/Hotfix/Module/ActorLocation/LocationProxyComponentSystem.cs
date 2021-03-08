﻿using System;

namespace ET
{
    [ObjectSystem]
    public class LocationProxyComponentSystem: AwakeSystem<LocationProxyComponent>
    {
        public override void Awake(LocationProxyComponent self)
        {
            self.Awake();
        }
    }

    public static class LocationProxyComponentEx
    {
        public static void Awake(this LocationProxyComponent self)
        {
            LocationProxyComponent.Instance = self;
        }
        
        private static long GetLocationSceneId(long key)
        {
            return StartSceneConfigCategory.Instance.LocationConfig.SceneId;
        }

        public static async ETTask Add(this LocationProxyComponent self, long key, long instanceId)
        {
            Log.Info($"location proxy add {key}, {instanceId} {TimeHelper.ServerNow()}");
            await MessageHelper.CallActor(GetLocationSceneId(key),
                new ObjectAddRequest() { Key = key, InstanceId = instanceId });
        }

        public static async ETTask Lock(this LocationProxyComponent self, long key, long instanceId, int time = 1000)
        {
            Log.Info($"location proxy lock {key}, {instanceId} {TimeHelper.ServerNow()}");
            await MessageHelper.CallActor(GetLocationSceneId(key),
                new ObjectLockRequest() { Key = key, InstanceId = instanceId, Time = time });
        }

        public static async ETTask UnLock(this LocationProxyComponent self, long key, long oldInstanceId, long instanceId)
        {
            Log.Info($"location proxy unlock {key}, {instanceId} {TimeHelper.ServerNow()}");
            await MessageHelper.CallActor(GetLocationSceneId(key),
                new ObjectUnLockRequest() { Key = key, OldInstanceId = oldInstanceId, InstanceId = instanceId });
        }

        public static async ETTask Remove(this LocationProxyComponent self, long key)
        {
            Log.Info($"location proxy add {key}, {TimeHelper.ServerNow()}");
            await MessageHelper.CallActor(GetLocationSceneId(key),
                new ObjectRemoveRequest() { Key = key });
        }

        public static async ETTask<long> Get(this LocationProxyComponent self, long key)
        {
            if (key == 0)
            {
                throw new Exception($"get location key 0");
            }

            // location server配置到共享区，一个大战区可以配置N多个location server,这里暂时为1
            ObjectGetResponse response =
                    (ObjectGetResponse) await MessageHelper.CallActor(GetLocationSceneId(key),
                        new ObjectGetRequest() { Key = key });
            return response.InstanceId;
        }

        public static async ETTask AddLocation(this Entity self)
        {
            await LocationProxyComponent.Instance.Add(self.Id, self.InstanceId);
        }

        public static async ETTask RemoveLocation(this Entity self)
        {
            await LocationProxyComponent.Instance.Remove(self.Id);
        }
    }
}