namespace Model
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
	public abstract class AConfig: Entity
	{
		protected AConfig()
		{
		}

		protected AConfig(long id): base(id)
		{
		}
	}
}