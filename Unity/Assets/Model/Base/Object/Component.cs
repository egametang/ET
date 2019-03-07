using System;
using ETModel;
using MongoDB.Bson.Serialization.Attributes;
#if !SERVER
using UnityEngine;
#endif

namespace ETModel
{
	[BsonIgnoreExtraElements]
	public abstract class Component : Object, IDisposable
	{
		[BsonIgnore]
		public long InstanceId { get; private set; }
		
#if !SERVER
		public static GameObject Global { get; } = GameObject.Find("/Global");
		
		[BsonIgnore]
		public GameObject GameObject { get; protected set; }
#endif

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
					this.InstanceId = IdGenerater.GenerateInstanceId();
				}
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

		private Component parent;
		
		[BsonIgnore]
		public Component Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				this.parent = value;

#if !SERVER
				if (this.parent == null)
				{
					this.GameObject.transform.SetParent(Global.transform, false);
					return;
				}

				if (this.GameObject != null && this.parent.GameObject != null)
				{
					this.GameObject.transform.SetParent(this.parent.GameObject.transform, false);
				}
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
			this.InstanceId = IdGenerater.GenerateInstanceId();
#if !SERVER
			if (!this.GetType().IsDefined(typeof(HideInHierarchy), true))
			{
				this.GameObject = new GameObject();
				this.GameObject.name = this.GetType().Name;
				this.GameObject.layer = LayerNames.GetLayerInt(LayerNames.HIDDEN);
				this.GameObject.transform.SetParent(Global.transform, false);
				this.GameObject.AddComponent<ComponentView>().Component = this;
			}
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

			if (this.IsFromPool)
			{
				Game.ObjectPool.Recycle(this);
			}
			else
			{
#if !SERVER
				if (this.GameObject != null)
				{
					UnityEngine.Object.Destroy(this.GameObject);
				}
#endif
			}
		}

		public override void EndInit()
		{
			Game.EventSystem.Deserialize(this);
		}
		
		public override string ToString()
		{
			return MongoHelper.ToJson(this);
		}
	}
}