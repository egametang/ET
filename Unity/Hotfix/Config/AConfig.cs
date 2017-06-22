namespace Hotfix
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
	public abstract class AConfig: HotfixEntity
	{
		protected AConfig(EntityType entityType): base(entityType)
		{
		}

		protected AConfig(long id, EntityType entityType): base(id, entityType)
		{
		}
	}
}