namespace Model
{
    [ObjectEvent]
    public class MatcherEvent : ObjectEvent<Matcher>, IAwake<long>
    {
        public void Awake(long id)
        {
            this.Get().Awake(id);
        }
    }

    /// <summary>
    /// 匹配器
    /// </summary>
    public sealed class Matcher : Entity
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public long PlayerId { get; private set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 网关SessionID
        /// </summary>
        public long GateSessionId { get; set; }

        /// <summary>
        /// 网关AppID
        /// </summary>
        public int GateAppId { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="id"></param>
        public void Awake(long id)
        {
            this.PlayerId = id;
        }
    }
}
