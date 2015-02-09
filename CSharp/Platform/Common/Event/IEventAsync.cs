using System.Threading.Tasks;

namespace Common.Event
{
	public interface IEventAsync
	{
		Task RunAsync(Env env);
	}
}