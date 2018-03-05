using System;

namespace ETModel
{
	[AttributeUsage(AttributeTargets.Field)]
	public class NodeInputAttribute: NodeFieldBaseAttribute
	{
		public NodeInputAttribute(string desc, Type _envKeyType): base(desc, null, _envKeyType)
		{
			//             if (_envKeyType == null)
			//             {
			//                 Log.Error($"{desc}_envKeyType can't be null");
			//                 return;
			//             }
			this.envKeyType = _envKeyType;
		}
	}
}