using System.Collections.Generic;
using System.Linq;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class GamePlayerComponentAwakeSystem : AwakeSystem<GamePlayerComponent>
    {
        public override void Awake(GamePlayerComponent self)
        {
            self.Awake();
        }
    }

    public class GamePlayerComponent : Component
    {
        public static GamePlayerComponent Instance { get; private set; }

        public GamePlayer MyPlayer;

        private readonly Dictionary<long, GamePlayer> idGamePlayers = new Dictionary<long, GamePlayer>();

        public void Awake()
        {
            Instance = this;
        }

        public void Add(GamePlayer GamePlayer)
        {
            this.idGamePlayers.Add(GamePlayer.Id, GamePlayer);
        }

        public GamePlayer Get(long id)
        {
            GamePlayer GamePlayer;
            this.idGamePlayers.TryGetValue(id, out GamePlayer);
            return GamePlayer;
        }

        public void Remove(long id)
        {
            this.idGamePlayers.Remove(id);
        }

        public int Count
        {
            get
            {
                return this.idGamePlayers.Count;
            }
        }

        public GamePlayer[] GetAll()
        {
            return this.idGamePlayers.Values.ToArray();
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            foreach (GamePlayer GamePlayer in this.idGamePlayers.Values)
            {
                GamePlayer.Dispose();
            }
            
            Instance = null;
        }
    }
}