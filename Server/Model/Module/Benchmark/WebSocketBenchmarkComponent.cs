using System;
using System.Net;
using System.Threading.Tasks;

namespace ETModel
{
	public class WebSocketBenchmarkComponent: Component
	{
		public int k;

		public long time1 = TimeHelper.ClientNow();
	}
}