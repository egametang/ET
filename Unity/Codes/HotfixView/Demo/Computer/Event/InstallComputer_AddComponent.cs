using ET.EventType;

namespace ET
{
    public class InstallComputer_AddComponent:AEvent<EventType.InstallConmputer>
    {
        protected override async ETTask Run(InstallConmputer arg)
        {
            await TimerComponent.Instance.WaitAsync(2000);
            
            arg.Computer.AddComponent<PCCaseComponent>();
            arg.Computer.AddComponent<MonitorsComponent>();
        }
    }
}