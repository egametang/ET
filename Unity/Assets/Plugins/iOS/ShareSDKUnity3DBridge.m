//
//  ShareSDKUnity3DBridge.m
//  Unity-iPhone
//
//  Created by 陈 剑东 on 15/7/28.
//  Copyright (c) 2015年 mob. All rights reserved.
//

#import "ShareSDKUnity3DBridge.h"
#import <ShareSDK/ShareSDK.h>
#import <ShareSDK/ShareSDK+Base.h>
#import <ShareSDKExtension/ShareSDK+Extension.h>
#import <ShareSDKExtension/SSDKFriendsPaging.h>
#import <ShareSDKUI/ShareSDK+SSUI.h>
#import <ShareSDKExtension/SSEShareHelper.h>
#import <ShareSDKConnector/ShareSDKConnector.h>
#import <ShareSDKConfigFile/ShareSDK+XML.h>
#import <MOBFoundation/MOBFJson.h>
#import <MOBFoundation/MOBFRegex.h>
#import <MOBFoundation/MOBFDevice.h>

#define __SHARESDK_WECHAT__
#define __SHARESDK_QQ__
#define __SHARESDK_SINA_WEIBO__
#define __SHARESDK_RENREN__
#define __SHARESDK_KAKAO__
#define __SHARESDK_YIXIN__
#define __SHARESDK_FACEBOOK_MSG__
#define __SHARESDK_ALIPAYSOCIAL__
#define __SHARESDK_DINGTALK__

#ifdef __SHARESDK_WECHAT__
#import "WXApi.h"
#endif

#ifdef __SHARESDK_QQ__
#import <TencentOpenAPI/TencentOAuth.h>
#import <TencentOpenAPI/QQApiInterface.h>
#endif

#ifdef __SHARESDK_SINA_WEIBO__
#import "WeiboSDK.h"
#endif

#ifdef __SHARESDK_RENREN__
#import <RennSDK/RennSDK.h>
#endif

#ifdef __SHARESDK_KAKAO__
#import <KakaoOpenSDK/KakaoOpenSDK.h>
#endif

#ifdef __SHARESDK_YIXIN__
#import "YXApi.h"
#endif

#ifdef __SHARESDK_FACEBOOK_MSG__
#import <FBSDKMessengerShareKit/FBSDKMessengerShareKit.h>
#endif

#ifdef __SHARESDK_ALIPAYSOCIAL__
#import "APOpenAPI.h"
#endif

#ifdef __SHARESDK_DINGTALK__
#import <DTShareKit/DTOpenAPI.h>
#endif

static UIView *_refView = nil;
#if defined (__cplusplus)
extern "C" {
#endif
    
    /**
     *	@brief	配置SDK并初始化
     *
     *	@param 	appKey      ShareSDK的AppKey
     *  @param  configInfo  配置信息
     */
    extern void __iosShareSDKRegisterAppAndSetPltformsConfig (void *appKey, void*configInfo);
    
    /**
     *  用户授权
     *
     *  @param reqID    流水号
     *  @param platType 平台类型
     *  @param observer 观察回调对象名称
     */
    extern void __iosShareSDKAuthorize (int reqID, int platType, void *observer);
    
    /**
     *	@brief	取消用户授权
     *
     *	@param 	platType 	平台类型
     */
    extern void __iosShareSDKCancelAuthorize (int platType);
    
    /**
     *	@brief	判断用户是否授权
     *
     *	@param 	platType 	平台类型
     *
     *	@return	YES 表示已经授权，NO 表示尚未授权
     */
    extern bool __iosShareSDKHasAuthorized (int platType);
    
    /**
     *	@brief	检测是否安装客户端
     *
     *	@param 	platType 	平台类型
     *
     *	@return	YES 表示已经安装，NO 表示尚未安装
     */
    extern bool __iosShareSDKIsClientInstalled (int platType);
    
    /**
     *	@brief	获取用户信息
     *
     *  @param  reqID       流水号
     *	@param 	platType 	平台类型
     *  @param  observer    观察回调对象名称
     */
    extern void __iosShareSDKGetUserInfo (int reqID, int platType, void *observer);
    
    /**
     *	@brief	分享内容
     *
     *  @param  reqID       流水号
     *	@param 	platType 	平台类型
     *	@param 	content 	分享内容
     *  @param  observer    观察回调对象名称
     */
    extern void __iosShareSDKShare (int reqID, int platType, void *content, void *observer);
    
    /**
     *	@brief	一键分享内容
     *
     *  @param  reqID       流水号
     *	@param 	platTypes 	平台类型列表
     *	@param 	content 	分享内容
     *  @param  observer    观察回调对象名称
     */
    extern void __iosShareSDKOneKeyShare (int reqID, void *platTypes, void *content, void *observer);
    
    /**
     *	@brief	显示分享菜单
     *
     *  @param  reqID       流水号
     *	@param 	platTypes 	平台类型列表
     *	@param 	content 	分享内容
     *	@param 	x 	弹出菜单的箭头的横坐标，仅用于iPad
     *	@param 	y 	弹出菜单的箭头的纵坐标，仅用于iPad
     *  @param  observer    观察回调对象名称
     */
    extern void __iosShareSDKShowShareMenu (int reqID, void *platTypes, void *content, int x, int y, void *observer);
    
    /**
     *	@brief	显示分享编辑界面
     *
     *  @param  reqID       流水号
     *	@param 	platType 	平台类型
     *	@param 	content 	分享内容
     *  @param  observer    观察回调对象名称
     */
    extern void __iosShareSDKShowShareView (int reqID, int platType, void *content, void *observer);
    
    /**
     *  @brief 获取授权用户好友列表
     *
     *  @param reqID    流水号
     *  @param platType 平台类型
     *  @param count    单页好友数量
     *  @param page     页码/游标cusor
     *  @param observer 观察回调对象名称
     */
    extern void __iosShareSDKGetFriendsList (int reqID, int platType, int count , int page, void *observer);
    
    /**
     *	@brief	获取授权信息
     *
     *	@param 	platType 	平台类型
     *  @param  observer    观察回调对象名称
     */
    extern const char* __iosShareSDKGetCredential (int platType);
    
    /**
     *	@brief	关注/添加好友
     *
     *	@param 	platType 	平台类型
     *  @param  observer    观察回调对象名称
     */
    extern void __iosShareSDKFollowFriend (int reqID, int platType, void *account, void *observer);
    
    
    /**
     *  @brief  根据配置文件分享
     *
     *  @param reqID            流水号
     *  @param contentName      配置文件节点标识
     *  @param customHashtable  自定义字段表
     *  @param platType         平台类型
     *  @param observer         观察回调对象名称
     */
    extern void __iosShareSDKShareWithContentName(int reqID,
                                                  int platType,
                                                  void *contentName,
                                                  void *customHashtable,
                                                  void *observer);
    
    /**
     *  @brief  根据配置文件展示分享菜单
     *
     *  @param reqID            流水号
     *  @param contentName      配置文件节点标识
     *  @param customHashtable  自定义字段表
     *  @param 	platTypes 	平台类型列表
     *  @param 	x 	弹出菜单的箭头的横坐标，仅用于iPad
     *	@param 	y 	弹出菜单的箭头的纵坐标，仅用于iPad
     *  @param observer         观察回调对象名称
     */
    extern void __iosShareSDKShowShareMenuWithContentName(int reqID,
                                                          void *contentName,
                                                          void *customHashtable,
                                                          void *platTypes,
                                                          int x,
                                                          int y,
                                                          void *observer);
    
    /**
     *  @brief  根据配置文件展示分享编辑界面
     *
     *  @param reqID            流水号
     *  @param contentName      配置文件节点标识
     *  @param customHashtable  自定义字段表
     *  @param platType         平台类型
     *  @param observer         观察回调对象名称
     */
    extern void __iosShareSDKShowShareViewWithContentName(int reqID,
                                                          int platType,
                                                          void *contentName,
                                                          void *customHashtable,
                                                          void *observer);
    
#if defined (__cplusplus)
}
#endif


