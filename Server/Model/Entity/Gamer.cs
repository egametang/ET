using System.Collections.Generic;
using System.Linq;

namespace Model
{
	public sealed class Gamer : Entity
	{
		public string Account { get; }

		public int Team { get; set; }

		private readonly Dictionary<long, Unit> dictionary = new Dictionary<long, Unit>();

		public Gamer(string account)
		{
			this.Account = account;
		}

		public Unit[] GetAll()
		{
			return this.dictionary.Values.ToArray();
		}

		public void Add(Unit unit)
		{
			this.dictionary.Add(unit.Id, unit);
		}

		public void Remove(long id)
		{
			this.dictionary.Remove(id);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}