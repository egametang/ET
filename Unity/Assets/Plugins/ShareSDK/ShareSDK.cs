using UnityEngine;
using System.Collections;
using System;  
using System.Collections.Generic;  
using System.IO;  
using System.Text;
using System.Reflection;

namespace cn.sharesdk.unity3d
{
	/// <summary>
	/// ShareSDK.
	/// </summary>
	public class ShareSDK : MonoBehaviour 
	{
		private int reqID;
		//配置ShareSDK AppKey
		//注:此处区分仅为demo测试而区分，实际使用时可以不区分安卓或iOS
		//#if UNITY_ANDROID
		public string appKey = "androidv1101";
		//#elif UNITY_IPHONE
		//public string appKey = "iosv1101";
		//#endif
		public DevInfoSet devInfo;
		public ShareSDKImpl shareSDKUtils;

		public EventHandler authHandler;
		public EventHandler shareHandler;
		public EventHandler showUserHandler;
		public EventHandler getFriendsHandler;
		public EventHandler followFriendHandler;

		void Awake()
		{
			return;
			print("ShareSDK Awake");
			Type type = devInfo.GetType();
			Hashtable platformConfigs = new Hashtable();
			FieldInfo[] devInfoFields = type.GetFields();
			foreach (FieldInfo devInfoField in devInfoFields) 
			{	
				DevInfo info = (DevInfo) devInfoField.GetValue(devInfo);
				int platformId = (int) info.GetType().GetField("type").GetValue(info);
				FieldInfo[] fields = info.GetType().GetFields();
				Hashtable table = new Hashtable();
				foreach (FieldInfo field in fields) 
				{
					if ("type".EndsWith(field.Name)) {
						continue;
					} else if ("Enable".EndsWith(field.Name) || "ShareByAppClient".EndsWith(field.Name) || "BypassApproval".EndsWith(field.Name)) {
						table.Add(field.Name, Convert.ToString(field.GetValue(info)).ToLower());
					} else {
						table.Add(field.Name, Convert.ToString(field.GetValue(info)));
					}
				}
				platformConfigs.Add(platformId, table);
			}

			#if UNITY_ANDROID
			shareSDKUtils = new AndroidImpl(gameObject);
			#elif UNITY_IPHONE
			shareSDKUtils = new iOSImpl(gameObject);
			#endif
			shareSDKUtils.InitSDK(appKey);
			shareSDKUtils.SetPlatformConfig(platformConfigs);
		}
		
		/// <summary>
		/// callback the specified data.
		/// </summary>
		/// <param name='data'>
		/// Data.
		/// </param>
		private void _Callback (string data)
		{
			if (data == null) 
			{
				return;
			}
			
			Hashtable res = (Hashtable) MiniJSON.jsonDecode(data);
			if (res == null || res.Count <= 0) 
			{
				return;
			}
			
			int status = Convert.ToInt32(res["status"]);
			int reqID = Convert.ToInt32(res["reqID"]);
			PlatformType platform = (PlatformType)Convert.ToInt32(res["platform"]);
			int action = Convert.ToInt32(res["action"]);
			// Success = 1, Fail = 2, Cancel = 3
			switch(status) 
			{
				case 1: 
				{
					Console.WriteLine(data);
					Hashtable resp = (Hashtable) res["res"];
					OnComplete(reqID, platform, action, resp);
					break;
				} 
				case 2: 
				{
					Console.WriteLine(data);
					Hashtable throwable = (Hashtable) res["res"];
					OnError(reqID, platform, action, throwable);
					break;
				} 
				case 3: 
				{
					OnCancel(reqID, platform, action);
					break;
				} 
			}
		}

		/// <summary>
		/// Raises the error event.
		/// </summary>
		/// <param name="platform">Platform.</param>
		/// <param name="action">Action.</param>
		/// <param name="throwable">Throwable.</param>
		public void OnError (int reqID, PlatformType platform, int action, Hashtable throwable) 
		{
			switch (action) 
			{
			case 1: 
			{ // 1 == Platform.ACTION_AUTHORIZING
				if (authHandler != null) 
				{
					authHandler(reqID, ResponseState.Fail, platform, throwable);
				}
				break;
			} 
			case 2:
			{ //2 == Platform.ACTION_GETTING_FRIEND_LIST
				if (getFriendsHandler != null) 
				{
					getFriendsHandler(reqID, ResponseState.Fail, platform, throwable);
				}
				break;
			}
			case 6:
			{ //6 == Platform.ACTION_FOLLOWING_USER
				if (followFriendHandler != null) 
				{
					followFriendHandler(reqID, ResponseState.Fail, platform, throwable);
				}
				break;
			}
			case 8: 
			{ // 8 == Platform.ACTION_USER_INFOR
				if (showUserHandler != null) 
				{
					showUserHandler(reqID, ResponseState.Fail, platform, throwable);
				}
				break;
			} 
			case 9: 
			{ // 9 == Platform.ACTION_SHARE
				if (shareHandler != null) 
				{
					shareHandler(reqID, ResponseState.Fail, platform, throwable);
				}
				break;
			} 
			}
		}

