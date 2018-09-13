﻿using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
    [BsonIgnoreExtraElements]
	public abstract class Component : Object, IDisposable, IComponentSerialize
	{
		// 只有Game.EventSystem.Add方法中会设置该值，如果new出来的对象不想加入Game.EventSystem中，则需要自己在构造函数中设置
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

				this.InstanceId = IdGenerater.GenerateId();
				Game.EventSystem.Add(this);
#if UNITY_EDITOR
                ECSView.SetParent(this);
#endif
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
        private Component parent;
        [BsonIgnore]
        public Component Parent
        {
            get { return parent; }
            set
            {
                parent = value;
#if UNITY_EDITOR
                ECSView.SetParent(this, parent);
#endif
            }
        }

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
#if UNITY_EDITOR
            ECSView.CreateView(this);
#endif
		}

		public virtual void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			// 触发Destroy事件
			Game.EventSystem.Destroy(this);

			Game.EventSystem.Remove(this.InstanceId);
			
			this.InstanceId = 0;
#if UNITY_EDITOR
			if (this.IsFromPool)
			{
				Game.ObjectPool.Recycle(this);
                ECSView.ReturnPool(this);
			}
            else
            {
                ECSView.DestroyView(this);
            }
#else
            if (this.IsFromPool)
            {
                Game.ObjectPool.Recycle(this);
            }
#endif
        }

        public virtual void BeginSerialize()
		{
		}

		public virtual void EndDeSerialize()
		{
		}
	}
}