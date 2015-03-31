namespace Model
{
	/// <summary>
	/// 搭配EventComponent用来分发消息
	/// </summary>
	public class MessageAttribute: AEventAttribute
	{
		public MessageAttribute(int type, ServerType serverType): base(type, serverType)
		{
		}
	}
}