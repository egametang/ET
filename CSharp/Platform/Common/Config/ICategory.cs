using System;
using System.ComponentModel;

namespace Common.Config
{
	public interface ICategory: ISupportInitialize
	{
		Type ConfigType { get; }
	}
}