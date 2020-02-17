using System.ComponentModel;

namespace ETModel
{
	public partial class StartProcessConfigCategory : ISupportInitialize
	{
		
		
		public void EndInit()
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
