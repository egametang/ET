namespace Model
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
	public abstract class AConfig: Entity
	{
		public AConfig(EntityType entityType): base(entityType)
		{
		}

		public AConfig(long id, EntityType entityType): base(id, entityType)
		{
		}
	}
}