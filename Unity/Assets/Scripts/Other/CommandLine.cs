using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class CommandLines: ISupportInitialize
	{
		[BsonIgnore]
		public Options Manager { get; private set; }

		[BsonIgnore]
		public Options Realm { get; private set; }
		
		public List<Options> Options = new List<Options>();

		public void BeginInit()
		{
		}

		public void EndInit()
		{
			foreach (Options options in this.Options)
			{
				if (options.AppType == AppType.Realm)
				{
					this.Realm = options;
				}

				if (options.AppType == AppType.Manager)
				{
					this.Manager = options;
				}
			}
		}
	}
}
