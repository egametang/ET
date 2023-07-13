
namespace UniFramework.Tween
{
	/// <summary>
	/// 补间动画状态
	/// </summary>
	public enum ETweenStatus
	{
		/// <summary>
		/// 空闲状态
		/// </summary>
		Idle,

		/// <summary>
		/// 运行中
		/// </summary>
		Runing,

		/// <summary>
		/// 已经完成
		/// </summary>
		Completed,

		/// <summary>
		/// 已被终止
		/// </summary>
		Abort,
	}
}