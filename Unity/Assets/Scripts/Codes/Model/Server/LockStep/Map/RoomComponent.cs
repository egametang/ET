using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class RoomComponent: Entity, IAwake<Match2Map_GetRoom>
    {
        public int AlreadyJoinRoomCount;

        private long sceneInstanceId;
        
        public LSScene LsScene
        {
            get
            {
                return Root.Instance.Get(this.sceneInstanceId) as LSScene;
            }
            set
            {
                this.sceneInstanceId = value.InstanceId;
            }
        }
    }
}