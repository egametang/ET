using System;
using Common.Base;
using Common.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class Buff: Entity<Buff>, IDisposable
	{
		[BsonElement]
		private int configId { get; set; }

		[BsonElement]
		private ObjectId ownerId;

		[BsonElement]
		private long expiration;

		[BsonIgnore]
		private ObjectId timerId;

		[BsonIgnore]
		public long Expiration
		{
			get
			{
				return this.expiration;
			}
			set
			{
				this.expiration = value;
			}
		}

		[BsonIgnore]
		public ObjectId TimerId
		{
			get
			{
				return this.timerId;
			}
			set
			{
				this.timerId = value;
			}
		}

		public Buff(int configId, ObjectId ownerId)
		{
			this.configId = configId;
			this.ownerId = ownerId;
			if (this.Config.Duration != 0)
			{
				this.Expiration = TimeHelper.Now() + this.Config.Duration;
			}

			if (this.Expiration != 0)
			{
				// 注册Timer回调
				Env env = new Env();
				env[EnvKey.OwnerId] = this.OwnerId;
				env[EnvKey.BuffId] = this.Id;
				this.TimerId = World.Instance.GetComponent<TimerComponent>()
						.Add(this.Expiration, EventType.BuffTimeoutAction, env);
			}
		}

		protected void Dispose(bool disposing)
		{
			if (this.Expiration == 0)
			{
				return;
			}

			// Buff在垃圾回收或者主动Dispose,都需要释放Timer回调.非托管资源
			World.Instance.GetComponent<TimerComponent>().Remove(this.TimerId);

			this.expiration = 0;
		}

		~Buff()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		public override void EndInit()
		{
			base.EndInit();

			if (this.Expiration != 0)
			{
				// 注册Timer回调
				Env env = new Env();
				env[EnvKey.OwnerId] = this.OwnerId;
				env[EnvKey.BuffId] = this.Id;
				this.TimerId = World.Instance.GetComponent<TimerComponent>()
						.Add(this.Expiration, EventType.BuffTimeoutAction, env);
			}
		}

		[BsonIgnore]
		public BuffConfig Config
		{
			get
			{
				return World.Instance.GetComponent<ConfigComponent>().Get<BuffConfig>(this.configId);
			}
		}

		[BsonIgnore]
		public ObjectId OwnerId
		{
			get
			{
				return this.ownerId;
			}

			set
			{
				this.ownerId = value;
			}
		}
	}
}