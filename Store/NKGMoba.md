# NKGMobaBasedOnET

<div align=center><img width="128" height="128" src="https://images.gitee.com/uploads/images/2021/0422/123950_25e45fe3_2253805.png"/></div>

## 介绍
基于ET框架致敬LOL的Moba游戏，包含完整的客户端与服务端交互，热更新，基于状态帧同步的战斗系统（包含完整的预测回滚功能），基于双端行为树的技能系统（提供通用的可视化节点编辑器），更多精彩等你发现！

如果你对这个开源项目有好的想法或者想和大家一起交流，可以提Issues

对于想系统学习本项目而无从下手的，推荐去看看本项目的Wiki，里面有运行指南和基础教程以及常见问题，相关技术点讲解（如果运行出现问题请先看Wiki，解决99%问题）。
[这是Wiki地址](https://gitee.com/NKG_admin/NKGMobaBasedOnET/wikis)

**基于行为树的技能系统架构系列博客总目录：[博客链接](https://www.lfzxb.top/nkgmoba-totaltabs/)** 

**基于行为树的技能系统架构讲解视频1：[视频链接](https://www.bilibili.com/video/av74833675)** 

**基于行为树的技能系统架构讲解视频2：[视频链接](https://www.bilibili.com/video/av85318986)**

**战斗系统联网演示视频：[视频链接](https://www.bilibili.com/video/BV1cK4y1S7ko)**

**基于行为树的技能系统架构直播录屏：[视频链接](https://www.bilibili.com/video/BV13K4y137vR)** 


## 特别鸣谢

感谢JetBrains公司提供的使用许可证！

<p><a href="https://www.jetbrains.com/?from=NKGMobaBasedOnET ">
<img src="https://images.gitee.com/uploads/images/2020/0722/084147_cc1c0a4a_2253805.png" alt="JetBrains的Logo" width="20%" height="20%"></a></p>

 **本项目中使用了如下插件（仅供学习交流使用，请务必支持正版！）** 


-  **[ParadoxNotion-Slate](https://slate.paradoxnotion.com/)** 
-  **[Odin](https://odininspector.com/)** 
-  **[Animancer](https://kybernetik.com.au/animancer/)** 
-  **[MonKey Commander](https://assetstore.unity.com/packages/tools/utilities/monkey-productivity-commands-119938?locale=zh-CN)** 
-  **[Status Indicators](https://assetstore.unity.com/packages/tools/particles-effects/status-indicators-88156)** 


## 运行环境

 **编辑器：Unity 2020.3.12 LTS** 

 **客户端：.Net Framework 4.7.2** 

 **IDE：JetBrain Rider 2020**

 **服务端：.Net Core 3.1** 

## 已实现功能列表

- 基于 **[FGUI](https://www.fairygui.com/)** 的UI解决方案
- 基于 **[ILRuntime](http://ourpalm.github.io/ILRuntime/public/v1/guide/index.html)** 的代码热更新方案
- 基于 **[xasset](https://github.com/xasset/xasset)** 的资源热更新方案
- 基于状态帧同步的战斗系统，包含完整的预测回滚功能（参照： **[守望先锋GDC2017分享](https://www.lfzxb.top/ow-gdc-share-table-of-contents/)** ）
- 基于 **[kcp](https://github.com/skywind3000/kcp)** 的网络通信算法
- 基于 **[Unity GraphView](https://github.com/wqaetly/NodeGraphProcessor)** 的可视化节点解决方案，可用于制作各种可视化编辑器（技能编辑器，剧情编辑器，任务编辑器，新手引导编辑器等）
- 基于 **[NPBehave行为树](https://github.com/meniku/NPBehave)** 的可视化节点技能编辑器
- 基于 **[Animancer（PlayableAPI）](https://kybernetik.com.au/animancer/)** 的动画系统
- 基于Visual Effect Graph的特效系统
- 基于ECS架构的战斗系统，包括Buff系统，技能系统，状态系统，数值系统等，相关博客参见： **[基于行为树的MOBA技能系统：总目录](https://www.lfzxb.top/nkgmoba-totaltabs/)** 
- 基于 **[recastnavigation](https://github.com/recastnavigation/recastnavigation)** 的寻路系统

## 开发计划

1. 接入 **[Slate编辑器](https://slate.paradoxnotion.com/)** ，作为Timeline方案，可用于制作ACT技能编辑器
2. 实现状态帧同步
3. 为服务端定制一套行为树数据可视化DEBUG方案
4. 实现人物在河道行走时的水波纹效果，战争迷雾效果
5. 加入寒冰，盖伦，赵信
6. 开发匹配系统


## 开发进度展示
### 资源热更新界面

![image-20200722083928209](https://images.gitee.com/uploads/images/2020/0722/084147_fc1f9a7c_2253805.png)
### 登录界面

![输入图片说明](https://images.gitee.com/uploads/images/2021/0318/170241_c54f448d_2253805.png "屏幕截图.png")
### 大厅界面

![image-20200722083952197](https://images.gitee.com/uploads/images/2020/0722/084147_e41d6ac7_2253805.png)
### 战斗界面

![image-20200722084012352](https://images.gitee.com/uploads/images/2020/0722/084147_079e755b_2253805.png)

### 基于VEG特效制作

![诺手血怒特效制作](https://images.gitee.com/uploads/images/2021/0516/211303_0fec4407_2253805.png "屏幕截图.png")
### 基于Monkey Commander改造的编辑器拓展，按F呼出界面，输入关键字，选中之后点击/回车即可运行

![基于Monkey Commander改造的编辑器拓展，按F呼出界面，输入关键字，选中之后点击/回车即可运行](https://images.gitee.com/uploads/images/2020/1029/194658_b5dee162_2253805.png "QQ截图20201029192331.png")
### Box2D编辑器

![Box2D编辑器](https://images.gitee.com/uploads/images/2021/0324/121226_528a85b5_2253805.png "QQ截图20210324121119.png")
### 技能编辑器v1.0

![技能编辑器v1.0](https://images.gitee.com/uploads/images/2021/0617/221210_d98d04bb_2253805.png "技能编辑器v1.0")
### 技能系统架构图

![163758_138e22e9_2253805](https://images.gitee.com/uploads/images/2020/0722/084148_1f2eb6b1_2253805.png)