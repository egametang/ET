using Base;

namespace Model
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
	public abstract class AConfig: Entity
	{
		public AConfig(string entityType): base(entityType)
		{
		}

		public AConfig(long id, string entityType): base(id, entityType)
		{
		}
	}
}