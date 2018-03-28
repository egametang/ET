namespace ETHotfix
{
	public static class AssetBundleHelper
	{
		public static string GetBundleNameById(string id, EntityType entityType = EntityType.None)
		{
			string subString = id.Substring(0, 3);
			switch (subString[0])
			{
				case '1':
				case '2':
				case '3':
					return "3000";
				case '4':
				case '5':
				case '6':
				case '8':
					return subString;
				case '9':
					return "900";
			}
			return subString;
		}
	}
}