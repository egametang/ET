using System.Threading.Tasks;

namespace Model
{
	public abstract class DBTask : Component
	{
		public abstract Task Run();
	}
}