using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace World
{
	public class BuffManager: ISupportInitialize
	{
		public HashSet<Buff> Buffs { get; private set; }

		[BsonIgnore]
		public Dictionary<ObjectId, Buff> BuffGuidDict { get; private set; }

		[BsonIgnore]
		public Dictionary<int, Buff> BuffTypeDict { get; private set; }

		public BuffManager()
		{
			this.Buffs = new HashSet<Buff>();
			this.BuffGuidDict = new Dictionary<ObjectId, Buff>();
			this.BuffTypeDict = new Dictionary<int, Buff>();
		}

		void ISupportInitialize.BeginInit()
		{
		}

		void ISupportInitialize.EndInit()
		{
			foreach (var buff in Buffs)
			{
				this.BuffGuidDict.Add(buff.Id, buff);
			}

			foreach (var buff in Buffs)
			{
				this.BuffTypeDict.Add(buff.Type, buff);
			}
		}

		public bool Add(Buff buff)
		{
			if (this.Buffs.Contains(buff))
			{
				return false;
			}

			if (this.BuffGuidDict.ContainsKey(buff.Id))
			{
				return false;
			}

			if (this.BuffTypeDict.ContainsKey(buff.Type))
			{
				return false;
			}

			this.Buffs.Add(buff);
			this.BuffGuidDict.Add(buff.Id, buff);
			this.BuffTypeDict.Add(buff.Type, buff);

			return true;
		}

		public Buff GetById(ObjectId id)
		{
			if (!this.BuffGuidDict.ContainsKey(id))
			{
				return null;
			}

			return this.BuffGuidDict[id];
		}

		public Buff GetByType(int type)
		{
			if (!this.BuffTypeDict.ContainsKey(type))
			{
				return null;
			}

			return this.BuffTypeDict[type];
		}

		private bool Remove(Buff buff)
		{
			if (buff == null)
			{
				return false;
			}

			this.Buffs.Remove(buff);
			this.BuffGuidDict.Remove(buff.Id);
			this.BuffTypeDict.Remove(buff.Type);

			return true;
		}

		public bool RemoveById(ObjectId id)
		{
			var buff = this.GetById(id);
			return this.Remove(buff);
		}

		public bool RemoveByType(int type)
		{
			var buff = this.GetByType(type);
			return this.Remove(buff);
		}
	}
}
