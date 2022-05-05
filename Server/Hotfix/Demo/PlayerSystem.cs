namespace ET
{
    [FriendClass(typeof(Player))]
    public static class PlayerSystem
    {
        [ObjectSystem]
        public class PlayerAwakeSystem : AwakeSystem<Player, string>
        {
            public override void Awake(Player self, string a)
            {
                self.Account = a;
            }
        }
    }
}