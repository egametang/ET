namespace ETHotfix
{
    public static class GamePlayerFactory
    {
        public static GamePlayer Create(long userid, int level, int characterid, string playername, string title,int dong, int nan)
        {
            GamePlayer player = ComponentFactory.Create<GamePlayer>();
            player.UserID = userid;
            player.Level = level;
            player.CharacterID = characterid;
            player.Dong = dong;
            player.Nan = nan;
            player.PlayerName = playername;
            player.Title = title;
            GamePlayerComponent.Instance.Add(player);
            return player;
        }
    }
}