		/// <summary>
		/// Raises the success event.
		/// </summary>
		/// <param name="platform">Platform.</param>
		/// <param name="action">Action.</param>
		/// <param name="res">Res.</param>
		public void OnComplete (int reqID, PlatformType platform, int action, Hashtable res) 
		{
			switch (action) 
			{
			case 1: 
			{ // 1 == Platform.ACTION_AUTHORIZING
				if (authHandler != null) 
				{
					authHandler(reqID, ResponseState.Success, platform, null);
				}
				break;
			} 
			case 2:
			{ //2 == Platform.ACTION_GETTING_FRIEND_LIST
				if (getFriendsHandler != null) 
				{
					getFriendsHandler(reqID, ResponseState.Success, platform, res);
				}
				break;
			}
			case 6:
			{ //6 == Platform.ACTION_FOLLOWING_USER
				if (followFriendHandler != null) 
				{
					followFriendHandler(reqID, ResponseState.Success, platform, res);
				}
				break;
			}
			case 8: 
			{ // 8 == Platform.ACTION_USER_INFOR
				if (showUserHandler != null) 
				{
					showUserHandler(reqID, ResponseState.Success, platform, res);
				}
				break;
			} 
			case 9: 
			{ // 9 == Platform.ACTION_SHARE
				if (shareHandler != null) 
				{
					shareHandler(reqID, ResponseState.Success, platform, res);
				}
				break;
			}
			}
		}

		/// <summary>
		/// Raises the cancel event.
		/// </summary>
		/// <param name="platform">Platform.</param>
		/// <param name="action">Action.</param>
		public void OnCancel (int reqID, PlatformType platform, int action) 
		{
			switch (action) 
			{
			case 1: 
			{ // 1 == Platform.ACTION_AUTHORIZING
				if (authHandler != null) 
				{
					authHandler(reqID, ResponseState.Cancel, platform, null);
				}
				break;
			} 
			case 2:
			{ //2 == Platform.ACTION_GETTING_FRIEND_LIST
				if (getFriendsHandler != null) 
				{
					getFriendsHandler(reqID, ResponseState.Cancel, platform, null);
				}
				break;
			}
			case 6:
			{ //6 == Platform.ACTION_FOLLOWING_USER
				if (followFriendHandler != null) 
				{
					followFriendHandler(reqID, ResponseState.Cancel, platform, null);
				}
				break;
			}
			case 8: 
			{ // 8 == Platform.ACTION_USER_INFOR
				if (showUserHandler != null) 
				{
					showUserHandler(reqID, ResponseState.Cancel, platform, null);
				}
				break;
			} 
			case 9: 
			{ // 9 == Platform.ACTION_SHARE
				if (shareHandler != null) 
				{
					shareHandler(reqID, ResponseState.Cancel, platform, null);
				}
				break;
			}
			}
		}
		
		/// <summary>
		/// init the ShareSDK.
		/// </summary>
		public void InitSDK (String appKey)
		{			
			// if you don't add ShareSDK.xml in your assets folder, use the following line
			shareSDKUtils.InitSDK (appKey);
		}

		/// <summary>
		/// Sets the platform config.
		/// </summary>
		public void SetPlatformConfig (Hashtable configInfo)
		{			
			shareSDKUtils.SetPlatformConfig (configInfo);			
		}
		
		/// <summary>
		/// Authorize the specified type, observer and resultHandler.
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		/// <param name='observer'>
		/// Observer.
		/// </param>
		/// <param name='resultHandler'>
		/// Result handler.
		/// </param>
		public int Authorize (PlatformType platform)
		{
			reqID ++;
			shareSDKUtils.Authorize(reqID, platform);			
			return reqID;
		}
		
		/// <summary>
		/// Cancel authorized
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		public void CancelAuthorize (PlatformType platform)
		{
			shareSDKUtils.CancelAuthorize(platform);			
		}
		
		/// <summary>
		/// Has authorized
		/// </summary>
		/// <returns>
		/// true has authorized, otherwise not authorized.
		/// </returns>
		/// <param name='type'>
		/// Type.
		/// </param>
		public bool IsAuthorized (PlatformType platform)
		{
			return shareSDKUtils.IsAuthorized(platform);			
		}

		public bool IsClientValid (PlatformType platform)
		{
			return shareSDKUtils.IsClientValid(platform);			
		}
		
		/// <summary>
		/// Gets the user info.
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		/// <param name='callback'>
		/// Callback.
		/// </param>
		public int GetUserInfo (PlatformType platform)
		{
			reqID ++;
			shareSDKUtils.GetUserInfo(reqID, platform);			
			return reqID;
		}

		/// <summary>
		/// Shares the content.
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		/// <param name='content'>
		/// Content.
		/// </param>
		/// <param name='resultHandler'>
		/// Callback.
		/// </param>
		public int ShareContent(PlatformType platform, ShareContent content)
		{
			reqID ++;
			shareSDKUtils.ShareContent(reqID, platform, content);			
			return reqID;
		}

