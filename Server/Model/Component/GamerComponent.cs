using System.Collections.Generic;
using System.Linq;

namespace Model
{
	public class GamerComponent : Component
	{
		private readonly Dictionary<long, Gamer> idGamers = new Dictionary<long, Gamer>();

		public void Add(Gamer gamer)
		{
			if (this.idGamers.Count > 1)
			{
				gamer.Team = 1;
			}
			this.idGamers.Add(gamer.Id, gamer);
		}

		public Gamer Get(long id)
		{
			this.idGamers.TryGetValue(id, out Gamer gamer);
			return gamer;
		}

		public void Remove(long id)
		{
			this.idGamers.Remove(id);
		}

		public int Count
		{
			get
			{
				return this.idGamers.Count;
			}
		}

		public Gamer[] GetAll()
		{
			return this.idGamers.Values.ToArray();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}
			base.Dispose();

			foreach (Gamer gamer in this.idGamers.Values)
			{
				gamer.Dispose();
			}
		}
	}
}