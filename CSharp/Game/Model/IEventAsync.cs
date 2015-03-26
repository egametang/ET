using System.Threading.Tasks;

namespace Model
{
	public interface IEventAsync
	{
		Task RunAsync(Env env);
	}
}