using ETModel;

namespace ETHotfix
{
    //玩家当前行为状态
    public enum IdleState
    {
        Normal,
        Battle,
        Pk,
        //TODO 伐木挖矿钓鱼
    }

    [ObjectSystem]
    public class GamePlayerAwakeSystem : AwakeSystem<GamePlayer>
    {
        public override void Awake(GamePlayer self)
        {
            self.Awake();
        }
    }

    /// <summary>
    /// 玩家外表数据信息
    /// </summary>
    public class GamePlayer : Entity
    {
        public long UserID { get; set; }
        public int Level { get; set; }
        public int CharacterID { get; set; }
        public int Dong { get; set; }
        public int Nan { get; set; }
        public string PlayerName { get; set; }
        public string Title { get; set; }
        public bool IsTeamLeader { get; set; }
        public bool IsVip { get; set; }
        public IdleState IdleState { get; set; }

        public void Awake()
        {
        }
        
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            UserID = 0;
            Level = 0;
            CharacterID = 0;
            Dong = 0;
            Nan = 0;
            PlayerName = null;
            Title = null;
            IsTeamLeader = false;
            IsVip = false;
            IdleState = IdleState.Normal;
        }
    }
}
