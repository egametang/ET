using System;
using System.ComponentModel;

namespace Base
{
	public interface ICategory: ISupportInitialize
	{
		Type ConfigType { get; }
	}
}