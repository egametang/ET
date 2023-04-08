using System;

namespace ET.Server
{
    [FriendOf(typeof(RoomComponent))]
    public static class RoomComponentSystem
    {
        [ObjectSystem]
        public class RoomComponentAwakeSystem: AwakeSystem<RoomComponent, Match2Map_GetRoom>
        {
            protected override void Awake(RoomComponent self, Match2Map_GetRoom match2MapGetRoom)
            {
                foreach (long id in match2MapGetRoom.PlayerIds)
                {
                    RoomPlayer roomPlayer = self.AddChildWithId<RoomPlayer>(id);
                }
                
                // 创建LockStep场景
                LSScene lsScene = new LSScene(IdGenerater.Instance.GenerateInstanceId(), self.DomainZone(), SceneType.LockStep, "LockStep", self);
                self.LsScene = lsScene;
            }
        }
    }
}