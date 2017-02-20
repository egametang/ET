namespace Model
{
	public abstract class IInstanceMethod
	{
		public string Name { get; protected set; }
		public abstract void Run(params object[] param);
	}

	public abstract class IStaticMethod
	{
		public string Name { get; protected set; }
		public abstract void Run(object instance, params object[] param);
	}
}
