using System;
using System.Collections.Generic;
using Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
#pragma warning disable 4014

namespace Model
{
	public class BuffComponent: Component<Unit>
	{
		[BsonElement]
		private HashSet<Buff> buffs;

		[BsonIgnore]
		private Dictionary<ObjectId, Buff> idBuff;

		[BsonIgnore]
		private MultiMap<BuffType, Buff> typeBuff;

		public BuffComponent()
		{
			this.buffs = new HashSet<Buff>();
			this.idBuff = new Dictionary<ObjectId, Buff>();
			this.typeBuff = new MultiMap<BuffType, Buff>();
		}

		public override void BeginInit()
		{
			base.BeginInit();

			this.buffs = new HashSet<Buff>();
			this.idBuff = new Dictionary<ObjectId, Buff>();
			this.typeBuff = new MultiMap<BuffType, Buff>();
		}

		public override void EndInit()
		{
			base.EndInit();

			foreach (Buff buff in this.buffs)
			{
				this.idBuff.Add(buff.Id, buff);
				this.typeBuff.Add(buff.Config.Type, buff);
			}
		}

		public void Add(Buff buff)
		{
			if (this.buffs.Contains(buff))
			{
				throw new ArgumentException($"already exist same buff, Id: {buff.Id} ConfigId: {buff.Config.Id}");
			}

			if (this.idBuff.ContainsKey(buff.Id))
			{
				throw new ArgumentException($"already exist same buff, Id: {buff.Id} ConfigId: {buff.Config.Id}");
			}

			Env env = new Env();
			env[EnvKey.Owner] = this.Owner;
			env[EnvKey.Buff] = buff;

			World.Instance.GetComponent<EventComponent<EventAttribute>>().RunAsync(EventType.BeforeAddBuff, env);

			this.buffs.Add(buff);
			this.idBuff.Add(buff.Id, buff);
			this.typeBuff.Add(buff.Config.Type, buff);

			World.Instance.GetComponent<EventComponent<EventAttribute>>().RunAsync(EventType.AfterAddBuff, env);
		}

		public Buff GetById(ObjectId id)
		{
			if (!this.idBuff.ContainsKey(id))
			{
				return null;
			}

			return this.idBuff[id];
		}

		public Buff GetOneByType(BuffType type)
		{
			return this.typeBuff.GetOne(type);
		}

		public Buff[] GetByType(BuffType type)
		{
			return this.typeBuff.GetAll(type);
		}

		private void Remove(Buff buff)
		{
			if (buff == null)
			{
				return;
			}

			Env env = new Env();
			env[EnvKey.Owner] = this.Owner;
			env[EnvKey.Buff] = buff;

			World.Instance.GetComponent<EventComponent<EventAttribute>>()
					.RunAsync(EventType.BeforeRemoveBuff, env);

			this.buffs.Remove(buff);
			this.idBuff.Remove(buff.Id);
			this.typeBuff.Remove(buff.Config.Type, buff);

			World.Instance.GetComponent<EventComponent<EventAttribute>>().RunAsync(EventType.AfterRemoveBuff, env);
			buff.Dispose();
		}

		public void RemoveById(ObjectId id)
		{
			Buff buff = this.GetById(id);
			this.Remove(buff);
		}

		public void RemoveByType(BuffType type)
		{
			Buff[] allbuffs = this.GetByType(type);
			foreach (Buff buff in allbuffs)
			{
				this.Remove(buff);
			}
		}
	}
}