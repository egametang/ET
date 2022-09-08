namespace ET.Server
{
    [FriendOf(typeof(Player))]
    public static class PlayerSystem
    {
        [ObjectSystem]
        public class PlayerAwakeSystem : AwakeSystem<Player, string>
        {
            protected override void Awake(Player self, string a)
            {
                self.Account = a;
            }
        }
    }
}