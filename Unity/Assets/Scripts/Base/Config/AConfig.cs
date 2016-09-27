using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	public abstract class AConfig: ISupportInitialize
	{
		[BsonId]
		public long id { get; set; }

		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
		}
	}
}