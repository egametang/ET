namespace Model
{
	/// <summary>
	/// 搭配EventComponent用来分发消息
	/// </summary>
	public class MessageAttribute: AEventAttribute
	{
		public MessageAttribute(int type, params ServerType[] serverTypes)
			: base(type, serverTypes)
		{
		}
	}
}