		/// <summary>
		/// Shares the content.
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		/// <param name='content'>
		/// Content.
		/// </param>
		/// <param name='resultHandler'>
		/// Callback.
		/// </param>
		public int ShareContent(PlatformType[] platforms, ShareContent content)
		{
			reqID ++;
			shareSDKUtils.ShareContent(reqID, platforms, content);			
			return reqID;
		}
				
		/// <summary>
		/// Shows the share menu of using onekeyshare.
		/// </summary>
		/// <param name='types'>
		/// Types.
		/// </param>
		/// <param name='content'>
		/// Content.
		/// </param>
		/// <param name='callback'>
		/// Callback.
		/// </param>
		public int ShowPlatformList (PlatformType[] platforms, ShareContent content, int x, int y)
		{
			reqID ++;
			shareSDKUtils.ShowPlatformList(reqID, platforms, content, x, y);			
			return reqID;
		}
		
		/// <summary>
		/// Shows the share view of using onekeyshare.
		/// </summary>
		/// <param name='type'>
		/// Type.
		/// </param>
		/// <param name='content'>
		/// Content.
		/// </param>
		/// <param name='callback'>
		/// Callback.
		/// </param>
		public int ShowShareContentEditor (PlatformType platform, ShareContent content)
		{			
			reqID ++;
			shareSDKUtils.ShowShareContentEditor(reqID, platform, content);		
			return reqID;
		}

		/// <summary>
		/// share according to the name of node<Content> in ShareContent.xml(you can find it in Xcode) [only valid in iOS temporarily][此接口暂时仅支持iOS环境]
		/// </summary>
		/// <param name='platform'>
		/// Platform Type
		/// </param>
		/// <param name='contentName'>
		/// the name of node<Content> in ShareContent.xml file
		/// </param>
		/// <param name='customFields'>
		/// your share customFields which will be replace in ShareContent.xml
		/// </param>
		public int ShareWithContentName (PlatformType platform, string contentName, Hashtable customFields)
		{
			reqID++;
			shareSDKUtils.ShareWithContentName (reqID, platform, contentName, customFields);
			return reqID;
		}

		/// <summary>
		/// share according to the name of node<Content> in ShareContent.xml(you can find it in Xcode)   (only valid in iOS temporarily)(此接口暂时仅支持iOS环境)
		/// </summary>
		/// </param>
		/// <param name='contentName'>
		/// the name of node<Content> in ShareContent.xml file
		/// </param>
		/// <param name='customFields'>
		/// your share customFields which will be replace in ShareContent.xml
		/// </param>
		/// <param name='platforms'>
		/// Platform Types
		/// </param>
		/// <param name='x','y'>
		/// the coordinates of the share menu
		/// </param>
		public int ShowPlatformListWithContentName (string contentName, Hashtable customFields, PlatformType[] platforms, int x, int y)
		{
			reqID++;
			shareSDKUtils.ShowPlatformListWithContentName (reqID, contentName, customFields, platforms, x, y);
			return reqID;
		}

		/// <summary>
		/// share according to the name of node<Content> in ShareContent.xml file  (only valid in iOS temporarily)(此接口暂时仅支持iOS环境)
		/// </summary>
		/// <param name='platform'>
		/// Platform Type
		/// </param>
		/// <param name='contentName'>
		/// the name of node<Content> in ShareContent.xml file
		/// </param>
		/// <param name='customFields'>
		/// your share customFields which will be replace in ShareContent.xml
		/// </param>
		public int ShowShareContentEditorWithContentName (PlatformType platform, string contentName, Hashtable customFields)
		{
			reqID++;
			shareSDKUtils.ShowShareContentEditorWithContentName (reqID, platform, contentName, customFields);
			return reqID;
		}
		/// <summary>
		/// Gets the friends.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="count">Count.</param>
		/// <param name="page">Page.</param>
		public int GetFriendList (PlatformType platform, int count, int page)
		{
			reqID ++;
			shareSDKUtils.GetFriendList (reqID, platform, count, page);			
			return reqID;
		}

		/// <summary>
		/// Follows the friend.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="account">Account.</param>
		public int AddFriend (PlatformType platform, String account)
		{
			reqID ++;
			shareSDKUtils.AddFriend (reqID, platform, account);			
			return reqID;
		}

		/// <summary>
		/// Gets the auth info.
		/// </summary>
		/// <param name="type">Type.</param>
		public Hashtable GetAuthInfo (PlatformType platform)
		{
			return shareSDKUtils.GetAuthInfo (platform);			
		}

		/// <summary>
		/// Close the SSO when authorize.
		/// </summary>
		/// <param name="open">If set to <c>true</c> open.</param>
		public void DisableSSO(Boolean open)
		{
			shareSDKUtils.DisableSSO (open);			
		}
			
		/// <summary>
		/// Event result listener.
		/// </summary>
		public delegate void EventHandler (int reqID, ResponseState state, PlatformType type, Hashtable data);

	}
}