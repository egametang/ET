namespace ET.Server
{
    [FriendOf(typeof(Player))]
    public static partial class PlayerSystem
    {
        [EntitySystem]
        private static void Awake(this Player self, string a)
        {
            self.Account = a;
        }
    }
}