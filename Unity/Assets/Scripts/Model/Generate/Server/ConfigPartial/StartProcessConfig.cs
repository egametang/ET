using System.Net;

namespace ET
{
    public partial class StartProcessConfig
    {
        public long SceneId;

        public string InnerIP => this.StartMachineConfig.InnerIP;

        public string OuterIP => this.StartMachineConfig.OuterIP;

        public StartMachineConfig StartMachineConfig => StartMachineConfigCategory.Instance.Get(this.MachineId);

        public override void EndInit()
        {
            InstanceIdStruct instanceIdStruct = new(0, (uint)this.Id);
            this.SceneId = instanceIdStruct.ToLong();
            Log.Info($"StartProcess info: {this.MachineId} {this.Id} {this.SceneId}");
        }
    }
}