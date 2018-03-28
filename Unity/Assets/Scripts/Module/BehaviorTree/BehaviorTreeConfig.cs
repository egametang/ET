using UnityEngine;

namespace ETModel
{
	public class BehaviorTreeConfig: MonoBehaviour
	{
		public BehaviorNodeConfig RootNodeConfig;
		private int mAutoId = 1;

		public int AutoId
		{
			get
			{
				return mAutoId++;
			}
		}

		public int RootNodeId
		{
			get
			{
				return RootNodeConfig == null? 0 : RootNodeConfig.id;
			}
		}

		public NodeProto RootNodeProto
		{
			get
			{
				return RootNodeConfig == null? null : RootNodeConfig.ToNodeProto();
			}
		}

		public void Clear()
		{
			DestroyImmediate(RootNodeConfig, true);
			RootNodeConfig = null;
		}
	}
}