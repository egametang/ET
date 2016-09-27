using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	public enum AssetUrlEnum
	{
		Local,
		Remote,
		RemoteAliWeb
	}

	public enum ConfigPathEnum
	{
		Editor,
		Launcher,
		Release,
	}

	[BsonIgnoreExtraElements]
	public class GlobalProto: AConfig
	{
		public NetworkProtocol Protocol = NetworkProtocol.TCP;
		public string AssetBundleServerUrl = "http://192.168.11.200/";
		public AssetUrlEnum AssetUrlType = AssetUrlEnum.Remote;
		public string Host = "192.168.11.200";
        public ushort Port = 22222;
	    public string AssetBundleName = "global";
		
        [BsonDefaultValue(false)]
        public bool IsEnglishEdition = false;
    }
}