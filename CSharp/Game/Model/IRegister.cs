namespace Model
{
	public interface IRegister
	{
		void Register();
	}

	public abstract class MEvent<T, R>: IRegister
	{
		public void Register()
		{
			World.Instance.GetComponent<MessageComponent>().Register<T, R>(this.Run);
		}

		public abstract R Run(T t);
	}
}