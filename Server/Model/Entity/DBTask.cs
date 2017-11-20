using System.Threading.Tasks;

namespace Model
{
	public abstract class DBTask : Disposer
	{
		protected DBTask()
		{
		}

		protected DBTask(long id): base(id)
		{
		}
		
		public abstract Task Run();
	}
}