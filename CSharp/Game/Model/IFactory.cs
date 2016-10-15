using Common.Base;

namespace Model
{
	public interface IFactory<out T> where T : Entity<T>
	{
		T Create(int configId);
	}
}