using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	/// <summary>
	/// 每个Config的基类
	/// </summary>
	[BsonKnownTypes(typeof (ClientConfig))]
	[BsonKnownTypes(typeof (InnerConfig))]
	[BsonKnownTypes(typeof (OuterConfig))]
	public abstract class AConfigComponent: HotfixComponent
	{
	}
}