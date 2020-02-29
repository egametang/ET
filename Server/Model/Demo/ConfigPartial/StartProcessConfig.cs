using System.ComponentModel;

namespace ET
{
	public partial class StartProcessConfigCategory
	{
		public override void EndInit()
		{
		}
	}
	
	public partial class StartProcessConfig: ISupportInitialize
	{
		public string InnerAddress;

		public void BeginInit()
		{
		}

		public void EndInit()
		{
			this.InnerAddress = $"{this.InnerIP}:{this.InnerPort}";
		}
	}
}
