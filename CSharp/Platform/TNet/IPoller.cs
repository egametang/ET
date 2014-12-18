using System;

namespace TNet
{
	public interface IPoller
	{
		void Add(Action action);

		void RunOnce(int timeout);

		void Run();
	}
}
