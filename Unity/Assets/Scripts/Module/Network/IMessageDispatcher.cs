namespace Model
{
	public interface IMessageDispatcher
	{
		void Dispatch(Session session, PacketInfo packetInfo);
	}
}
