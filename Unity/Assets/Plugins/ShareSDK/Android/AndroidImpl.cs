using System;
using System.Collections;
using UnityEngine; 

namespace cn.sharesdk.unity3d
{
	#if UNITY_ANDROID
	public class AndroidImpl : ShareSDKImpl
	{
		private AndroidJavaObject ssdk;

		public AndroidImpl (GameObject go) 
		{
			Debug.Log("AndroidImpl  ===>>>  AndroidImpl" );
			try{
				ssdk = new AndroidJavaObject("cn.sharesdk.unity3d.ShareSDKUtils", go.name, "_Callback");
			} catch(Exception e) {
				Console.WriteLine("{0} Exception caught.", e);
			}
		}

		public override void InitSDK (String appKey) 
		{
			Debug.Log("AndroidImpl  ===>>>  InitSDK === " + appKey);
			if (ssdk != null) 
			{			
				ssdk.Call("initSDK", appKey);
			}
		}

		public override void SetPlatformConfig (Hashtable configs) 
		{
			String json = MiniJSON.jsonEncode(configs);
			Debug.Log("AndroidImpl  ===>>>  SetPlatformConfig === " + json);
			if (ssdk != null) 
			{			
				ssdk.Call("setPlatformConfig", json);
			}
		}

		public override void Authorize (int reqID, PlatformType platform) 
		{
			Debug.Log("AndroidImpl  ===>>>  Authorize" );
			if (ssdk != null) 
			{
				ssdk.Call("authorize", reqID, (int)platform);
			}
		}

		public override void CancelAuthorize (PlatformType platform) 
		{
			if (ssdk != null) 
			{
				ssdk.Call("removeAccount", (int)platform);
			}
		}

		public override bool IsAuthorized (PlatformType platform) 
		{
			if (ssdk != null) 
			{
				return ssdk.Call<bool>("isAuthValid", (int)platform);
			}
			return false;
		}

		public override bool IsClientValid (PlatformType platform) 
		{
			if (ssdk != null) 
			{
				return ssdk.Call<bool>("isClientValid", (int)platform);
			}
			return false;
		}

		public override void GetUserInfo (int reqID, PlatformType platform) 
		{
			Debug.Log("AndroidImpl  ===>>>  ShowUser" );
			if (ssdk != null) 
			{
				ssdk.Call("showUser", reqID, (int)platform);
			}
		}

		public override void ShareContent (int reqID, PlatformType platform, ShareContent content) 
		{
			Debug.Log("AndroidImpl  ===>>>  ShareContent to one platform" );
			ShareContent (reqID, new PlatformType[]{ platform }, content);
		}

		public override void ShareContent (int reqID, PlatformType[] platforms, ShareContent content) 
		{
			Debug.Log("AndroidImpl  ===>>>  Share" );
			if (ssdk != null) 
			{
				foreach (PlatformType platform in platforms)
				{
					ssdk.Call("shareContent", reqID, (int)platform, content.GetShareParamsStr());
				}
			}
		}

		public override void ShowPlatformList (int reqID, PlatformType[] platforms, ShareContent content, int x, int y) 
		{
			ShowShareContentEditor(reqID, 0, content);
		}

		public override void ShowShareContentEditor (int reqID, PlatformType platform, ShareContent content) 
		{
			Debug.Log("AndroidImpl  ===>>>  OnekeyShare platform ===" + (int)platform );
			if (ssdk != null) 
			{
				ssdk.Call("onekeyShare", reqID, (int)platform, content.GetShareParamsStr());
			}
		}
		
		public override void GetFriendList (int reqID, PlatformType platform, int count, int page) 
		{
			Debug.Log("AndroidImpl  ===>>>  GetFriendList" );
			if (ssdk != null) 
			{
				ssdk.Call("getFriendList", reqID, (int)platform, count, page);
			}
		}

		public override void AddFriend (int reqID, PlatformType platform, String account)
		{
			Debug.Log("AndroidImpl  ===>>>  FollowFriend" );
			if (ssdk != null) 
			{
				ssdk.Call("followFriend", reqID, (int)platform, account);
			}
		}

		public override Hashtable GetAuthInfo (PlatformType platform) 
		{
			Debug.Log("AndroidImpl  ===>>>  GetAuthInfo" );
			if (ssdk != null) 
			{
				String result = ssdk.Call<String>("getAuthInfo", (int)platform);
				return (Hashtable) MiniJSON.jsonDecode(result);
			}
			return new Hashtable ();
		}

		public override void DisableSSO (Boolean disable)
		{
			Debug.Log("AndroidImpl  ===>>>  DisableSSOWhenAuthorize" );
			if (ssdk != null) 
			{
				ssdk.Call("disableSSOWhenAuthorize", disable);
			}
		}

		public override void ShareWithContentName (int reqId, PlatformType platform, string contentName, Hashtable customFields)
		{
			Debug.Log("#WARING : Do not support this feature in Android temporarily" );
		}
		
		public override void ShowPlatformListWithContentName (int reqId, string contentName, Hashtable customFields, PlatformType[] platforms, int x, int y)
		{
			Debug.Log("#WARING : Do not support this feature in Android temporarily" );
		}

		public override void ShowShareContentEditorWithContentName (int reqId, PlatformType platform, string contentName, Hashtable customFields)
		{
			Debug.Log("#WARING : Do not support this feature in Android temporarily" );
		}

	}
	#endif
}
