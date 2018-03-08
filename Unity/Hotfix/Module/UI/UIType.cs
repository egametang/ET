using System;
using System.Collections.Generic;

namespace ETHotfix
{
    public static class UIType
    {
	    public const int Root = 0;
	    public const int UILogin = 1;
	    public const int UILobby = 2;

	    public static Dictionary<int, string> UIName = new Dictionary<int, string>()
	    {
			{Root,            "Root" },
			{UILogin,         "UILogin" },
			{UILobby,         "UILobby" },
		};

	    public static string GetUIName(int type)
	    {
		    try
		    {
			    return UIName[type];
		    }
		    catch (Exception e)
		    {
			    throw new Exception($"not found ui: {type}", e);
		    }
	    }
	}
}