#if defined (__cplusplus)
extern "C" {
#endif
    
    NSMutableDictionary *__parseWithHashtable (void*configInfo)
    {
        NSString *confCs = [NSString stringWithCString:configInfo encoding:NSUTF8StringEncoding];
        NSMutableDictionary *dic = [NSMutableDictionary dictionaryWithDictionary:[MOBFJson objectFromJSONString:confCs]];
        return dic;
    }
    
    SSDKPlatformType __convertContentType(NSInteger type)
    {
        switch (type)
        {
            case 0:
                return SSDKContentTypeAuto;
            case 1:
                return SSDKContentTypeText;
            case 2:
                return SSDKContentTypeImage;
            case 4:
                return SSDKContentTypeWebPage;
            case 5:
                return SSDKContentTypeAudio;
            case 6:
                return SSDKContentTypeVideo;
            case 7:
                return SSDKContentTypeApp;
            case 8:
                return SSDKContentTypeFile;
            case 9:
                return SSDKContentTypeImage;
            default:
                return SSDKContentTypeText;
        }
    }
    
    void __setWechatParams(NSDictionary *value,NSMutableDictionary *params,SSDKPlatformType subType)
    {
        NSString *text = nil;
        NSString *title = nil;
        NSString *url = nil;
        NSString *thumbImg = nil;
        NSString *image = nil;
        NSString *musicFileUrl = nil;
        NSString *extInfo = nil;
        NSData *fileData = nil;
        NSData *emoData = nil;
        NSString *sourceFileExtension = nil;
        NSData *sourceFileData = nil;
        
        SSDKContentType type = SSDKContentTypeText;
        
        if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
        {
            text = [value objectForKey:@"text"];
        }
        if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
        {
            title = [value objectForKey:@"title"];
        }
        if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
        {
            url = [value objectForKey:@"url"];
        }
        if ([[value objectForKey:@"thumbImageUrl"] isKindOfClass:[NSString class]])
        {
            thumbImg = [value objectForKey:@"thumbImageUrl"];
        }
        if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
        {
            image = [value objectForKey:@"imageUrl"];
        }
        if ([[value objectForKey:@"musicUrl"] isKindOfClass:[NSString class]])
        {
            musicFileUrl = [value objectForKey:@"musicUrl"];
        }
        if ([[value objectForKey:@"extInfoPath"] isKindOfClass:[NSString class]])
        {
            extInfo = [value objectForKey:@"extInfoPath"];
        }
        if ([[value objectForKey:@"filePath"] isKindOfClass:[NSString class]])
        {
            fileData = [NSData dataWithContentsOfFile:[value objectForKey:@"filePath"]];
        }
        if ([[value objectForKey:@"emotionPath"] isKindOfClass:[NSString class]])
        {
            emoData = [NSData dataWithContentsOfFile:[value objectForKey:@"emotionPath"]];
        }
        
        if ([[value objectForKey:@"sourceFileExtension"] isKindOfClass:[NSString class]])
        {
            sourceFileExtension = [value objectForKey:@"sourceFileExtension"];
        }
        if ([[value objectForKey:@"sourceFilePath"] isKindOfClass:[NSString class]])
        {
            sourceFileData = [NSData dataWithContentsOfFile:[value objectForKey:@"sourceFilePath"]];
        }
        
        if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
        {
            type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
        }

        [params SSDKSetupWeChatParamsByText:text
                                      title:title
                                        url:[NSURL URLWithString:url]
                                 thumbImage:thumbImg
                                      image:image
                               musicFileURL:[NSURL URLWithString:musicFileUrl]
                                    extInfo:extInfo
                                   fileData:fileData
                               emoticonData:emoData
                        sourceFileExtension:sourceFileExtension
                             sourceFileData:sourceFileData
                                       type:type
                         forPlatformSubType:subType];
        
    }
    
    void __setQQParams(NSDictionary *value,NSMutableDictionary *params,SSDKPlatformType subType)
    {
        NSString *text = nil;
        NSString *title = nil;
        NSString *url = nil;
        NSString *thumbImg = nil;
        NSString *image = nil;
        SSDKContentType type = SSDKContentTypeText;
        
        if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
        {
            text = [value objectForKey:@"text"];
        }
        if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
        {
            title = [value objectForKey:@"title"];
        }
        if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
        {
            url = [value objectForKey:@"url"];
        }
        if ([[value objectForKey:@"thumbImageUrl"] isKindOfClass:[NSString class]])
        {
            thumbImg = [value objectForKey:@"thumbImageUrl"];
        }
        if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
        {
            image = [value objectForKey:@"imageUrl"];
        }
        if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
        {
            type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
        }
        [params SSDKSetupQQParamsByText:text
                                  title:title
                                    url:[NSURL URLWithString:url]
                             thumbImage:thumbImg
                                  image:image
                                   type:type
                     forPlatformSubType:subType];
        
    }
    
    void __setYixinParams(NSDictionary *value,NSMutableDictionary *params,SSDKPlatformType subType)
    {
        
        NSString *text = nil;
        NSString *title = nil;
        NSString *url = nil;
        NSString *thumbImg = nil;
        NSString *image = nil;
        NSString *musicFileURL = nil;
        NSString *extInfo = nil;
        NSString *fileDataPath = nil;
        NSString *comment = nil;
        NSString *toUserId = nil;
        SSDKContentType type = SSDKContentTypeText;
        
        if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
        {
            text = [value objectForKey:@"text"];
        }
        if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
        {
            title = [value objectForKey:@"title"];
        }
        if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
        {
            url = [value objectForKey:@"url"];
        }
        if ([[value objectForKey:@"thumbImageUrl"] isKindOfClass:[NSString class]])
        {
            thumbImg = [value objectForKey:@"thumbImageUrl"];
        }
        if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
        {
            image = [value objectForKey:@"imageUrl"];
        }
        if ([[value objectForKey:@"musicUrl"] isKindOfClass:[NSString class]])
        {
            musicFileURL = [value objectForKey:@"musicUrl"];
        }
        if ([[value objectForKey:@"extInfoPath"] isKindOfClass:[NSString class]])
        {
            extInfo = [value objectForKey:@"extInfoPath"];
        }
        if ([[value objectForKey:@"filePath"] isKindOfClass:[NSString class]])
        {
            fileDataPath = [value objectForKey:@"filePath"];
        }
        if ([[value objectForKey:@"comment"] isKindOfClass:[NSString class]])
        {
            comment = [value objectForKey:@"comment"];
        }
        if ([[value objectForKey:@"toUserID"] isKindOfClass:[NSString class]])
        {
            toUserId = [value objectForKey:@"toUserID"];
        }
        if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
        {
            type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
        }
        
        [params SSDKSetupYiXinParamsByText:text
                                     title:title
                                       url:[NSURL URLWithString:url]
                                thumbImage:thumbImg
                                     image:image
                              musicFileURL:[NSURL URLWithString:musicFileURL]
                                   extInfo:extInfo
                                  fileData:fileDataPath
                                   comment:comment
                                  toUserId:toUserId
                                      type:type
                        forPlatformSubType:subType];
    }
    
    void __setKakaoParams(NSDictionary *value,NSMutableDictionary *params,SSDKPlatformType subType)
    {
        NSString *text = nil;
        NSMutableArray *images = [NSMutableArray array];
        NSString *title = nil;
        NSString *url = nil;
        NSString *permission = nil;
        BOOL enableShare;
        CGFloat imageWidth;
        CGFloat imageHeight;
        NSString *appButtonTitle = nil;
        
        NSDictionary *androidExecParam = nil;
        NSString *androidMarkParam = nil;
        
        NSDictionary *iphoneExecParams = nil;
        NSString *iphoneMarkParam = nil;
        
        NSDictionary *ipadExecParams = nil;
        NSString *ipadMarkParam = nil;
        
        SSDKContentType type = SSDKContentTypeText;
        
        if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
        {
            text = [value objectForKey:@"text"];
        }
        
        if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
        {
            NSString *imgPath =  [value objectForKey:@"imageUrl"];
            SSDKImage *image = nil;
            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                    options:MOBFRegexOptionsNoOptions
                                    inRange:NSMakeRange(0, imgPath.length)
                                 withString:imgPath])
            {
                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
            }
            else
            {
                UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                image = [[SSDKImage alloc] initWithImage:localImg
                                                  format:SSDKImageFormatJpeg
                                                settings:nil];
            }
            
            if (image)
            {
                [images addObject:imgPath];
            }
            else
            {
                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
            }
            
        }
        else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
        {
            
            NSArray *paths = [value objectForKey:@"imageUrl"];
            for (NSString *path in paths)
            {
                SSDKImage *image = nil;
                if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                        options:MOBFRegexOptionsNoOptions
                                        inRange:NSMakeRange(0, path.length)
                                     withString:path])
                {
                    image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                }
                else
                {
                    UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                    image = [[SSDKImage alloc] initWithImage:localImg
                                                      format:SSDKImageFormatJpeg
                                                    settings:nil];
                }
                
                if (image)
                {
                    [images addObject:path];
                }
                else
                {
                    NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                }
            }
        }
        
        if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
        {
            title = [value objectForKey:@"title"];
        }
        if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
        {
            url = [value objectForKey:@"url"];
        }
        if ([[value objectForKey:@"permission"] isKindOfClass:[NSString class]])
        {
            permission = [value objectForKey:@"permission"];
        }
        
        if ([[value objectForKey:@"enableShare"] boolValue])
        {
            enableShare = YES;
        }
        
        if ([[value objectForKey:@"imageWidth"] isKindOfClass:[NSNumber class]])
        {
            imageWidth = [[value objectForKey:@"imageWidth"] floatValue];
        }
        if ([[value objectForKey:@"imageHeight"] isKindOfClass:[NSNumber class]])
        {
            imageHeight = [[value objectForKey:@"imageHeight"] floatValue];
        }
        if ([[value objectForKey:@"appButtonTitle"] isKindOfClass:[NSString class]])
        {
            appButtonTitle = [value objectForKey:@"appButtonTitle"];
        }
        
        if ([[value objectForKey:@"androidExecParam"] isKindOfClass:[NSDictionary class]])
        {
            androidExecParam = [value objectForKey:@"androidExecParam"];
        }
        if ([[value objectForKey:@"androidMarkParam"] isKindOfClass:[NSString class]])
        {
            androidMarkParam = [value objectForKey:@"androidMarkParam"];
        }
        
        if ([[value objectForKey:@"iphoneExecParam"] isKindOfClass:[NSDictionary class]])
        {
            iphoneExecParams = [value objectForKey:@"iphoneExecParam"];
        }
        if ([[value objectForKey:@"iphoneMarkParam"] isKindOfClass:[NSString class]])
        {
            iphoneMarkParam = [value objectForKey:@"iphoneMarkParam"];
        }
        
        if ([[value objectForKey:@"ipadExecParam"] isKindOfClass:[NSDictionary class]])
        {
            ipadExecParams = [value objectForKey:@"ipadExecParam"];
        }
        if ([[value objectForKey:@"ipadMarkParam"] isKindOfClass:[NSString class]])
        {
            ipadMarkParam = [value objectForKey:@"ipadMarkParam"];
        }
        
        if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
        {
            type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
            
        }
        
        [params SSDKSetupKaKaoParamsByText:text
                                    images:images
                                     title:title
                                       url:[NSURL URLWithString:url]
                                permission:permission
                               enableShare:enableShare
                                 imageSize:CGSizeMake(imageWidth, imageHeight)
                            appButtonTitle:appButtonTitle
                          androidExecParam:androidExecParam
                          androidMarkParam:androidMarkParam
                          iphoneExecParams:iphoneExecParams
                           iphoneMarkParam:iphoneMarkParam
                            ipadExecParams:ipadExecParams
                             ipadMarkParam:ipadMarkParam
                                      type:type
                        forPlatformSubType:subType];

    }
    
    
    NSMutableDictionary *__getShareParamsWithString(NSString *dataStr)
    {
        NSMutableDictionary *params = [NSMutableDictionary dictionary];
        
        NSMutableArray *imageArray = [NSMutableArray array];
        NSString *text = nil;
        NSString *title = nil;
        NSString *url = nil;
        SSDKContentType type = SSDKContentTypeText;
        
        NSDictionary *shareParamsDic = [MOBFJson objectFromJSONString:dataStr];

        if (shareParamsDic)
        {
            
            if ([[shareParamsDic objectForKey:@"text"] isKindOfClass:[NSString class]])
            {
                text = [shareParamsDic objectForKey:@"text"];
            }
            
            id img = [shareParamsDic objectForKey:@"imageUrl"];
            if ([img isKindOfClass:[NSString class]])
            {
                NSString *imgPath = img;
                SSDKImage *image = nil;
                if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                        options:MOBFRegexOptionsNoOptions
                                        inRange:NSMakeRange(0, imgPath.length)
                                     withString:imgPath])
                {
                    image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                }
                else
                {
                    UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                    image = [[SSDKImage alloc] initWithImage:localImg
                                                      format:SSDKImageFormatJpeg
                                                    settings:nil];
                }
                
                if (image)
                {
                   
                    [imageArray addObject:imgPath];
                }
                else
                {
                    NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                }
            }
            else if([img isKindOfClass:[NSArray class]])
            {
                NSArray *paths = [img copy];
                for (NSString *path in paths)
                {
                    
                    SSDKImage *image = nil;
                    
                    if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                            options:MOBFRegexOptionsNoOptions
                                            inRange:NSMakeRange(0, path.length)
                                         withString:path])
                    {
                        image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                    }
                    else
                    {
                        UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                        image = [[SSDKImage alloc] initWithImage:localImg
                                                          format:SSDKImageFormatJpeg
                                                        settings:nil];
                    }
                    
                    if (image)
                    {
                        [imageArray addObject:path];
                    }
                    else
                    {
                        NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                    }
                }
            }
            
            if ([[shareParamsDic objectForKey:@"title"] isKindOfClass:[NSString class]])
            {
                title = [shareParamsDic objectForKey:@"title"];
                
            }
            
            if ([[shareParamsDic objectForKey:@"url"] isKindOfClass:[NSString class]])
            {
                url = [shareParamsDic objectForKey:@"url"];
                
            }
            if ([[shareParamsDic objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
            {
                type = __convertContentType([[shareParamsDic objectForKey:@"shareType"] integerValue]);
            }
            
            if ([[shareParamsDic objectForKey:@"clientShare"] isKindOfClass:[NSNumber class]])
            {
                NSInteger enable = [[shareParamsDic objectForKey:@"clientShare"] integerValue];
                if (enable > 0)
                {
                    [params SSDKEnableUseClientShare];
                }
            }
            
            
            [params SSDKSetupShareParamsByText:text
                                        images:imageArray
                                           url:[NSURL URLWithString:url]
                                         title:title
                                          type:type];
            
            
            NSDictionary *customizeShareParams = [shareParamsDic objectForKey:@"customizeShareParams"];
            
            if (customizeShareParams && [[customizeShareParams allKeys] count] > 0)
            {
                //新浪微博
                id value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeSinaWeibo]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *title = nil;
                    NSString *image = nil;
                    NSString *url = nil;
                    double lat = 0;
                    double lng = 0;
                    NSString *objID = nil;
                    SSDKContentType type = SSDKContentTypeWebPage;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image =  [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"latitude"] isKindOfClass:[NSString class]])
                    {
                        lat = [[value objectForKey:@"latitude"] doubleValue];
                    }
                    if ([[value objectForKey:@"longitude"] isKindOfClass:[NSString class]])
                    {
                        lng = [[value objectForKey:@"longitude"] doubleValue];
                    }
                    if ([[value objectForKey:@"objectID"] isKindOfClass:[NSString class]])
                    {
                        objID = [value objectForKey:@"objectID"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    
                    if ([[value objectForKey:@"clientShare"] isKindOfClass:[NSNumber class]])
                    {
                        NSInteger enable = [[value objectForKey:@"clientShare"] integerValue];
                        if (enable > 0)
                        {
                            [params SSDKEnableUseClientShare];
                        }
                    }
                    
                    [params SSDKSetupSinaWeiboShareParamsByText:text
                                                          title:title
                                                          image:image
                                                            url:[NSURL URLWithString:url]
                                                       latitude:lat
                                                      longitude:lng
                                                       objectID:objID
                                                           type:type];
                    
                }
                //腾讯微博
                value  = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeTencentWeibo]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    double lat;
                    double lng;
                    SSDKContentType type = SSDKContentTypeImage;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc]initWithImage:localImg
                                                             format:SSDKImageFormatJpeg
                                                           settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        
                        for (NSString *path in paths)
                        {
                            
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                        }
                    }
                    
                    if ([[value objectForKey:@"latitude"] isKindOfClass:[NSNumber class]])
                    {
                        lat = [[value objectForKey:@"latitude"] doubleValue];
                    }
                    if ([[value objectForKey:@"longitude"] isKindOfClass:[NSNumber class]])
                    {
                        lng = [[value objectForKey:@"longitude"] doubleValue];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    
                    
                    [params SSDKSetupTencentWeiboShareParamsByText:text
                                                            images:images
                                                          latitude:lat
                                                         longitude:lng
                                                              type:type];
                    
                }
                //豆瓣
                value  = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeDouBan]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *title = nil;
                    NSString *url = nil;
                    NSString *urlDesc = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"urlDesc"] isKindOfClass:[NSString class]])
                    {
                        urlDesc = [value objectForKey:@"urlDesc"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    [params SSDKSetupDouBanParamsByText:text
                                                  image:image
                                                  title:title
                                                    url:[NSURL URLWithString:url]
                                                urlDesc:urlDesc
                                                   type:type];
                    
                }
                //QQ系列
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeQQFriend]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setQQParams(value,params,SSDKPlatformSubTypeQQFriend);
                    
                }
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeQZone]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setQQParams(value,params,SSDKPlatformSubTypeQZone);
                }
                
                //微信系列
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeWechatSession]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setWechatParams(value,params,SSDKPlatformSubTypeWechatSession);
                }
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeWechatTimeline]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setWechatParams(value,params,SSDKPlatformSubTypeWechatTimeline);
                }
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeWechatFav]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setWechatParams(value,params,SSDKPlatformSubTypeWechatFav);
                }
                
                //人人网
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeRenren]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *url = nil;
                    NSString *albumId = nil;
                    SSDKContentType type = SSDKContentTypeImage;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"albumID"] isKindOfClass:[NSString class]])
                    {
                        albumId = [value objectForKey:@"albumID"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    [params SSDKSetupRenRenParamsByText:text
                                                  image:image
                                                    url:[NSURL URLWithString:url]
                                                albumId:albumId
                                                   type:type];
                }
                //开心网
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeKaixin]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    [params SSDKSetupKaiXinParamsByText:text
                                                  image:image
                                                   type:type];
                }
                //Facebook
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeFacebook]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *title = nil;
                    NSString *urlDesc = nil;
                    NSString *attachmentPath = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"urlDesc"] isKindOfClass:[NSString class]])
                    {
                        urlDesc = [value objectForKey:@"urlDesc"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    if ([[value objectForKey:@"attachmentPath"] isKindOfClass:[NSString class]])
                    {
                        attachmentPath = [value objectForKey:@"attachmentPath"];
                    }
                    
                    if ([[value objectForKey:@"clientShare"] isKindOfClass:[NSNumber class]])
                    {
                        NSInteger enable = [[value objectForKey:@"clientShare"] integerValue];
                        if (enable > 0)
                        {
                            [params SSDKEnableUseClientShare];
                        }
                    }
                    
                    [params SSDKSetupFacebookParamsByText:text
                                                    image:image
                                                      url:[NSURL URLWithString:url]
                                                 urlTitle:title
                                                  urlName:urlDesc
                                           attachementUrl:[NSURL URLWithString:attachmentPath]
                                                     type:type];
                    
                }
                //Twitter
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeTwitter]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    double lat;
                    double lng;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc] initWithImage:localImg
                                                              format:SSDKImageFormatJpeg
                                                            settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        
                        for (NSString *path in paths)
                        {
                            
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                            
                        }
                    }
                    
                    if ([[value objectForKey:@"latitude"] isKindOfClass:[NSNumber class]])
                    {
                        lat = [[value objectForKey:@"latitude"] doubleValue];
                    }
                    if ([[value objectForKey:@"longitude"] isKindOfClass:[NSNumber class]])
                    {
                        lng = [[value objectForKey:@"longitude"] doubleValue];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    
                    [params SSDKSetupTwitterParamsByText:text
                                                  images:images
                                                latitude:lat
                                               longitude:lng
                                                    type:type];
                    
                }
                
                //YinXiang
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeYinXiang]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text  = nil;
                    NSString *title = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    NSMutableArray *tags = [NSMutableArray array];
                    NSString *notebook = nil;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc] initWithImage:localImg
                                                              format:SSDKImageFormatJpeg
                                                            settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        for (NSString *path in paths)
                        {
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                            
                        }
                    }
                    if ([[value objectForKey:@"notebook"] isKindOfClass:[NSString class]])
                    {
                        notebook = [value objectForKey:@"notebook"];
                    }
                    id tagValue = [value objectForKey:@"tags"];
                    if ([tagValue isKindOfClass:[NSString class]])
                    {
                        [tags addObject:tagValue];
                    }
                    else if ([tagValue isKindOfClass:[NSArray class]])
                    {
                        tags = [tagValue mutableCopy];
                    }
                    
                    [params SSDKSetupEvernoteParamsByText:text
                                                   images:images
                                                    title:title
                                                 notebook:notebook
                                                     tags:tags
                                             platformType:SSDKPlatformTypeYinXiang];
                }
                
                //GooglePlus
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeGooglePlus]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    
                    NSString *text = nil;
                    NSString *url = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    
                    [params SSDKSetupGooglePlusParamsByText:text
                                                        url:[NSURL URLWithString:url]
                                                       type:type];
                    
                }
                
                //Instagram
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeInstagram]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *image = nil;
                    CGFloat menuX;
                    CGFloat menuY;
                    
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"menuX"] isKindOfClass:[NSString class]])
                    {
                        menuX = [[value objectForKey:@"menuX"] floatValue];
                    }
                    if ([[value objectForKey:@"menuY"] isKindOfClass:[NSString class]])
                    {
                        menuX = [[value objectForKey:@"menuY"] floatValue];
                    }
                    
                    CGPoint point = CGPointMake(menuX, menuY);
                    [params SSDKSetupInstagramByImage:image menuDisplayPoint:point];
                }
                
                //LinkedIn
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeLinkedIn]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *title = nil;
                    NSString *url = nil;
                    NSString *urlDesc = nil;
                    NSString *visibility = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"urlDesc"] isKindOfClass:[NSString class]])
                    {
                        urlDesc = [value objectForKey:@"urlDesc"];
                    }
                    if ([[value objectForKey:@"visibility"] isKindOfClass:[NSString class]])
                    {
                        visibility = [value objectForKey:@"visibility"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    [params SSDKSetupLinkedInParamsByText:text
                                                    image:image
                                                      url:[NSURL URLWithString:url]
                                                    title:title
                                                  urlDesc:urlDesc
                                               visibility:visibility
                                                     type:type];
                }
                //Tumblr
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeTumblr]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *title = nil;
                    NSString *url = nil;
                    NSString *blogName = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"blogName"] isKindOfClass:[NSString class]])
                    {
                        blogName = [value objectForKey:@"blogName"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    [params SSDKSetupTumblrParamsByText:text
                                                  image:image
                                                    url:[NSURL URLWithString:url]
                                                  title:title
                                               blogName:blogName
                                                   type:type];
                }
                
                //Mail
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeMail]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text  = nil;
                    NSString *title = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    NSMutableArray *attachments = [NSMutableArray array];
                    NSMutableArray *recipients = [NSMutableArray array];
                    NSMutableArray *ccRecipients = [NSMutableArray array];
                    NSMutableArray *bccRecipients = [NSMutableArray array];
                    SSDKContentType type = SSDKContentTypeText;
                    
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc] initWithImage:localImg
                                                              format:SSDKImageFormatJpeg
                                                            settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        
                        for (NSString *path in paths)
                        {
                            
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                            
                        }
                    }
                    
                    if ([[value objectForKey:@"attachmentPath"] isKindOfClass:[NSString class]])
                    {
                        NSData *attachementsData = [NSData dataWithContentsOfFile:[value objectForKey:@"attachmentPath"]];
                        [attachments addObject:attachementsData];
                    }
                    else if ([[value objectForKey:@"attachmentPath"] isKindOfClass:[NSArray class]])
                    {
                        NSArray *paths = [value objectForKey:@"attachmentPath"];
                        for (NSString *path in paths)
                        {
                            
                            NSData *attachementsData = [NSData dataWithContentsOfFile:path];
                            [attachments addObject:attachementsData];
                        }
                    }
                    if ([[value objectForKey:@"recipients"] isKindOfClass:[NSString class]])
                    {
                        [recipients addObject:[value objectForKey:@"recipients"]];
                    }
                    else if ([[value objectForKey:@"recipients"] isKindOfClass:[NSArray class]])
                    {
                        NSArray *recipientsArray = [value objectForKey:@"recipients"];
                        for (NSString *recipient in recipientsArray)
                        {
                            [recipients addObject:recipient];
                        }
                    }
                    if ([[value objectForKey:@"ccRecipients"] isKindOfClass:[NSString class]])
                    {
                        [ccRecipients addObject:[value objectForKey:@"ccRecipients"]];
                    }
                    else if ([[value objectForKey:@"ccRecipients"] isKindOfClass:[NSArray class]])
                    {
                        NSArray *recipientsArray = [value objectForKey:@"ccRecipients"];
                        for (NSString *recipient in recipientsArray)
                        {
                            [ccRecipients addObject:recipient];
                        }
                    }
                    if ([[value objectForKey:@"bccRecipients"] isKindOfClass:[NSString class]])
                    {
                        [bccRecipients addObject:[value objectForKey:@"bccRecipients"]];
                    }
                    else if ([[value objectForKey:@"bccRecipients"] isKindOfClass:[NSArray class]])
                    {
                        NSArray *recipientsArray = [value objectForKey:@"bccRecipients"];
                        for (NSString *recipient in recipientsArray)
                        {
                            [bccRecipients addObject:recipient];
                        }
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    
                    [params SSDKSetupMailParamsByText:text
                                                title:title
                                               images:images
                                          attachments:attachments
                                           recipients:recipients
                                         ccRecipients:ccRecipients
                                        bccRecipients:bccRecipients
                                                 type:type];
                }
                
                //SMS
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeSMS]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text  = nil;
                    NSString *title = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    NSMutableArray *attachments = [NSMutableArray array];
                    NSMutableArray *recipients = [NSMutableArray array];
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc] initWithImage:localImg
                                                              format:SSDKImageFormatJpeg
                                                            settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        
                        for (NSString *path in paths)
                        {
                            
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                            
                        }
                    }
                    
                    if ([[value objectForKey:@"attachmentPath"] isKindOfClass:[NSString class]])
                    {
                        NSData *attachementsData = [NSData dataWithContentsOfFile:[value objectForKey:@"attachmentPath"]];
                        [attachments addObject:attachementsData];
                    }
                    else if ([[value objectForKey:@"attachmentPath"] isKindOfClass:[NSArray class]])
                    {
                        NSArray *paths = [value objectForKey:@"attachmentPath"];
                        for (NSString *path in paths)
                        {
                            
                            NSData *attachementsData = [NSData dataWithContentsOfFile:path];
                            [attachments addObject:attachementsData];
                        }
                    }
                    if ([[value objectForKey:@"recipients"] isKindOfClass:[NSString class]])
                    {
                        [recipients addObject:[value objectForKey:@"recipients"]];
                    }
                    else if ([[value objectForKey:@"recipients"] isKindOfClass:[NSArray class]])
                    {
                        NSArray *recipientsArray = [value objectForKey:@"recipients"];
                        for (NSString *recipient in recipientsArray)
                        {
                            [recipients addObject:recipient];
                        }
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    
                    [params SSDKSetupSMSParamsByText:text
                                               title:title
                                              images:images
                                         attachments:attachments
                                          recipients:recipients
                                                type:type];
                }
                
                //Print
                //无定制内容方法
                
                //Copy
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeCopy]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    NSString *url = nil;
                    SSDKContentType type = SSDKContentTypeImage;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc] initWithImage:localImg
                                                              format:SSDKImageFormatJpeg
                                                            settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        
                        for (NSString *path in paths)
                        {
                            
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                            
                        }
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    [params SSDKSetupCopyParamsByText:text
                                               images:images
                                                  url:[NSURL URLWithString:url]
                                                 type:type];
                }
                
                //Instapaper
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeInstapaper]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *url = nil;
                    NSString *title = nil;
                    NSString *desc = nil;
                    NSString *content = nil;
                    BOOL isPrivateFromSource;
                    NSInteger folderId;
                    BOOL resolveFinalUrl;
                    
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"desc"] isKindOfClass:[NSString class]])
                    {
                        desc = [value objectForKey:@"desc"];
                    }
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        content = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"isPrivateFromSource"] boolValue])
                    {
                        isPrivateFromSource = YES;
                    }
                    if (![[value objectForKey:@"resolveFinalUrl"] boolValue])
                    {
                        resolveFinalUrl = YES;
                    }
                    
                    folderId = [[value objectForKey:@"folderId"] integerValue];
                    
                    [params SSDKSetupInstapaperParamsByUrl:[NSURL URLWithString:url]
                                                     title:title
                                                      desc:desc
                                                   content:content
                                       isPrivateFromSource:isPrivateFromSource
                                                  folderId:folderId
                                           resolveFinalUrl:resolveFinalUrl];
                }
                
                //Pocket
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypePocket]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *url = nil;
                    NSString *title = nil;
                    NSString *tags = nil;
                    NSString *tweetId = nil;
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    id tagValue = [value objectForKey:@"tags"];
                    if ([tagValue isKindOfClass:[NSString class]])
                    {
                        tags = tagValue;
                    }
                    else if ([tagValue isKindOfClass:[NSArray class]])
                    {
                        NSArray *tagsArr = tagValue;
                        tags = [tagsArr componentsJoinedByString:@","];
                    }
                    if ([[value objectForKey:@"tweetID"] isKindOfClass:[NSString class]])
                    {
                        tweetId = [value objectForKey:@"tweetID"];
                    }
                    [params SSDKSetupPocketParamsByUrl:[NSURL URLWithString:url]
                                                 title:title
                                                  tags:tags
                                               tweetId:tweetId];
                }
                
                //YouDaoNote
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeYouDaoNote]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text  = nil;
                    NSString *title = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    NSString *source = nil;
                    NSString *author = nil;
                    NSString *notebook = nil;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc] initWithImage:localImg
                                                              format:SSDKImageFormatJpeg
                                                            settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        
                        for (NSString *path in paths)
                        {
                            
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                            
                        }
                    }
                    if ([[value objectForKey:@"source"] isKindOfClass:[NSString class]])
                    {
                        source = [value objectForKey:@"source"];
                    }
                    if ([[value objectForKey:@"author"] isKindOfClass:[NSString class]])
                    {
                        author = [value objectForKey:@"author"];
                    }
                    if ([[value objectForKey:@"notebook"] isKindOfClass:[NSString class]])
                    {
                        notebook = [value objectForKey:@"notebook"];
                    }
                    
                    [params SSDKSetupYouDaoNoteParamsByText:text
                                                     images:images
                                                      title:title
                                                     source:source
                                                     author:author
                                                   notebook:notebook];
                }
                
                //Pinterest
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypePinterest]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *image = nil;
                    NSString *desc = nil;
                    NSString *url = nil;
                    NSString *board = nil;
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"desc"] isKindOfClass:[NSString class]])
                    {
                        desc = [value objectForKey:@"desc"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"board"] isKindOfClass:[NSString class]])
                    {
                        board = [value objectForKey:@"board"];
                    }

                    [params SSDKSetupPinterestParamsByImage:image
                                                       desc:desc
                                                        url:[NSURL URLWithString:url]
                                                  boardName:board];
                }
                
                //Flickr
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeFlickr]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *title = nil;
                    NSMutableArray *tags = [NSMutableArray array];
                    BOOL isPublic;
                    BOOL isFriend;
                    BOOL isFamiliy;
                    NSInteger safetyLevel;
                    NSInteger contentType;
                    NSInteger hidden;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    id tagValue = [value objectForKey:@"tags"];
                    if ([tagValue isKindOfClass:[NSString class]])
                    {
                        [tags addObject:tagValue];
                    }
                    else if ([tagValue isKindOfClass:[NSArray class]])
                    {
                        tags = [tagValue mutableCopy];
                    }
                    if ([[value objectForKey:@"isPublic"] boolValue])
                    {
                        isPublic = YES;
                    }
                    if ([[value objectForKey:@"isFriend"] boolValue])
                    {
                        isFriend = YES;
                    }
                    if ([[value objectForKey:@"isFamiliy"] boolValue])
                    {
                        isFamiliy = YES;
                    }
                    if ([[value objectForKey:@"safetyLevel"] integerValue])
                    {
                        safetyLevel = [[value objectForKey:@"safetyLevel"] integerValue];
                    }
                    if ([[value objectForKey:@"contentType"] integerValue])
                    {
                        contentType = [[value objectForKey:@"contentType"] integerValue];
                    }
                    if ([[value objectForKey:@"hidden"] integerValue])
                    {
                        hidden = [[value objectForKey:@"hidden"] integerValue];
                    }
                    
                    [params SSDKSetupFlickrParamsByText:text
                                                  image:image
                                                  title:title
                                                   tags:tags
                                               isPublic:isPublic
                                               isFriend:isFriend
                                               isFamily:isFamiliy
                                            safetyLevel:safetyLevel
                                            contentType:contentType
                                                 hidden:hidden];
                }
                
                //Dropbox
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeDropbox]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *attachmentPath = nil;
                    if ([[value objectForKey:@"attachmentPath"] isKindOfClass:[NSString class]])
                    {
                        attachmentPath = [value objectForKey:@"attachmentPath"];
                    }
                    [params SSDKSetupDropboxParamsByAttachment:attachmentPath];
                }
                
                //VKontakte
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeVKontakte]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *url = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    NSString *groupId = nil;
                    BOOL friendsOnly;
                    double lat;
                    double lng;
                    SSDKContentType type = SSDKContentTypeText;
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc] initWithImage:localImg
                                                              format:SSDKImageFormatJpeg
                                                            settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        for (NSString *path in paths)
                        {
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                            
                        }
                    }
                    if ([[value objectForKey:@"groupID"] isKindOfClass:[NSString class]])
                    {
                        groupId = [value objectForKey:@"groupID"];
                    }
                    if ([[value objectForKey:@"friendsOnly"] boolValue])
                    {
                        friendsOnly = YES;
                    }
                    if ([[value objectForKey:@"latitude"] isKindOfClass:[NSString class]])
                    {
                        lat = [[value objectForKey:@"latitude"] doubleValue];
                    }
                    if ([[value objectForKey:@"longitude"] isKindOfClass:[NSString class]])
                    {
                        lng = [[value objectForKey:@"longitude"] doubleValue];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                        
                    }
                    
                    [params SSDKSetupVKontakteParamsByText:text
                                                    images:images
                                                       url:[NSURL URLWithString:url]
                                                   groupId:groupId
                                               friendsOnly:friendsOnly
                                                  latitude:lat
                                                 longitude:lng
                                                      type:type];
                }
                
                //Yixin系列
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeYiXinSession]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setYixinParams(value, params, SSDKPlatformSubTypeYiXinSession);
                }
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeYiXinTimeline]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setYixinParams(value, params, SSDKPlatformSubTypeYiXinTimeline);
                }
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeYiXinFav]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setYixinParams(value, params, SSDKPlatformSubTypeYiXinFav);
                }
                
                //MingDao
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeMingDao]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *url = nil;
                    NSString *title = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                        
                    }
                    
                    [params SSDKSetupMingDaoParamsByText:text
                                                   image:image
                                                     url:[NSURL URLWithString:url]
                                                   title:title
                                                    type:type];
                }
                
                //Line
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeLine]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                        
                    }
                    [params SSDKSetupLineParamsByText:text
                                                image:image
                                                 type:type];
                }
                
                //whatsApp
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeWhatsApp]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *audioPath = nil;
                    NSString *videoPath = nil;
                    CGFloat menuX;
                    CGFloat menuY;
                    SSDKContentType type = SSDKContentTypeText;
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"audioPath"] isKindOfClass:[NSString class]])
                    {
                        audioPath = [value objectForKey:@"audioPath"];
                    }
                    if ([[value objectForKey:@"videoPath"] isKindOfClass:[NSString class]])
                    {
                        videoPath = [value objectForKey:@"videoPath"];
                    }
                    if ([[value objectForKey:@"menuX"] isKindOfClass:[NSString class]])
                    {
                        menuX = [[value objectForKey:@"menuX"] floatValue];
                    }
                    if ([[value objectForKey:@"menuY"] isKindOfClass:[NSString class]])
                    {
                        menuX = [[value objectForKey:@"menuY"] floatValue];
                    }
                    CGPoint point = CGPointMake(menuX, menuY);
                    
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                        
                    }
                    
                    [params SSDKSetupWhatsAppParamsByText:text
                                                    image:image
                                                    audio:audioPath
                                                    video:videoPath
                                         menuDisplayPoint:point
                                                     type:type];
                    
                }
                
                
                //Kakao系列
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeKakaoTalk]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setKakaoParams(value,params,SSDKPlatformSubTypeKakaoTalk);
                }
                
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformSubTypeKakaoStory]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    __setKakaoParams(value,params,SSDKPlatformSubTypeKakaoStory);
                }
                
                
                //支付宝好友
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeAliPaySocial]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *title = nil;
                    NSString *url = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }
                    
                    [params SSDKSetupAliPaySocialParamsByText:text
                                                        image:image
                                                        title:title
                                                          url:[NSURL URLWithString:url]
                                                         type:type];
                }
                
                //钉钉
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeDingTalk]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text = nil;
                    NSString *image = nil;
                    NSString *title = nil;
                    NSString *url = nil;
                    SSDKContentType type = SSDKContentTypeText;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        image = [value objectForKey:@"imageUrl"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"url"] isKindOfClass:[NSString class]])
                    {
                        url = [value objectForKey:@"url"];
                    }
                    if ([[value objectForKey:@"shareType"] isKindOfClass:[NSNumber class]])
                    {
                        type = __convertContentType([[value objectForKey:@"shareType"] integerValue]);
                    }

                    [params SSDKSetupDingTalkParamsByText:text
                                                    image:image
                                                    title:title
                                                      url:[NSURL URLWithString:url]
                                                     type:type];
                    
                }
                
                //Evernote
                value = [MOBFJson objectFromJSONString:[customizeShareParams objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeEvernote]]];
                if ([value isKindOfClass:[NSDictionary class]])
                {
                    NSString *text  = nil;
                    NSString *title = nil;
                    NSMutableArray *images = [NSMutableArray array];
                    NSMutableArray *tags = [NSMutableArray array];
                    NSString *notebook = nil;
                    
                    if ([[value objectForKey:@"text"] isKindOfClass:[NSString class]])
                    {
                        text = [value objectForKey:@"text"];
                    }
                    if ([[value objectForKey:@"title"] isKindOfClass:[NSString class]])
                    {
                        title = [value objectForKey:@"title"];
                    }
                    if ([[value objectForKey:@"imageUrl"] isKindOfClass:[NSString class]])
                    {
                        NSString *imgPath =  [value objectForKey:@"imageUrl"];
                        SSDKImage *image = nil;
                        if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                options:MOBFRegexOptionsNoOptions
                                                inRange:NSMakeRange(0, imgPath.length)
                                             withString:imgPath])
                        {
                            image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:imgPath]];
                        }
                        else
                        {
                            UIImage *localImg = [UIImage imageWithContentsOfFile:imgPath];
                            image = [[SSDKImage alloc] initWithImage:localImg
                                                              format:SSDKImageFormatJpeg
                                                            settings:nil];
                        }
                        
                        if (image)
                        {
                            [images addObject:imgPath];
                        }
                        else
                        {
                            NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                        }
                        
                    }
                    else if([[value objectForKey:@"imageUrl"] isKindOfClass:[NSArray class]])
                    {
                        
                        NSArray *paths = [value objectForKey:@"imageUrl"];
                        
                        for (NSString *path in paths)
                        {
                            
                            SSDKImage *image = nil;
                            if ([MOBFRegex isMatchedByRegex:@"\\w://.*"
                                                    options:MOBFRegexOptionsNoOptions
                                                    inRange:NSMakeRange(0, path.length)
                                                 withString:path])
                            {
                                image = [[SSDKImage alloc]initWithURL:[NSURL URLWithString:path]];
                            }
                            else
                            {
                                UIImage *localImg = [UIImage imageWithContentsOfFile:path];
                                image = [[SSDKImage alloc] initWithImage:localImg
                                                                  format:SSDKImageFormatJpeg
                                                                settings:nil];
                            }
                            
                            if (image)
                            {
                                [images addObject:path];
                            }
                            else
                            {
                                NSLog(@"#waring : 检测不到有效图片路径,请检查传入图片的路径的有效性");
                            }
                            
                        }
                    }
                    if ([[value objectForKey:@"notebook"] isKindOfClass:[NSString class]])
                    {
                        notebook = [value objectForKey:@"notebook"];
                    }
                    id tagValue = [value objectForKey:@"tags"];
                    if ([tagValue isKindOfClass:[NSString class]])
                    {
                        [tags addObject:tagValue];
                    }
                    else if ([tagValue isKindOfClass:[NSArray class]])
                    {
                        tags = [tagValue mutableCopy];
                    }
                    [params SSDKSetupEvernoteParamsByText:text
                                                   images:images
                                                    title:title
                                                 notebook:notebook
                                                     tags:tags
                                             platformType:SSDKPlatformTypeEvernote];
                }
                
            }
        }
        return params;
    }
    
    void __iosShareSDKRegisterAppAndSetPltformsConfig (void *appKey, void*configInfo)
    {
        NSMutableArray *activePlatforms = [NSMutableArray array];
        NSMutableDictionary *platformsDict = [NSMutableDictionary dictionary];
        NSString *appKeyStr = [NSString stringWithCString:appKey encoding:NSUTF8StringEncoding];
        
        if (configInfo)
        {
            platformsDict = __parseWithHashtable(configInfo);
        }
        for (id obj in [platformsDict allKeys])
        {
            NSInteger platformInterger = [obj integerValue];
            NSDictionary *dict = [platformsDict objectForKey:[NSString stringWithFormat:@"%@",obj]];
            
            if ([[dict objectForKey:@"Enable"] isEqualToString:@"true"])
            {
                [activePlatforms addObject:[NSNumber numberWithInteger:platformInterger]];
            }
            
            
        }
        [ShareSDK registerApp:appKeyStr
              activePlatforms:activePlatforms
                     onImport:^(SSDKPlatformType platformType) {
                         switch (platformType)
                         {
                                 
                             case SSDKPlatformTypeSinaWeibo:
#ifdef __SHARESDK_SINA_WEIBO__
                                 [ShareSDKConnector connectWeibo:[WeiboSDK class]];
#endif
                                 break;
                                 
                             case SSDKPlatformTypeQQ:
#ifdef __SHARESDK_QQ__
                                 [ShareSDKConnector connectQQ:[QQApiInterface class]
                                            tencentOAuthClass:[TencentOAuth class]];
#endif
                                 break;
                                 
                             case SSDKPlatformTypeWechat:
#ifdef __SHARESDK_WECHAT__
                                 [ShareSDKConnector connectWeChat:[WXApi class]];
#endif
                                 break;
                             case SSDKPlatformTypeRenren:
#ifdef __SHARESDK_RENREN__
                                 [ShareSDKConnector connectRenren:[RennClient class]];
#endif
                                 break;
                             case SSDKPlatformTypeKakao:
#ifdef __SHARESDK_KAKAO__
                                 [ShareSDKConnector connectKaKao:[KOSession class]];
#endif
                                 break;
                             case SSDKPlatformTypeYiXin:
#ifdef __SHARESDK_YIXIN__
                                 [ShareSDKConnector connectYiXin:[YXApi class]];
#endif
                                 break;
                             case SSDKPlatformTypeFacebookMessenger:
#ifdef __SHARESDK_FACEBOOK_MSG__
                                 [ShareSDKConnector connectFacebookMessenger:[FBSDKMessengerSharer class]];
#endif
                                 break;
                             case SSDKPlatformTypeAliPaySocial:
#ifdef __SHARESDK_ALIPAYSOCIAL__
                                 [ShareSDKConnector connectAliPaySocial:[APOpenAPI class]];
#endif
                                 break;
                             case SSDKPlatformTypeDingTalk:
#ifdef __SHARESDK_DINGTALK__
                                 [ShareSDKConnector connectDingTalk:[DTOpenAPI class]];
#endif
                                 break;
                             default:
                                 break;
                         }
                     } onConfiguration:^(SSDKPlatformType platformType, NSMutableDictionary *appInfo) {
                         
                         switch (platformType)
                         {
                             case SSDKPlatformTypeWechat:
                             {
                                 
                                 NSArray *weChatTypes = @[@(SSDKPlatformTypeWechat),
                                                          @(SSDKPlatformSubTypeWechatSession),
                                                          @(SSDKPlatformSubTypeWechatTimeline),
                                                          @(SSDKPlatformSubTypeWechatFav)];
                                 
                                 [weChatTypes enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
                                     
                                     NSDictionary *wechatDict = [platformsDict objectForKey:[NSString stringWithFormat:@"%@",obj]];
                                     
                                     if (wechatDict && [[wechatDict allKeys] count] > 0)
                                     {
                                         [appInfo SSDKSetupWeChatByAppId:[wechatDict objectForKey:@"app_id"]
                                                               appSecret:[wechatDict objectForKey:@"app_secret"]];
                                         *stop = YES;
                                     }
                                     
                                 }];
                                 break;
                             }
                             case SSDKPlatformTypeQQ:
                             {
                                 NSArray *QQTypes = @[@(SSDKPlatformTypeQQ),
                                                      @(SSDKPlatformSubTypeQQFriend),
                                                      @(SSDKPlatformSubTypeQZone)];
                                 [QQTypes enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
                                     
                                     NSDictionary *QQDict = [platformsDict objectForKey:[NSString stringWithFormat:@"%@",obj]];
                                     
                                     if (QQDict && [[QQDict allKeys] count] > 0)
                                     {
                                         [appInfo SSDKSetupQQByAppId:[QQDict objectForKey:@"app_id"]
                                                              appKey:[QQDict objectForKey:@"app_key"]
                                                            authType:[QQDict objectForKey:@"auth_type"]];
                                         *stop = YES;
                                     }
                                 }];
                                 break;
                             }
                             case SSDKPlatformTypeKakao:
                             {
                                 NSArray *KakaoTypes = @[@(SSDKPlatformTypeKakao),
                                                         @(SSDKPlatformSubTypeKakaoTalk),
                                                         @(SSDKPlatformSubTypeKakaoStory)];
                                 
                                 [KakaoTypes enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
                                     
                                     NSDictionary *KakaoDict = [platformsDict objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)obj]];
                                     
                                     if (KakaoDict && [[KakaoDict allKeys] count] > 0)
                                     {
                                         [appInfo SSDKSetupKaKaoByAppKey:[KakaoDict objectForKey:@"app_key"]
                                                              restApiKey:[KakaoDict objectForKey:@"rest_api_key"]
                                                             redirectUri:[KakaoDict objectForKey:@"redirect_uri"]
                                                                authType:[KakaoDict objectForKey:@"auth_type"]];
                                         
                                         *stop = YES;
                                     }
                                 }];
                                 
                                 break;
                             }
                             case SSDKPlatformTypeYiXin:
                             {
                                 NSArray *yiXinTypes = @[@(SSDKPlatformTypeYiXin),
                                                         @(SSDKPlatformSubTypeYiXinSession),
                                                         @(SSDKPlatformSubTypeYiXinTimeline),
                                                         @(SSDKPlatformSubTypeYiXinFav)];
                                 
                                 [yiXinTypes enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
                                     
                                     NSDictionary *yixinDict = [platformsDict objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)obj]];
                                     
                                     if (yixinDict && [[yixinDict allKeys] count] > 0)
                                     {
                                         [appInfo SSDKSetupYiXinByAppId:[yixinDict objectForKey:@"app_id"]
                                                              appSecret:[yixinDict objectForKey:@"app_secret"]
                                                            redirectUri:[yixinDict objectForKey:@"redirect_uri"]
                                                               authType:[yixinDict objectForKey:@"auth_type"]];
                                         
                                         *stop = YES;
                                     }
                                 }];
                                 break;
                             }
                             case SSDKPlatformTypeAliPaySocial:
                             {
                                 NSDictionary *platformDict = nil;
                                 NSDictionary *dictFromAliPaySocial = [platformsDict objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeAliPaySocial]];
                                 NSDictionary *dictFromAliPaySocialTimeline = [platformsDict objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)SSDKPlatformTypeAliPaySocialTimeline]];
                                 if (dictFromAliPaySocial)
                                 {
                                     platformDict = dictFromAliPaySocial;
                                 }
                                 else
                                 {
                                     platformDict = dictFromAliPaySocialTimeline;
                                 }
                                 [appInfo addEntriesFromDictionary:platformDict];
                                 break;
                             }
                             default:
                             {
                                 NSDictionary *platformDict = [platformsDict objectForKey:[NSString stringWithFormat:@"%lu",(unsigned long)platformType]];
                                 [appInfo addEntriesFromDictionary:platformDict];
                                 
                                 break;
                             }
                         }
                         
                     }];
    }
    
    
    void __iosShareSDKAuthorize (int reqID, int platType, void *observer)
    {
        
        NSString *observerStr = nil;
        if (observer)
        {
            observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        }
        
        [ShareSDK authorize:platType
                   settings:nil
             onStateChanged:^(SSDKResponseState state, SSDKUser *user, NSError *error) {
                 NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
                 [resultDict setObject:[NSNumber numberWithInteger:1] forKey:@"action"];
                 [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
                 [resultDict setObject:[NSNumber numberWithInteger:platType] forKey:@"platform"];
                 [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
                 
                 if (state == SSDKResponseStateFail && error)
                 {
                     NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                     [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                     if ([[error userInfo] objectForKey:@"error_message"])
                     {
                         if ([[error userInfo] objectForKey:@"error_message"])
                         {
                             [errorDict setObject:[[error userInfo] objectForKey:@"error_message"] forKey:@"error_msg"];
                             
                         }
                     }
                     else if ([[error userInfo] objectForKey:@"user_data"])
                     {
                         NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                         if ([error_data objectForKey:@"error"])
                         {
                             [errorDict setObject:[error_data objectForKey:@"error"] forKey:@"error_msg"];
                         }
                         if ([error_data objectForKey:@"error_code"])
                         {
                             [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]] forKey:@"error_code"];
                         }
                     }
                     
                     [resultDict setObject:errorDict forKey:@"res"];
                     
                 }
                 if (state == SSDKResponseStateSuccess)
                 {
                     if ([user rawData])
                     {
                         [resultDict setObject:[[user credential] rawData] forKey:@"res"];
                     }
                 }
                 
                 NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
                 UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
                 
             }];
    }
    
    void __iosShareSDKCancelAuthorize (int platType)
    {
        [ShareSDK cancelAuthorize:platType];
    }
    
    bool __iosShareSDKHasAuthorized (int platType)
    {
        return [ShareSDK hasAuthorized:platType];
    }
    
    bool __iosShareSDKIsClientInstalled(int platType)
    {
        return [ShareSDK isClientInstalled:platType];
    }
    
    void __iosShareSDKGetUserInfo (int reqID, int platType, void *observer)
    {
        NSString *observerStr = nil;
        if (observer)
        {
            observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        }
        
        [ShareSDK getUserInfo:platType
               onStateChanged:^(SSDKResponseState state, SSDKUser *user, NSError *error)
         {
             
             NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
             [resultDict setObject:[NSNumber numberWithInteger:8] forKey:@"action"];
             [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
             [resultDict setObject:[NSNumber numberWithInteger:platType] forKey:@"platform"];
             [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
             
             if (state == SSDKResponseStateFail && error)
             {
                 NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                 [errorDict setObject:[NSNumber numberWithInteger:[error code]]
                               forKey:@"error_code"];
                 
                 if ([[error userInfo] objectForKey:@"error_message"])
                 {
                     if ([[error userInfo] objectForKey:@"error_message"])
                     {
                         [errorDict setObject:[[error userInfo] objectForKey:@"error_message"]
                                       forKey:@"error_msg"];
                         
                     }
                 }
                 else if ([[error userInfo] objectForKey:@"user_data"])
                 {
                     NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                     if ([error_data objectForKey:@"error"])
                     {
                         [errorDict setObject:[error_data objectForKey:@"error"]
                                       forKey:@"error_msg"];
                     }
                     if ([error_data objectForKey:@"error_code"])
                     {
                         [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]]
                                       forKey:@"error_code"];
                     }
                 }
                 
                 
                 [resultDict setObject:errorDict forKey:@"res"];
             }
             if (state == SSDKResponseStateSuccess && user)
             {
                 [resultDict setObject:[user rawData] forKey:@"res"];
             }
             
             NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
             UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
             
         }];
    }
    
    void __iosShareSDKShare (int reqID, int platType, void *content, void *observer)
    {
        NSString *observerStr = nil;
        NSMutableDictionary *shareParams = [NSMutableDictionary dictionary];
        observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        
        if (content)
        {
            NSString *contentStr = [NSString stringWithCString:content encoding:NSUTF8StringEncoding];
            shareParams = __getShareParamsWithString(contentStr);
        }
        
        [ShareSDK share:platType
             parameters:shareParams
         onStateChanged:^(SSDKResponseState state, NSDictionary *userData, SSDKContentEntity *contentEntity, NSError *error) {
             
             NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
             [resultDict setObject:[NSNumber numberWithInteger:9] forKey:@"action"];
             [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
             [resultDict setObject:[NSNumber numberWithInteger:platType] forKey:@"platform"];
             [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
             
             if (state == SSDKResponseStateFail && error)
             {
                 NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                 [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                 if ([[error userInfo] objectForKey:@"error_message"])
                 {
                     if ([[error userInfo] objectForKey:@"error_message"])
                     {
                         [errorDict setObject:[[error userInfo] objectForKey:@"error_message"]
                                       forKey:@"error_msg"];
                         
                     }
                 }
                 else if ([[error userInfo] objectForKey:@"user_data"])
                 {
                     NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                     if ([error_data objectForKey:@"error"])
                     {
                         [errorDict setObject:[error_data objectForKey:@"error"] forKey:@"error_msg"];
                     }
                     if ([error_data objectForKey:@"error_code"])
                     {
                         [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]]
                                       forKey:@"error_code"];
                     }
                 }
                 
                 [resultDict setObject:errorDict forKey:@"res"];
                 
             }
             
             if (state == SSDKResponseStateSuccess)
             {
                 if ([contentEntity rawData])
                 {
                     [resultDict setObject:[contentEntity rawData]  forKey:@"res"];
                 }
             }
             NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
             
             UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
             
         }];
        
    }
    
    void __iosShareSDKOneKeyShare (int reqID, void *platTypes, void *content, void *observer)
    {
        NSArray *platTypesArr = nil;
        NSString *observerStr = nil;
        NSMutableDictionary *shareParams = [NSMutableDictionary dictionary];
        
        observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        
        if (platTypes)
        {
            NSString *platTypesStr = [NSString stringWithCString:platTypes encoding:NSUTF8StringEncoding];
            platTypesArr = [MOBFJson objectFromJSONString:platTypesStr];
        }
        
        if (content)
        {
            NSString *contentStr = [NSString stringWithCString:content encoding:NSUTF8StringEncoding];
            shareParams = __getShareParamsWithString(contentStr);
        }
        
        [SSEShareHelper oneKeyShare:platTypesArr
                         parameters:shareParams
                     onStateChanged:^(SSDKPlatformType platformType, SSDKResponseState state, NSDictionary *userData, SSDKContentEntity *contentEntity, NSError *error, BOOL end) {
                         
                         NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
                         [resultDict setObject:[NSNumber numberWithInteger:9] forKey:@"action"];
                         [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
                         [resultDict setObject:[NSNumber numberWithInteger:platformType] forKey:@"platform"];
                         [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
                         
                         if (state == SSDKResponseStateFail && error)
                         {
                             NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                             [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                             
                             
                             
                             
                             if ([[error userInfo] objectForKey:@"error_message"])
                             {
                                 if ([[error userInfo] objectForKey:@"error_message"])
                                 {
                                     [errorDict setObject:[[error userInfo] objectForKey:@"error_message"] forKey:@"error_msg"];
                                     
                                 }
                             }
                             else if ([[error userInfo] objectForKey:@"user_data"])
                             {
                                 NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                                 
                                 if ([error_data objectForKey:@"error"])
                                 {
                                     [errorDict setObject:[error_data objectForKey:@"error"] forKey:@"error_msg"];
                                 }
                                 
                                 if ([error_data objectForKey:@"error_code"])
                                 {
                                     [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]]
                                                   forKey:@"error_code"];
                                 }
                             }
                             
                             [resultDict setObject:errorDict forKey:@"res"];
                         }
                         
                         if (state == SSDKResponseStateSuccess)
                         {
                             if ([contentEntity rawData])
                             {
                                 [resultDict setObject:[contentEntity rawData] forKey:@"res"];
                             }
                         }
                         
                         NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
                         UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
                         
                     }];
    }
    
    void __iosShareSDKShowShareMenu (int reqID, void *platTypes, void *content, int x, int y, void *observer)
    {
       
        NSArray *platTypesArr = nil;
        NSMutableArray *actionSheetItems = [NSMutableArray array];
        NSString *observerStr = nil;
        NSMutableDictionary *shareParams = [NSMutableDictionary dictionary];
        
        observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        
        if (platTypes)
        {
            NSString *platTypesStr = [NSString stringWithCString:platTypes encoding:NSUTF8StringEncoding];
            platTypesArr = [MOBFJson objectFromJSONString:platTypesStr];
        }
        else
        {
            platTypesArr = [ShareSDK activePlatforms];
            for (id obj in platTypesArr)
            {
                NSInteger platformInterger = [obj integerValue];
                [actionSheetItems addObject:[NSNumber numberWithInteger:platformInterger]];
            }
            platTypesArr = [actionSheetItems mutableCopy];
            
        }
        
        if (content)
        {
            NSString *contentStr = [NSString stringWithCString:content encoding:NSUTF8StringEncoding];
            shareParams = __getShareParamsWithString(contentStr);
        }
        
        if ([MOBFDevice isPad])
        {
            if (!_refView)
            {
                _refView = [[UIView alloc] initWithFrame:CGRectMake(x, y, 10, 10)];
            }
            
            [[UIApplication sharedApplication].keyWindow.rootViewController.view addSubview:_refView];
            
        }
        
        [ShareSDK showShareActionSheet:_refView
                                 items:platTypesArr
                           shareParams:shareParams
                   onShareStateChanged:^(SSDKResponseState state, SSDKPlatformType platformType, NSDictionary *userData, SSDKContentEntity *contentEntity, NSError *error, BOOL end) {
                       
                       NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
                       [resultDict setObject:[NSNumber numberWithInteger:9] forKey:@"action"];
                       [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
                       [resultDict setObject:[NSNumber numberWithInteger:platformType] forKey:@"platform"];
                       [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
                       
                       if (state == SSDKResponseStateFail && error)
                       {
                           
                           NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                           [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                           if ([[error userInfo] objectForKey:@"error_message"])
                           {
                               if ([[error userInfo] objectForKey:@"error_message"])
                               {
                                   [errorDict setObject:[[error userInfo] objectForKey:@"error_message"]
                                                 forKey:@"error_msg"];
                                   
                               }
                           }
                           else if ([[error userInfo] objectForKey:@"user_data"])
                           {
                               NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                               if ([error_data objectForKey:@"error"])
                               {
                                   [errorDict setObject:[error_data objectForKey:@"error"]
                                                 forKey:@"error_msg"];
                               }
                               else if ([error_data objectForKey:@"error_message"])
                               {
                                   [errorDict setObject:[error_data objectForKey:@"error_message"]
                                                 forKey:@"error_msg"];
                               }
                               
                               if ([error_data objectForKey:@"error_code"])
                               {
                                   [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]]
                                                 forKey:@"error_code"];
                               }
                           }
                           
                           [resultDict setObject:errorDict forKey:@"res"];
                       }
                       
                       if (state == SSDKResponseStateSuccess)
                       {
                           if ([contentEntity rawData])
                           {
                               [resultDict setObject:[contentEntity rawData] forKey:@"res"];
                           }
                       }
                       
                       NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
                       UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
                       if (_refView)
                       {
                           //移除视图
                           [_refView removeFromSuperview];
                       }
                       
                   }];
        
    }
    
    void __iosShareSDKShowShareView (int reqID, int platType, void *content, void *observer)
    {
        NSString *observerStr = nil;
        NSMutableDictionary *shareParams = [NSMutableDictionary dictionary];
        
        
        observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        
        if (content)
        {
            NSString *contentStr = [NSString stringWithCString:content encoding:NSUTF8StringEncoding];
            shareParams = __getShareParamsWithString(contentStr);
        }
        
        
        [ShareSDK showShareEditor:platType
               otherPlatformTypes:nil
                      shareParams:shareParams
              onShareStateChanged:^(SSDKResponseState state, SSDKPlatformType platformType, NSDictionary *userData, SSDKContentEntity *contentEntity, NSError *error, BOOL end) {
                  
                  NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
                  [resultDict setObject:[NSNumber numberWithInteger:9] forKey:@"action"];
                  [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
                  [resultDict setObject:[NSNumber numberWithInteger:platformType] forKey:@"platform"];
                  [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
                  
                  if (state == SSDKResponseStateFail && error)
                  {
                      NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                      [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                      if ([[error userInfo] objectForKey:@"error_message"])
                      {
                          if ([[error userInfo] objectForKey:@"error_message"])
                          {
                              [errorDict setObject:[[error userInfo] objectForKey:@"error_message"] forKey:@"error_msg"];
                              
                          }
                      }
                      else if ([[error userInfo] objectForKey:@"user_data"])
                      {
                          NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                          if ([error_data objectForKey:@"error"])
                          {
                              [errorDict setObject:[error_data objectForKey:@"error"] forKey:@"error_msg"];
                          }
                          if ([error_data objectForKey:@"error_code"])
                          {
                              [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]] forKey:@"error_code"];
                          }
                      }
                      
                      [resultDict setObject:errorDict forKey:@"res"];
                  }
                  
                  if (state == SSDKResponseStateSuccess)
                  {
                      
                      if ([contentEntity rawData])
                      {
                          [resultDict setObject:[contentEntity rawData] forKey:@"res"];
                      }
                  }
                  
                  NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
                  UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
              }];
    }
    
    void __iosShareSDKGetFriendsList (int reqID, int platType, int count , int page, void *observer)
    {
        
        SSDKPlatformType shareType = (SSDKPlatformType)platType;
        NSString *observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        NSInteger cursor = page;
        NSInteger size = count;
        
        if (shareType == SSDKPlatformTypeTwitter)
        {
            cursor = -1;
        }
        
        [ShareSDK getFriends:platType
                      cursor:cursor
                        size:size
              onStateChanged:^(SSDKResponseState state, SSDKFriendsPaging *paging, NSError *error)
         {
             NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
             [resultDict setObject:[NSNumber numberWithInteger:2] forKey:@"action"];
             [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
             [resultDict setObject:[NSNumber numberWithInteger:shareType] forKey:@"platform"];
             [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
             
             if (state == SSDKResponseStateFail && error)
             {
                 NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                 [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                 if ([[error userInfo] objectForKey:@"error_message"])
                 {
                     if ([[error userInfo] objectForKey:@"error_message"])
                     {
                         [errorDict setObject:[[error userInfo] objectForKey:@"error_message"] forKey:@"error_msg"];
                         
                     }
                 }
                 else if ([[error userInfo] objectForKey:@"user_data"])
                 {
                     NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                     if ([error_data objectForKey:@"error"])
                     {
                         [errorDict setObject:[error_data objectForKey:@"error"] forKey:@"error_msg"];
                     }
                     if ([error_data objectForKey:@"error_code"])
                     {
                         [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]] forKey:@"error_code"];
                     }
                 }
                 
                 [resultDict setObject:errorDict forKey:@"res"];
                 
             }
             
             if (state == SSDKResponseStateSuccess)
             {
                 if (paging)
                 {
                     NSArray *friends = [NSArray array];
                     friends = paging.users;
                     NSMutableDictionary *resDict = [NSMutableDictionary dictionary];
                     [resDict setObject:friends forKey:@"users"];
                     [resDict setObject:[NSNumber numberWithInteger:paging.prevCursor] forKey:@"prev_cursor"];
                     [resDict setObject:[NSNumber numberWithInteger:paging.nextCursor] forKey:@"next_cursor"];
                     [resDict setObject:[NSNumber numberWithUnsignedInteger:paging.total] forKey:@"total"];
                     [resDict setObject:[NSNumber numberWithBool:paging.hasNext] forKey:@"has_next"];
                     [resultDict setObject:resDict forKey:@"res"];
                     
                 }
             }
             NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
             UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
             
         }];
        
    }
    char * __StringCopy( const char *string)
    {
        if (string != NULL)
        {
            char *copyStr = (char*)malloc(strlen(string)+1);
            strcpy(copyStr, string);
            return copyStr;
        }
        else
        {
            return NULL;
        }
    }
    
    extern const char* __iosShareSDKGetCredential (int platType)
    {
        SSDKPlatformType shareType = (SSDKPlatformType)platType;
        SSDKUser *userInfo = [ShareSDK currentUser:shareType];
        SSDKCredential *credential = userInfo.credential;
        NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
        [resultDict setObject:[NSNumber numberWithInteger:shareType] forKey:@"type"];
        
        if ([credential available])
        {
            if ([credential uid])
            {
                [resultDict setObject:[credential uid] forKey:@"uid"];
            }
            if ([credential token])
            {
                [resultDict setObject:[credential token] forKey:@"token"];
            }
            if ([credential secret])
            {
                [resultDict setObject:[credential secret] forKey:@"secret"];
            }
            if ([credential expired])
            {
                [resultDict setObject:@([[credential expired] timeIntervalSince1970]) forKey:@"expired"];
            }
            
            [resultDict setObject:[NSNumber numberWithBool:[credential available]] forKey:@"available"];
            
        }
        else
        {
            [resultDict setObject:[NSNumber numberWithBool:NO] forKey:@"available"];
            [resultDict setObject:@"Invalid Authorization" forKey:@"error"];
        }
        
        NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
        return __StringCopy([resultStr UTF8String]);
    }
    
    void __iosShareSDKFollowFriend (int reqID, int platType,void *account, void *observer)
    {
        SSDKPlatformType shareType = (SSDKPlatformType)platType;
        NSString *observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        SSDKUser *user = [[SSDKUser alloc]init];
        user.uid = [NSString stringWithCString:account encoding:NSUTF8StringEncoding];
        if (shareType == SSDKPlatformTypeTencentWeibo)
        {
            user.uid = nil;
            user.nickname = [NSString stringWithCString:account encoding:NSUTF8StringEncoding];
        }
        
        [ShareSDK addFriend:shareType
                       user:user
             onStateChanged:^(SSDKResponseState state, SSDKUser *user, NSError *error) {
                 NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
                 [resultDict setObject:[NSNumber numberWithInteger:6] forKey:@"action"];
                 [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
                 [resultDict setObject:[NSNumber numberWithInteger:shareType] forKey:@"platform"];
                 [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
                 
                 if (state == SSDKResponseStateFail && error)
                 {
                     NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                     [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                     if ([[error userInfo] objectForKey:@"error_message"])
                     {
                         if ([[error userInfo] objectForKey:@"error_message"])
                         {
                             [errorDict setObject:[[error userInfo] objectForKey:@"error_message"] forKey:@"error_msg"];
                             
                         }
                     }
                     else if ([[error userInfo] objectForKey:@"user_data"])
                     {
                         NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                         if ([error_data objectForKey:@"error"])
                         {
                             [errorDict setObject:[error_data objectForKey:@"error"] forKey:@"error_msg"];
                         }
                         if ([error_data objectForKey:@"error_code"])
                         {
                             [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]]
                                           forKey:@"error_code"];
                         }
                     }
                     
                     [resultDict setObject:errorDict forKey:@"res"];
                 }
                 
                 if (state == SSDKResponseStateSuccess)
                 {
                     NSDictionary *userRawdata = [NSDictionary dictionaryWithDictionary:[user rawData]];
                     [resultDict setObject:userRawdata forKey:@"res"];
                 }
                 
                 NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
                 UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
             }];
    }
    
    void __iosShareSDKShareWithContentName(int reqID, int platType, void *contentName, void *customHashtable,  void *observer)
    {
        SSDKPlatformType shareType = (SSDKPlatformType)platType;
        NSString *contentNodeName = [NSString stringWithCString:contentName encoding:NSUTF8StringEncoding];
        NSString *observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        NSDictionary *customFields = nil;
        
        if (customHashtable)
        {
            customFields = __parseWithHashtable(customHashtable);
        }
        
        [ShareSDK shareWithContentName:contentNodeName
                              platform:shareType
                          customFields:customFields
                        onStateChanged:^(SSDKResponseState state, NSDictionary *userData, SSDKContentEntity *contentEntity, NSError *error) {
                            
                            NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
                            [resultDict setObject:[NSNumber numberWithInteger:9] forKey:@"action"];
                            [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
                            [resultDict setObject:[NSNumber numberWithInteger:platType] forKey:@"platform"];
                            [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
                            
                            if (state == SSDKResponseStateFail && error)
                            {
                                NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                                [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                                if ([[error userInfo] objectForKey:@"error_message"])
                                {
                                    if ([[error userInfo] objectForKey:@"error_message"])
                                    {
                                        [errorDict setObject:[[error userInfo] objectForKey:@"error_message"]
                                                      forKey:@"error_msg"];
                                    }
                                }
                                else if ([[error userInfo] objectForKey:@"user_data"])
                                {
                                    NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                                    if ([error_data objectForKey:@"error"])
                                    {
                                        [errorDict setObject:[error_data objectForKey:@"error"] forKey:@"error_msg"];
                                    }
                                    if ([error_data objectForKey:@"error_code"])
                                    {
                                        [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]]
                                                      forKey:@"error_code"];
                                    }
                                }
                                else if([[error userInfo] objectForKey:@"error_msg"])
                                {
                                    if ([[error userInfo] objectForKey:@"error_msg"])
                                    {
                                        [errorDict setObject:[[error userInfo] objectForKey:@"error_msg"]
                                                      forKey:@"error_msg"];
                                    }
                                }
                                
                                [resultDict setObject:errorDict forKey:@"res"];
                            }
                            
                            if (state == SSDKResponseStateSuccess)
                            {
                                if ([contentEntity rawData])
                                {
                                    [resultDict setObject:[contentEntity rawData]  forKey:@"res"];
                                }
                            }
                            
                            NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
                            UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
                        }];
        
    }
    
    void __iosShareSDKShowShareMenuWithContentName(int reqID, void *contentName, void *customHashtable, void *platTypes, int x, int y, void *observer)
    {
        NSString *contentNodeName = [NSString stringWithCString:contentName encoding:NSUTF8StringEncoding];
        NSDictionary *customFields = nil;
        if (customHashtable)
        {
            customFields = __parseWithHashtable(customHashtable);
        }
        NSArray *platTypesArr = nil;
        NSMutableArray *actionSheetItems = [NSMutableArray array];
        NSString *observerStr = nil;
        
        observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        
        if (platTypes)
        {
            NSString *platTypesStr = [NSString stringWithCString:platTypes encoding:NSUTF8StringEncoding];
            platTypesArr = [MOBFJson objectFromJSONString:platTypesStr];
        }
        else
        {
            platTypesArr = [ShareSDK activePlatforms];
            for (id obj in platTypesArr)
            {
                NSInteger platformInterger = [obj integerValue];
                [actionSheetItems addObject:[NSNumber numberWithInteger:platformInterger]];
            }
            platTypesArr = [actionSheetItems mutableCopy];
            
        }
        

        if ([MOBFDevice isPad])
        {
            if (!_refView)
            {
                _refView = [[UIView alloc] initWithFrame:CGRectMake(x, y, 10, 10)];
            }
            
            [[UIApplication sharedApplication].keyWindow.rootViewController.view addSubview:_refView];
            
        }
        
        [ShareSDK showShareActionSheet:_refView
                                 items:platTypesArr
                           contentName:contentNodeName
                          customFields:customFields
                   onShareStateChanged:^(SSDKResponseState state, SSDKPlatformType platformType, NSDictionary *userData, SSDKContentEntity *contentEntity, NSError *error, BOOL end) {
                       
                       NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
                       [resultDict setObject:[NSNumber numberWithInteger:9] forKey:@"action"];
                       [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
                       [resultDict setObject:[NSNumber numberWithInteger:platformType] forKey:@"platform"];
                       [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
                       
                       if (state == SSDKResponseStateFail && error)
                       {
                           
                           NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                           [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                           if ([[error userInfo] objectForKey:@"error_message"])
                           {
                               if ([[error userInfo] objectForKey:@"error_message"])
                               {
                                   [errorDict setObject:[[error userInfo] objectForKey:@"error_message"]
                                                 forKey:@"error_msg"];
                                   
                               }
                           }
                           else if ([[error userInfo] objectForKey:@"user_data"])
                           {
                               NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                               if ([error_data objectForKey:@"error"])
                               {
                                   [errorDict setObject:[error_data objectForKey:@"error"]
                                                 forKey:@"error_msg"];
                               }
                               else if ([error_data objectForKey:@"error_message"])
                               {
                                   [errorDict setObject:[error_data objectForKey:@"error_message"]
                                                 forKey:@"error_msg"];
                               }
                               
                               if ([error_data objectForKey:@"error_code"])
                               {
                                   [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]]
                                                 forKey:@"error_code"];
                               }
                           }
                           
                           [resultDict setObject:errorDict forKey:@"res"];
                       }
                       
                       if (state == SSDKResponseStateSuccess)
                       {
                           if ([contentEntity rawData])
                           {
                               [resultDict setObject:[contentEntity rawData] forKey:@"res"];
                           }
                       }
                       
                       NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
                       UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
                       if (_refView)
                       {
                           //移除视图
                           [_refView removeFromSuperview];
                       }

                   }];
        
    }
    
    void __iosShareSDKShowShareViewWithContentName(int reqID, int platType, void *contentName, void *customHashtable, void *observer)
    {
        SSDKPlatformType shareType = (SSDKPlatformType)platType;
        NSString *contentNodeName = [NSString stringWithCString:contentName encoding:NSUTF8StringEncoding];
        NSString *observerStr = [NSString stringWithCString:observer encoding:NSUTF8StringEncoding];
        NSDictionary *customFields = nil;
        
        if (customHashtable)
        {
            customFields = __parseWithHashtable(customHashtable);
        }
        
        [ShareSDK showShareEditor:shareType
               otherPlatformTypes:nil
                      contentName:contentNodeName
                     customFields:customFields
              onShareStateChanged:^(SSDKResponseState state, SSDKPlatformType platformType, NSDictionary *userData, SSDKContentEntity *contentEntity, NSError *error, BOOL end) {
                 
                  NSMutableDictionary *resultDict = [NSMutableDictionary dictionary];
                  [resultDict setObject:[NSNumber numberWithInteger:9] forKey:@"action"];
                  [resultDict setObject:[NSNumber numberWithInteger:state] forKey:@"status"];
                  [resultDict setObject:[NSNumber numberWithInteger:platformType] forKey:@"platform"];
                  [resultDict setObject:[NSNumber numberWithInteger:reqID] forKey:@"reqID"];
                  
                  if (state == SSDKResponseStateFail && error)
                  {
                      NSMutableDictionary *errorDict = [NSMutableDictionary dictionary];
                      [errorDict setObject:[NSNumber numberWithInteger:[error code]] forKey:@"error_code"];
                      if ([[error userInfo] objectForKey:@"error_message"])
                      {
                          if ([[error userInfo] objectForKey:@"error_message"])
                          {
                              [errorDict setObject:[[error userInfo] objectForKey:@"error_message"] forKey:@"error_msg"];
                              
                          }
                      }
                      else if ([[error userInfo] objectForKey:@"user_data"])
                      {
                          NSDictionary *error_data = [[error userInfo] objectForKey:@"user_data"];
                          if ([error_data objectForKey:@"error"])
                          {
                              [errorDict setObject:[error_data objectForKey:@"error"] forKey:@"error_msg"];
                          }
                          if ([error_data objectForKey:@"error_code"])
                          {
                              [errorDict setObject:[NSNumber numberWithInteger:[[error_data objectForKey:@"error_code"] integerValue]] forKey:@"error_code"];
                          }
                      }
                      
                      [resultDict setObject:errorDict forKey:@"res"];
                  }
                  
                  if (state == SSDKResponseStateSuccess)
                  {
                      
                      if ([contentEntity rawData])
                      {
                          [resultDict setObject:[contentEntity rawData] forKey:@"res"];
                      }
                  }
                  
                  NSString *resultStr = [MOBFJson jsonStringFromObject:resultDict];
                  UnitySendMessage([observerStr UTF8String], "_Callback", [resultStr UTF8String]);
                  
              }];
    }
    
#if defined (__cplusplus)
}
#endif
@implementation ShareSDKUnity3DBridge

@end
