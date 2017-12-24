namespace Model
{
    [ObjectEvent]
    public class GamerEvent : ObjectEvent<Gamer>, IAwake<long>
    {
        public void Awake(long id)
        {
            this.Get().Awake(id);
        }
    }

    /// <summary>
    /// 游戏玩家类
    /// </summary>
    public sealed class Gamer : Entity
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public long UserId { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// 是否掉线
        /// </summary>
        public bool isOffline { get; set; }

        public void Awake(long id)
        {
            this.UserId = id;
        }
    }
}
