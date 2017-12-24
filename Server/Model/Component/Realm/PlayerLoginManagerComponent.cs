using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    /// <summary>
    /// 玩家登录组件
    /// </summary>
    public class PlayerLoginManagerComponent : Component
    {
        private  readonly Dictionary<long, int> _dictionary = new Dictionary<long, int>();

        public void Add(long userId, int gateAppId)
        {
            _dictionary.Add(userId, gateAppId);
        }

        public int Get(long userId)
        {
            _dictionary.TryGetValue(userId, out var gateAppId);
            return gateAppId;
        }

        public void Remove(long userId)
        {
            _dictionary.Remove(userId);
        }
    }
}