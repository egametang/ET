using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using cn.sharesdk.unity3d.sdkporter;
using System.IO;


public static class ShareSDKPostProcessBuild {
	//[PostProcessBuild]
	[PostProcessBuildAttribute(88)]
	public static void onPostProcessBuild(BuildTarget target,string targetPath){
		string unityEditorAssetPath = Application.dataPath;

		if (target != BuildTarget.iOS) {
			Debug.LogWarning ("Target is not iPhone. XCodePostProcess will not run");
			return;
		}

		XCProject project = new XCProject (targetPath);
		//var files = System.IO.Directory.GetFiles( unityEditorAssetPath, "*.projmods", System.IO.SearchOption.AllDirectories );
		var files = System.IO.Directory.GetFiles( unityEditorAssetPath + "/Editor/SDKPorter", "*.projmods", System.IO.SearchOption.AllDirectories);
		foreach( var file in files ) {
			project.ApplyMod( file );
		}

		//如需要预配置Xocode中的URLScheme 和 白名单,请打开下两行代码,并自行配置相关键值
		string projPath = Path.GetFullPath (targetPath);
		EditInfoPlist (projPath);


		//Finally save the xcode project
		project.Save();

	}

	private static void EditInfoPlist(string projPath){

		XCPlist plist = new XCPlist (projPath);

		//URL Scheme 添加
		string PlistAdd = @"  
            <key>CFBundleURLTypes</key>
			<array>
				<dict>
					<key>CFBundleURLSchemes</key>
					<array>
						<string>dingoanxyrpiscaovl4qlw</string>
					</array>
					<key>CFBundleURLName</key>
					<string>dingtalk</string>
				</dict>
				<dict>
					<key>CFBundleURLSchemes</key>
					<array>
						<string>ap2015072400185895</string>
					</array>
					<key>CFBundleURLName</key>
					<string>alipayShare</string>
				</dict>
				<dict>
					<key>CFBundleURLSchemes</key>
					<array>
					<string>vk5312801</string>
					<string>yx0d9a9f9088ea44d78680f3274da1765f</string>
					<string>pin4797078908495202393</string>
					<string>kakao48d3f524e4a636b08d81b3ceb50f1003</string>
					<string>pdk4797078908495202393</string>
					<string>tb2QUXqO9fcgGdtGG1FcvML6ZunIQzAEL8xY6hIaxdJnDti2DYwM</string>
					<string>com.mob.demoShareSDK</string>
					<string>rm226427com.mob.demoShareSDK</string>
					<string>pocketapp1234</string>
					<string>QQ05FB8B52</string>
					<string>wx4868b35061f87885</string>
					<string>tencent100371282</string>
					<string>fb107704292745179</string>
					<string>wb568898243</string>
					</array>
				</dict>
			</array>";

		//白名单添加
		string LSAdd = @"
		<key>LSApplicationQueriesSchemes</key>
			<array>
			<string>dingtalk-open</string>
			<string>dingtalk</string>
			<string>mqqopensdkapiV4</string>
			<string>weibosdk</string>
			<string>sinaweibohd</string>
			<string>sinaweibo</string>
			<string>vkauthorize</string>
			<string>fb-messenger</string>
			<string>yixinfav</string>
			<string>yixinoauth</string>
			<string>yixinopenapi</string>
			<string>yixin</string>
			<string>pinit</string>
			<string>kakaolink</string>
			<string>kakao48d3f524e4a636b08d81b3ceb50f1003</string>
			<string>alipay</string>
			<string>storykompassauth</string>
			<string>pinterestsdk.v1</string>
			<string>kakaokompassauth</string>
			<string>alipayshare</string>
			<string>pinit</string>
			<string>line</string>
			<string>whatsapp</string>
			<string>mqqwpa</string>
			<string>instagram</string>
			<string>fbauth2</string>
			<string>renren</string>
			<string>renrenios</string>
			<string>renrenapi</string>
			<string>rm226427com.mob.demoShareSDK</string>
			<string>mqq</string>
			<string>mqqopensdkapiV2</string>
			<string>mqqopensdkapiV3</string>
			<string>wtloginmqq2</string>
			<string>mqqapi</string>
			<string>mqqOpensdkSSoLogin</string>
			<string>sinaweibohdsso</string>
			<string>sinaweibosso</string>
			<string>wechat</string>
			<string>weixin</string>
		</array>";


		//在plist里面增加一行
		plist.AddKey(PlistAdd);
		plist.AddKey (LSAdd);
		plist.Save();
	}


}