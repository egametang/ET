﻿using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public class HttpConfig : AConfigComponent
	{
		public string Url { get; set; } = "";
		public int AppId { get; set; }
		public string AppKey { get; set; } = "";
		public string ManagerSystemUrl { get; set; } = "";
	}
}