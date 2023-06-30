using System;

namespace ET.Server
{
    public static partial class LocationProxyComponentSystem
    {
        private static ActorId GetLocationSceneId(long key)
        {
            return StartSceneConfigCategory.Instance.LocationConfig.ActorId;
        }

        public static async ETTask Add(this LocationProxyComponent self, int type, long key, ActorId actorId)
        {
            Log.Info($"location proxy add {key}, {actorId} {self.Fiber().TimeInfo.ServerNow()}");
            await self.Root().GetComponent<ActorSenderComponent>().Call(GetLocationSceneId(key),
                new ObjectAddRequest() { Type = type, Key = key, ActorId = actorId });
        }

        public static async ETTask Lock(this LocationProxyComponent self, int type, long key, ActorId actorId, int time = 60000)
        {
            Log.Info($"location proxy lock {key}, {actorId} {self.Fiber().TimeInfo.ServerNow()}");
            await self.Root().GetComponent<ActorSenderComponent>().Call(GetLocationSceneId(key),
                new ObjectLockRequest() { Type = type, Key = key, ActorId = actorId, Time = time });
        }

        public static async ETTask UnLock(this LocationProxyComponent self, int type, long key, ActorId oldActorId, ActorId newActorId)
        {
            Log.Info($"location proxy unlock {key}, {newActorId} {self.Fiber().TimeInfo.ServerNow()}");
            await self.Root().GetComponent<ActorSenderComponent>().Call(GetLocationSceneId(key),
                new ObjectUnLockRequest() { Type = type, Key = key, OldActorId = oldActorId, NewActorId = newActorId });
        }

        public static async ETTask Remove(this LocationProxyComponent self, int type, long key)
        {
            Log.Info($"location proxy add {key}, {self.Fiber().TimeInfo.ServerNow()}");
            await self.Root().GetComponent<ActorSenderComponent>().Call(GetLocationSceneId(key),
                new ObjectRemoveRequest() { Type = type, Key = key });
        }

        public static async ETTask<ActorId> Get(this LocationProxyComponent self, int type, long key)
        {
            if (key == 0)
            {
                throw new Exception($"get location key 0");
            }

            // location server配置到共享区，一个大战区可以配置N多个location server,这里暂时为1
            ObjectGetResponse response =
                    (ObjectGetResponse) await self.Root().GetComponent<ActorSenderComponent>().Call(GetLocationSceneId(key),
                        new ObjectGetRequest() { Type = type, Key = key });
            return response.ActorId;
        }

        public static async ETTask AddLocation(this Entity self, int type)
        {
            await self.Root().GetComponent<LocationProxyComponent>().Add(type, self.Id, self.GetActorId());
        }

        public static async ETTask RemoveLocation(this Entity self, int type)
        {
            await self.Root().GetComponent<LocationProxyComponent>().Remove(type, self.Id);
        }
    }
}