using System.Net;

namespace ETModel
{
	/// <summary>
	/// 用来与数据库操作代理
	/// </summary>
	public class DBProxyComponent: Component
	{
		public IPEndPoint dbAddress;
		
		public MultiMap<string, object> TcsQueue = new MultiMap<string, object>();
	}
}