using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
	public abstract class AConfig: ISupportInitialize
	{
		[BsonId]
		public long Id { get; set; }

		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
		}
	}
}