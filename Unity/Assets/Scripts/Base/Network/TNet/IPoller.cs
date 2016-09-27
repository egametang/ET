using System;

namespace Base
{
	public interface IPoller
	{
		void Add(Action action);

		void Update();
	}
}