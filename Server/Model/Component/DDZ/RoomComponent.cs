using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    /// <summary>
    /// 房间管理组件
    /// </summary>
    public class RoomComponent : Component
    {
        /// <summary>
        /// 房间列表
        /// </summary>
        private readonly Dictionary<long, Room> rooms = new Dictionary<long, Room>();

        /// <summary>
        /// 添加一个房间
        /// </summary>
        /// <param name="room"></param>
        public void Add(Room room)
        {
            this.rooms.Add(room.Id, room);
        }

        /// <summary>
        /// 根据ID获取房间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Room Get(long id)
        {
            this.rooms.TryGetValue(id, out var room);
            return room;
        }

        /// <summary>
        /// 根据ID移除房间
        /// </summary>
        /// <param name="id"></param>
        public void Remove(long id)
        {
            Room room = Get(id);
            this.rooms.Remove(id);
            room?.Dispose();
        }

        /// <summary>
        /// 释放所有房间
        /// </summary>
        public override void Dispose()
        {
            if (this.Id == 0)
            {
                return;
            }

            base.Dispose();

            foreach (var room in this.rooms.Values)
            {
                room.Dispose();
            }
        }
    }
}
