using Model;

namespace Hotfix
{
	public abstract class Disposer: Object
	{
		protected Disposer(): base(IdGenerater.GenerateId())
		{
			ObjectEvents.Instance.Add(this);
		}

		protected Disposer(long id): base(id)
		{
			ObjectEvents.Instance.Add(this);
		}

		public override void Dispose()
		{
			this.Id = 0;
			ObjectEvents.Instance.Remove(this);
		}
	}
}