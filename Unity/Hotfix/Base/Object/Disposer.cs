using Model;

namespace Hotfix
{
	public abstract class Disposer : Object, IDisposable2
	{
		public bool IsFromPool { get; set; }
		
		public bool IsDisposed { get; set; }

		public virtual void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			this.IsDisposed = true;

			if (this.IsFromPool)
			{
				Hotfix.ObjectPool.Recycle(this);
			}
		}
	}
}