using System;
using System.ComponentModel;

namespace Hotfix
{
	public interface ICategory: ISupportInitialize
	{
		Type ConfigType { get; }
	}
}