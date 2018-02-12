using System.Threading.Tasks;

namespace Model
{
	public abstract class DBTask : Component
	{
		protected DBTask()
		{
		}

		protected DBTask(long id)
		{
			this.Id = id;
		}
		
		public abstract Task Run();
	}
}