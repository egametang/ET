using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	[BsonIgnoreExtraElements]
	public abstract class Component : Object, IDisposable, IComponentSerialize
	{
		[BsonIgnore]
		public long InstanceId { get; protected set; }

		[BsonIgnore]
		private bool isFromPool;

		[BsonIgnore]
		public bool IsFromPool
		{
			get
			{
				return this.isFromPool;
			}
			set
			{
				this.isFromPool = value;

				if (!this.isFromPool)
				{
					return;
				}

				if (this.InstanceId == 0)
				{
					this.InstanceId = IdGenerater.GenerateId();
				}

				Game.EventSystem.Add(this);
			}
		}

		[BsonIgnore]
		public bool IsDisposed
		{
			get
			{
				return this.InstanceId == 0;
			}
		}
		
		[BsonIgnore]
		public Component Parent { get; set; }

		public T GetParent<T>() where T : Component
		{
			return this.Parent as T;
		}

		[BsonIgnore]
		public Entity Entity
		{
			get
			{
				return this.Parent as Entity;
			}
		}
		
		protected Component()
		{
			this.InstanceId = IdGenerater.GenerateId();
		}

		public virtual void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			Game.EventSystem.Remove(this.InstanceId);

			this.InstanceId = 0;

			if (this.IsFromPool)
			{
				Game.ObjectPool.Recycle(this);
			}

			// 触发Destroy事件
			Game.EventSystem.Destroy(this);
		}

		public virtual void BeginSerialize()
		{
		}

		public virtual void EndDeSerialize()
		{
		}
	}
}