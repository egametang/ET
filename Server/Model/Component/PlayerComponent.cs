﻿using System.Collections.Generic;
using System.Linq;

namespace Model
{
	public class PlayerComponent : Component
	{
		private readonly Dictionary<long, Player> idPlayers = new Dictionary<long, Player>();

		public void Add(Player player)
		{
			this.idPlayers.Add(player.Id, player);
		}

		public Player Get(long id)
		{
			this.idPlayers.TryGetValue(id, out Player gamer);
			return gamer;
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
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();

			foreach (Player player in this.idPlayers.Values)
			{
				player.Dispose();
			}
		}
	}
}