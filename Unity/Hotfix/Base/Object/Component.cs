using ETModel;
using MongoDB.Bson.Serialization.Attributes;

namespace ETHotfix
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
                if (Define.IsEditorMode)
                    ECSView.SetParent(this);
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
                if (Define.IsEditorMode)
                    ECSView.SetParent(this, parent);
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
            if (Define.IsEditorMode)
                ECSView.CreateView(this);
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

            if (this.IsFromPool)
                Game.ObjectPool.Recycle(this);

            if (Define.IsEditorMode)
            {
                if (this.IsFromPool)
                    ECSView.ReturnPool(this);
                else
                    ECSView.DestroyView(this);
            }

        }

        public virtual void BeginSerialize()
		{
		}

		public virtual void EndDeSerialize()
		{
		}
	}
}