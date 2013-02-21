using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Helper;

namespace Modules.Robot
{
	[DataContract]
	public class ServerConfig
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string Host { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public string Name { get; set; }

		[DataMember(Order = 3, IsRequired = true)]
		public uint Port { get; set; }
	}

	[DataContract]
	public class CityServerConfig
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string CityServerName { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public ServerConfig ServerConfig { get; set; }

		[DataMember(Order = 3, IsRequired = true)]
		public int FrameMilliseconds { get; set; }

		[DataMember(Order = 4, IsRequired = true)]
		public int Icon { get; set; }

		[DataMember(Order = 5, IsRequired = true)]
		public int Color { get; set; }

		[DataMember(Order = 6, IsRequired = true)]
		public int TimeZone { get; set; }

		[DataMember(Order = 7, IsRequired = true)]
		public int AllowedSecurityLevel { get; set; }

		[DataMember(Order = 8, IsRequired = true)]
		public double Population { get; set; }

		[DataMember(Order = 9, IsRequired = true)]
		public string RealmBuilds { get; set; }

		[DataMember(Order = 10, IsRequired = true)]
		public string CityDisplayName { get; set; }

		[DataMember(Order = 11, IsRequired = true)]
		public string MapConfigFilePath { get; set; }

		[DataMember(Order = 13, IsRequired = true)]
		public string FriendConfigFilePath { get; set; }

		[DataMember(Order = 14, IsRequired = true)]
		public string GmCommandPermitFile { get; set; }

		[DataMember(Order = 15, IsRequired = true)]
		public string AchievementProtoPath { get; set; }

		[DataMember(Order = 16, IsRequired = true)]
		public string CityTaskPath { get; set; }

		[DataMember(Order = 17, IsRequired = true)]
		public int MaxPlayers { get; set; }

		[DataMember(Order = 18, IsRequired = true)]
		public int DenyRoomInviteCooldownSeconds { get; set; }

		[DataMember(Order = 19, IsRequired = true)]
		public string CityNpcPath { get; set; }

		[DataMember(Order = 20, IsRequired = true)]
		public string PythonDataPath { get; set; }

		[DataMember(Order = 21, IsRequired = true)]
		public string CityDataPath { get; set; }
	}

	[DataContract]
	public class MapServerConfig 
	{
		[DataMember(Order = 1, IsRequired = true)]
		public string MapServerName { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public ServerConfig ServerConfig { get; set; }

		[DataMember(Order = 3, IsRequired = true)]
		public int FrameMilliseconds { get; set; }

		[DataMember(Order = 4, IsRequired = true)]
		public string MapConfigFilePath { get; set; }

		[DataMember(Order = 5, IsRequired = true)]
		public string CityTaskPath { get; set; }

		[DataMember(Order = 7, IsRequired = true)]
		public string MapDataPath { get; set; }

		[DataMember(Order = 6, IsRequired = true)]
		public string PythonDataPath { get; set; }

		[DataMember(Order = 8, IsRequired = true)]
		public string AchievementProtoPath { get; set; }
	}

	[DataContract]
	public class WorldServiceConfig
	{
		[DataMember(Order = 1, IsRequired = true)]
		public List<CityServerConfig> CityServerConfig { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public List<MapServerConfig> MapServerConfig { get; set; }
	}

	[DataContract]
	public class Config
	{
		private static readonly Config instance;

		static Config()
		{
			string content = File.ReadAllText(@"..\App\Config\Config.json");
			instance = JsonHelper.FromString<Config>(content);
		}

		public static Config Instance
		{
			get
			{
				return instance;
			}
		}

		[DataMember(Order = 1, IsRequired = true)]
		public string IP { get; set; }

		[DataMember(Order = 2, IsRequired = true)]
		public ushort Port { get; set; }
	}
}
