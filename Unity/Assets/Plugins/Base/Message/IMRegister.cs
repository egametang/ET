namespace Model
{
	public interface IMRegister<in T>
	{
		void Register(T component, ushort opcode);
	}
}
