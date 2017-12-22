using Model;

namespace Hotfix
{
    public static class RoomFactory
    {
        public static Room Create(RoomLevel level)
        {
            Room room = EntityFactory.Create<Room>();
            room.AddComponent<RoomJoinKeyComponent>();
            room.AddComponent<DeckComponent>();
            room.AddComponent<DeskCardsCacheComponent>();
            room.AddComponent<OrderControllerComponent>();

            RoomConfig config = RoomHelper.GetConfig(level);
            GameControllerComponent gameController = room.AddComponent<GameControllerComponent, RoomConfig>(config);

            //添加管理,避免被GC释放
            Game.Scene.GetComponent<RoomComponent>().Add(room);
            return room;
        }
    }
}
