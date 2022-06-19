using System.Linq;

namespace ET.Server
{
    [FriendOf(typeof(PlayerComponent))]
    public static class PlayerComponentSystem
    {
        public static void Add(this PlayerComponent self, Player player)
        {
            self.idPlayers.Add(player.Id, player);
        }

        public static Player Get(this PlayerComponent self, long id)
        {
            self.idPlayers.TryGetValue(id, out Player gamer);
            return gamer;
        }

        public static void Remove(this PlayerComponent self, long id)
        {
            self.idPlayers.Remove(id);
        }

        public static Player[] GetAll(this PlayerComponent self)
        {
            return self.idPlayers.Values.ToArray();
        }
    }
}