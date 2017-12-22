namespace Model
{
	[ObjectEvent]
	public class PlayerEvent : ObjectEvent<Player>, IAwake<long>
	{
		public void Awake(long account)
		{
			this.Get().Awake(account);
		}
	}

    /// <summary>
    /// 玩家实体
    /// </summary>
	public sealed class Player : Entity
	{
        /// <summary>
        /// 用户ID
        /// </summary>
		public long UserId { get; private set; }

		public long UnitId { get; set; }

        /// <summary>
        /// ActorID
        /// </summary>
        public long ActorId { get; set; }

        public void Awake(long userid)
		{
			this.UserId = userid;
		}
		
		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}