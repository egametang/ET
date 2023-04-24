using TrueSync;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class RoomClientUpdater: Entity, IAwake, IUpdate
    {
        public LSInputInfo InputInfo = new LSInputInfo();
    }
}