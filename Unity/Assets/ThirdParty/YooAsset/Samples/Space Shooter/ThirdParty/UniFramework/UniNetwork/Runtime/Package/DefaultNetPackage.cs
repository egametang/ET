
namespace UniFramework.Network
{
	public class DefaultNetPackage : INetPackage
	{
		/// <summary>
		/// 消息ID
		/// </summary>
		public int MsgID { set; get; }

		/// <summary>
		/// 包体数据
		/// </summary>
		public byte[] BodyBytes { set; get; }
	}
}