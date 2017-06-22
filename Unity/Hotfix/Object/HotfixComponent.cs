using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	[BsonKnownTypes(typeof (AConfigComponent))]
	public abstract class HotfixComponent: Disposer
	{
		[BsonIgnore]
		public HotfixEntity Owner { get; set; }

		public T GetOwner<T>() where T : HotfixEntity
		{
			return this.Owner as T;
		}

		protected HotfixComponent()
		{
		}

		protected HotfixComponent(long id): base(id)
		{
		}

		public T GetComponent<T>() where T : HotfixComponent
		{
			return this.Owner.GetComponent<T>();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			this.Owner.RemoveComponent(this.GetType());
		}
	}
}