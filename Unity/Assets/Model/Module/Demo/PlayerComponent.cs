using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
	[ObjectSystem]
	public class PlayerComponentAwakeSystem : AwakeSystem<PlayerComponent>
	{
		public override void Awake(PlayerComponent self)
		{
			self.Awake();
		}
	}
	
	public class PlayerComponent : Component
	{
		public static PlayerComponent Instance { get; private set; }

		private Player myPlayer;

		public Player MyPlayer
		{
			get
			{
				return this.myPlayer;
			}
			set
			{
				this.myPlayer = value;
				this.myPlayer.Parent = this;
			}
		}
		
		private readonly Dictionary<long, Player> idPlayers = new Dictionary<long, Player>();

		public void Awake()
		{
			Instance = this;
		}
		
		public void Add(Player player)
		{
			this.idPlayers.Add(player.Id, player);
			player.Parent = this;
		}

		public Player Get(long id)
		{
			Player player;
			this.idPlayers.TryGetValue(id, out player);
			return player;
		}

		public void Remove(long id)
		{
			this.idPlayers.Remove(id);
		}

		public int Count
		{
			get
			{
				return this.idPlayers.Count;
			}
		}

		public Player[] GetAll()
		{
			return this.idPlayers.Values.ToArray();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (Player player in this.idPlayers.Values)
			{
				player.Dispose();
			}

			Instance = null;
		}
	}
}