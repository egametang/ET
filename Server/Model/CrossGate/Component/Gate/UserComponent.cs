using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
    /// <summary>
    /// User对象管理组件
    /// </summary>
    public class UserComponent : Component
    {
        private readonly Dictionary<long, User> idUsers = new Dictionary<long, User>();

        /// <summary>
        /// 添加User对象
        /// </summary>
        /// <param name="user"></param>
        public void Add(User user)
        {
            this.idUsers.Add(user.UserID, user);
        }

        /// <summary>
        /// 获取User对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User Get(long id)
        {
            this.idUsers.TryGetValue(id, out User gamer);
            return gamer;
        }

        /// <summary>
        /// 移除User对象
        /// </summary>
        /// <param name="id"></param>
        public void Remove(long id)
        {
            this.idUsers.Remove(id);
        }

        /// <summary>
        /// User对象总数量
        /// </summary>
        public int Count
        {
            get
            {
                return this.idUsers.Count;
            }
        }

        /// <summary>
        /// 获取所有User对象
        /// </summary>
        /// <returns></returns>
        public User[] GetAll()
        {
            return this.idUsers.Values.ToArray();
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            foreach (User user in this.idUsers.Values)
            {
                user.Dispose();
            }
        }
    }
}