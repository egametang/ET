#if !SERVER
using UnityEngine;
#endif

namespace ETModel
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
#if !SERVER
	[HideInHierarchy]
#endif
	public abstract class AConfigComponent: Component, ISerializeToEntity
	{
	}
}