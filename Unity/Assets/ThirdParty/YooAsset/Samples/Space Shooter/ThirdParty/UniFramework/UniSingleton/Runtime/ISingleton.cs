
namespace UniFramework.Singleton
{
	public interface ISingleton
	{
		/// <summary>
		/// 创建单例
		/// </summary>
		void OnCreate(System.Object createParam);

		/// <summary>
		/// 更新单例
		/// </summary>
		void OnUpdate();

		/// <summary>
		/// 销毁单例
		/// </summary>
		void OnDestroy();
	}
}