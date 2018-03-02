using System;

namespace Hotfix
{
	public interface ISupportInitialize2
	{
		void BeginInit();
		void EndInit();
	}

	public interface ICategory: ISupportInitialize2
	{
		Type ConfigType { get; }
	}
}