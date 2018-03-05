namespace ETModel
{
	public interface IMessageDispatcher
	{
		void Dispatch(Session session, PacketInfo packetInfo);
	}
}
