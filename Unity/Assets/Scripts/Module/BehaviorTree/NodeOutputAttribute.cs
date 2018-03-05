using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Field)]
	public class NodeOutputAttribute: NodeFieldBaseAttribute
	{
		public NodeOutputAttribute(string desc, Type _envKeyType, string value = BTEnvKey.None): base(desc, value, _envKeyType)
		{
			//              if (_envKeyType == null)
			//              {
			//                 Log.Error($"_envKeyType can't be null");
			//                 return;
			//              }
			envKeyType = _envKeyType;
		}
	}
}