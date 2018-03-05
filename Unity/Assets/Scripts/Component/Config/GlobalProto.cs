namespace ETModel
{
	public class GlobalProto
	{
		public string AssetBundleServerUrl;
		public string Address;

		public string GetUrl()
		{
			string url = this.AssetBundleServerUrl;
#if UNITY_ANDROID
			url += "Android/";
#elif UNITY_IOS
			url += "IOS/";
#else
			url += "PC/";
#endif
			return url;
		}
	}
}
