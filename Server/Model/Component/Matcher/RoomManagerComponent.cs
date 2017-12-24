using System.Linq;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 房间管理组件
    /// </summary>
    public class RoomManagerComponent : Component
    {
        /// <summary>
        /// 房间列表
        /// </summary>
        private readonly Dictionary<long, Room> _rooms = new Dictionary<long, Room>();

        /// <summary>
        /// 游戏中的房间列表
        /// </summary>
        private readonly Dictionary<long, Room> _gameRooms = new Dictionary<long, Room>();

        /// <summary>
        /// 准备中的房间列表
        /// </summary>
        private readonly Dictionary<long, Room> _readyRooms = new Dictionary<long, Room>();

        /// <summary>
        /// 房间轮询
        /// </summary>
        private readonly EQueue<Room> _idleRooms = new EQueue<Room>();

        /// <summary>
        /// 总房间数
        /// </summary>
        public int TotalCount { get { return this._rooms.Count; } }

        /// <summary>
        /// 游戏中的房间数
        /// </summary>
        public int GameRoomCount { get { return _gameRooms.Count; } }

        /// <summary>
        /// 准备中的房间数
        /// </summary>
        public int ReadyRoomCount { get { return _readyRooms.Where(p => p.Value.Count < 3).Count(); } }

        /// <summary>
        /// 闲置的房间数
        /// </summary>
        public int IdleRoomCount { get { return _idleRooms.Count; } }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this.Id == 0)
            {
                return;
            }

            base.Dispose();

            foreach (var room in this._rooms.Values)
            {
                room.Dispose();
            }
        }
    }
}
