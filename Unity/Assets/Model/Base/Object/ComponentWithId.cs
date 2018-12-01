using MongoDB.Bson.Serialization.Attributes;
#if !SERVER
using UnityEngine;
#endif

namespace ETModel
{
	[BsonIgnoreExtraElements]
	public abstract class ComponentWithId : Component
	{
		[BsonIgnoreIfDefault]
		[BsonDefaultValue(0L)]
		[BsonElement]
		[BsonId]
		public long Id { get; set; }

		protected ComponentWithId()
		{
			this.Id = this.InstanceId;
		}

		protected ComponentWithId(long id)
		{
			this.Id = id;
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