using System;
using System.Collections.Generic;

namespace Model
{
	public class ServerType: Attribute
	{
		
	}

	public enum AppType
	{
		Client,
		Robot,
		
		Manager,
		Realm,
		Gate,
	}

	public static class AppTypeHelper
	{
		public static List<AppType> GetServerTypes()
		{
			List<AppType> appTypes = new List<AppType>() { AppType.Manager, AppType.Realm, AppType.Gate };
			return appTypes;
		}
	}
}
