namespace ETModel
{
	public interface IMessageDispatcher
	{
		void Dispatch(Session session, Packet packet);
	}
}
