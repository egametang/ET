using System;
using System.ComponentModel;

namespace Model
{
	public interface ICategory: ISupportInitialize
	{
		Type ConfigType { get; }
	}
}