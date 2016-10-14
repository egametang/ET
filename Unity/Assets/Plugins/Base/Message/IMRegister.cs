namespace Base
{
	public interface IMRegister<in T>
	{
		void Register(T component, ushort opcode);
	}
}
