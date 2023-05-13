namespace ET.Server
{
    [FriendOf(typeof(Player))]
    public static class PlayerSystem
    {
        [EntitySystem]
        public class PlayerAwakeSystem : AwakeSystem<Player, string>
        {
            protected override void Awake(Player self, string a)
            {
                self.Account = a;
            }
        }
    }
}