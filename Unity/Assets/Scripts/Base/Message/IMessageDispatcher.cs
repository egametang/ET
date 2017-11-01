namespace Model
{
	public interface IMessageDispatcher
	{
		void Dispatch(Session session, Opcode opcode, int offset, byte[] messageBytes, AMessage message);
	}
}
