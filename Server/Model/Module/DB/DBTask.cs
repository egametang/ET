using System.Threading.Tasks;

namespace ETModel
{
	public abstract class DBTask : Component
	{
		public abstract Task Run();
	}
}