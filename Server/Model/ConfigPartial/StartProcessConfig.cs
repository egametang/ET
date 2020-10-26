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
		private string innerAddress;

		public string InnerAddress
		{
			get
			{
				if (this.innerAddress == null)
				{
					this.innerAddress = $"{StartMachineConfigCategory.Instance.Get(this.MachineId).InnerIP}:{this.InnerPort}";
				}
				return this.innerAddress;
			}
		}

		public string OuterIP
		{
			get
			{
				return StartMachineConfigCategory.Instance.Get(this.MachineId).OuterIP;
			}
		}

		public void BeginInit()
		{
		}

		public void EndInit()
		{
		}
	}
}
