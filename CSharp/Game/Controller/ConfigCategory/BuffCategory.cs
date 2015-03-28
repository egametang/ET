using Model;

namespace Controller
{
	[Config(ServerType.Realm | ServerType.Gate | ServerType.City)]
	public class BuffCategory: ACategory<BuffConfig>
	{
	}
}