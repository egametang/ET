using System.Linq;
using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 匹配组件
    /// </summary>
    public class MatcherComponent : Component
    {
        /// <summary>
        /// 匹配玩家列表
        /// </summary>
        private readonly Dictionary<long, Matcher> matchers = new Dictionary<long, Matcher>();

        /// <summary>
        /// 匹配玩家
        /// </summary>
        public int Count { get { return matchers.Count; } }

        /// <summary>
        /// 添加一个匹配玩家
        /// </summary>
        /// <param name="matcher"></param>
        public void Add(Matcher matcher)
        {
            this.matchers.Add(matcher.PlayerId, matcher);
        }

        /// <summary>
        /// 根据ID获取匹配玩家
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Matcher Get(long id)
        {
            Matcher matcher;
            this.matchers.TryGetValue(id, out matcher);
            return matcher;
        }

        /// <summary>
        /// 获取所有匹配玩家
        /// </summary>
        /// <returns></returns>
        public Matcher[] GetAll()
        {
            return this.matchers.Values.ToArray();
        }

        /// <summary>
        /// 移除一个匹配玩家
        /// </summary>
        /// <param name="id"></param>
        public void Remove(long id)
        {
            Matcher matcher = Get(id);
            this.matchers.Remove(id);
            matcher?.Dispose();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public override void Dispose()
        {
            if (this.Id == 0)
            {
                return;
            }

            base.Dispose();

            foreach (var matcher in this.matchers.Values)
            {
                matcher.Dispose();
            }
        }
    }
}
