
namespace Component
{
	// 定义事件类型
	public enum EventType: short
	{
		// 登录world前触发
		LoginWorldBeforeEvent,
	}

	public enum EventNumber: short
	{
		#region    LoginWorldBeforeEvent

		CheckPlayerEvent,

		#endregion LoginWorldBeforeEvent
	}
}
