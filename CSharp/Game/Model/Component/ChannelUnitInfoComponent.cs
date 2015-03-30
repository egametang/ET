using Common.Base;
using Common.Network;
using MongoDB.Bson;

namespace Model
{
	/// <summary>
	/// channel中保存Unit一些信息,例如帐号
	/// </summary>
	public class ChannelUnitInfoComponent: Component<AChannel>
	{
		public byte[] Account { get; set; }
		public ObjectId UnitId { get; set; }
		public string ServerName { get; set; }
	}
}