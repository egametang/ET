using System;
using System.Threading.Tasks;
using Common.Event;
using Model;

namespace Controller.Message
{
	[Message(1)]
	internal class CMsgLogin: IEventAsync
	{
		public Task RunAsync(Env env)
		{
			throw new NotImplementedException();
		}
	}
}
