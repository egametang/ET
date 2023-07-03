using System.Net;

namespace ET
{
    public partial class StartProcessConfig
    {
        public string InnerIP => this.StartMachineConfig.InnerIP;

        public string OuterIP => this.StartMachineConfig.OuterIP;

        public StartMachineConfig StartMachineConfig => StartMachineConfigCategory.Instance.Get(this.MachineId);

        public override void EndInit()
        {
        }
    }
}