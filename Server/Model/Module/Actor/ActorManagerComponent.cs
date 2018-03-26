using System.Collections.Generic;

namespace ETModel
{
	/// <summary>
	/// 用来管理该服务器上所有的Actor对象
	/// </summary>
	public class ActorManagerComponent : Component
	{
		private readonly Dictionary<long, Entity> dictionary = new Dictionary<long, Entity>();
		
		public void Add(Entity entity)
		{
			Log.Info($"add actor: {entity.Id} {entity.GetType().Name}");
			dictionary[entity.Id] = entity;
		}

		public void Remove(long id)
		{
			Entity entity;
			if (!this.dictionary.TryGetValue(id, out entity))
			{
				return;
			}
			Log.Info($"remove actor: {entity.Id} {entity.GetType().Name}");
			this.dictionary.Remove(id);
		}

		public Entity Get(long id)
		{
			Entity entity = null;
			this.dictionary.TryGetValue(id, out entity);
			return entity;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}