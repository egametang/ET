using System;

namespace ET
{
	public abstract class AEvent<A> where A: struct
	{
		public abstract void Run(A a);
	}
}