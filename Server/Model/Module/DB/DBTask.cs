using System.Threading.Tasks;

namespace ETModel
{
	public abstract class DBTask : ComponentWithId
	{
		public abstract Task Run();
	}
}