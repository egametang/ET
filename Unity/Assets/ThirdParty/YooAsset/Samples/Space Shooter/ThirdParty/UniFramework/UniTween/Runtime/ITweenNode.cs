
namespace UniFramework.Tween
{
	public interface ITweenNode
	{
		/// <summary>
		/// 节点状态
		/// </summary>
		ETweenStatus Status { get; }

		void OnUpdate(float deltaTime);
		void OnDispose();
		void Abort();
	}
}