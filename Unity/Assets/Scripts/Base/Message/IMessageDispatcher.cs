namespace Model
{
	public interface IMessageDispatcher
	{
		void Dispatch(Session session, ushort opcode, int offset, byte[] messageBytes, AMessage message);
	}
}
