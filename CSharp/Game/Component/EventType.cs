
namespace Component
{
	// 定义事件类型
	public enum EventType: short
	{
		// 登录world前触发
		BeforeLoginWorldEvent,

		// 使用物品前触发
		BeforeUseItemEvent,
	}
}
