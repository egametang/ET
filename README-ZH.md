# [English](https://github.com/egametang/Egametang/blob/master/README-EN.md) 

# __讨论QQ群 : 474643097__  

# [ET论坛](https://et-framework.cn)  

# [ET代码商店](https://github.com/egametang/ET/tree/master/Store)  

# [ET6.0视频教程上线](https://edu.uwa4d.com/course-intro/1/375)   

# 重大注意事项：
1. Hotfix跟HotfixView是纯逻辑的，类中不要带有任何字段，否则热更就会丢失  
2. ETTask跟要么调用Coroutine要么就await，打开VS中的错误列表窗口，没有使用这两种的会报出问题，虽然既不await也不Coroutine的话能够编译通过，但是会丢失异常，十分危险  
3. 请不要使用任何虚函数，用逻辑分发替代  
4. 请不要使用任何继承，除了继承Entity，用组件替代  


# ET6 发布！ET6相比ET5有巨大变化，可以说是凤姐变亦菲，6.0拥有如下惊人的特点
1. 客户端逻辑全热更新（基于ILRuntime），没有不能更的部分  
2. 客户端服务端均可热重载，开发不用重启客户端服务端即可修改逻辑代码，开发极其方便  
3. 机器人框架，ET6的客户端的逻辑跟表现分离，机器人程序直接共享利用客户端的逻辑层代码做压测，只需要极少代码即可做出机器人，方便压测服务端  
4. 测试用例框架，利用客户端的逻辑层代码写单元测试，每个单元测试都是完整的游戏环境，无需各种恶心的mock  
5. AI框架，比行为树更加方便，写AI比写UI还简单  
6. 新的服务端架构，极其优美  
7. 内外网kcp网络，性能强劲，搭配软路由模块，可以防各种网络攻击  

# ET开发的商业mmo项目千古风流成功上线，64核128G内存的单服单物理机1.5W在线（实际线上策划为了生态限制为单服6000人同时在线，6000人的话cpu消耗约为30%）。为了堆栈行号正常，线上跑得是Debug版，如果使用Release版开启优化，性能还能翻一倍，达到单物理机3W在线！上线5个月来十分稳定。千古风流使用了ET框架从零开发，用时两年，这个开发速度可以说无人出其右。千古风流的成功上线证明了ET具备开发任何大型游戏的能力，开发速度，开发效率都令人叹为观止！千古风流使用到的客户端服务器技术：  
1. 动态副本跟分线，按需分配，用完回收  
2. 分线合线，分线人数较少会把多条线合并。合线功能基本上其它mmo游戏很少见到  
3. 客户端服务端场景无缝切换，也就是无缝大世界技术  
4. 跨服副本，跨服战场  
5. 前后端一体化，利用客户端代码开发服务器压测机器人，4台24核机器轻松模拟1W人做任务  
6. 千古风流各种ai设计，使用ET的全新开发的ai框架，使ai开发简单到跟写ui一样简单  
7. 测试用例框架，大部分重要系统，千古风流都写了测试用例，跟市面上的测试用例不同，每个千古风流的测试用例都是一个完整的游戏环境，针对协议级别，不需要搞各种接口去mock。写起来非常快速。  
8. 九宫格的aoi实现，动态调整看见的玩家，降低服务器负载  
9. 防攻击，千古风流开发了软路由功能，即使攻击也只能攻击到软路由，一旦被攻击，玩家客户端发现几秒钟无响应，即可动态切换到其它软路由，用户几乎无感知。整个过程客户端网络连接不断开，数据不丢失。  
10. 还有很多很多，这里就不啰嗦了  


# ET的介绍：
ET是一个开源的游戏客户端（基于unity3d）服务端双端框架，服务端是使用C# .net core开发的分布式游戏服务端，其特点是开发效率高，性能强，双端共享逻辑代码，客户端服务端热更机制完善，同时支持可靠udp tcp websocket协议，支持服务端3D recast寻路等等  

# ET的功能：
### 1.可用VS单步调试的分布式服务端，N变1  
一般来说，分布式服务端要启动很多进程，一旦进程多了，单步调试就变得非常困难，导致服务端开发基本上靠打log来查找问题。平常开发游戏逻辑也得开启一大堆进程，不仅启动慢，而且查找问题及其不方便，要在一堆堆日志里面查问题，这感觉非常糟糕，这么多年也没人解决这个问题。ET框架使用了类似守望先锋的组件设计，所有服务端内容都拆成了一个个组件，启动时根据服务器类型挂载自己所需要的组件。这有点类似电脑，电脑都模块化的拆成了内存，CPU，主板等等零件，搭配不同的零件就能组装成一台不同的电脑，例如家用台式机需要内存，CPU，主板，显卡，显示器，硬盘。而公司用的服务器却不需要显示器和显卡，网吧的电脑可能不需要硬盘等。正因为这样的设计，ET框架可以将所有的服务器组件都挂在一个服务器进程上，那么这个服务器进程就有了所有服务器的功能，一个进程就可以作为整组分布式服务器使用。这也类似电脑，台式机有所有的电脑组件，那它也完全可以当作公司服务器使用，也可以当作网吧电脑。  
### 2.随意可拆分功能的分布式服务端，1变N  
分布式服务端要开发多种类型的服务器进程，比如Login server，gate server，battle server，chat server friend server等等一大堆各种server，传统开发方式需要预先知道当前的功能要放在哪个服务器上，当功能越来越多的时候，比如聊天功能之前在一个中心服务器上，之后需要拆出来单独做成一个服务器，这时会牵扯到大量迁移代码的工作，烦不胜烦。ET框架在平常开发的时候根本不太需要关心当前开发的这个功能会放在什么server上，只用一个进程进行开发，功能开发成组件的形式。发布的时候使用一份多进程的配置即可发布成多进程的形式，是不是很方便呢？随便你怎么拆分服务器。只需要修改极少的代码就可以进行拆分。不同的server挂上不同的组件就行了嘛！  
### 3.跨平台的分布式服务端  
ET框架使用C#做服务端，现在C#是完全可以跨平台的，在linux上安装.netcore，即可，不需要修改任何代码，就能跑起来。性能方面，现在.netcore的性能非常强，比lua，python，js什么快的多了。做游戏服务端完全不在话下。平常我们开发的时候用VS在windows上开发调试，发布的时候发布到linux上即可。ET框架还提供了一键同步工具，打开unity->tools->rsync同步，即可同步代码到linux上  
```bash
./Run.sh Config/StartConfig/192.168.12.188.txt 
```
即可编译启动服务器。  
### 4.提供协程支持  
C#天生支持异步变同步语法 async和await，比lua，python的协程强大的多，新版python以及javascript语言甚至照搬了C#的协程语法。分布式服务端大量服务器之间的远程调用，没有异步语法的支持，开发将非常麻烦。所以java没有异步语法，做单服还行，不适合做大型分布式游戏服务端。例如：  

```c#
// 发送C2R_Ping并且等待响应消息R2C_Ping
R2C_Ping pong = await session.Call(new C2R_Ping()) as R2C_Ping;
Log.Debug("收到R2C_Ping");

// 向mongodb查询一个id为1的Player，并且等待返回
Player player = await Game.Scene.GetComponent<DBProxyComponent>().Query<Player>(1);
Log.Debug($"打印player name: {player.Name}")
```
可以看出，有了async await，所有的服务器间的异步操作将变得非常连贯，不用再拆成多段逻辑。大大简化了分布式服务器开发  
### 5.提供类似erlang的actor消息机制  
erlang语言一大优势就是位置透明的消息机制，用户完全不用关心对象在哪个进程，拿到id就可以对对象发送消息。ET框架也提供了actor消息机制，实体对象只需要挂上MailBoxComponent组件，这个实体对象就成了一个Actor，任何服务器只需要知道这个实体对象的id就可以向其发送消息，完全不用关心这个实体对象在哪个server，在哪台物理机器上。其实现原理也很简单，ET框架提供了一个位置服务器，所有挂载MailBoxComponent的实体对象都会将自己的id跟位置注册到这个位置服务器，其它服务器向这个实体对象发送消息的时候如果不知道这个实体对象的位置，会先去位置服务器查询，查询到位置再进行发送。
### 6.提供服务器不停服动态更新逻辑功能  
热更是游戏服务器不可缺少的功能，ET框架使用的组件设计，可以做成守望先锋的设计，组件只有成员，无方法，将所有方法做成扩展方法放到热更dll中，运行时重新加载dll即可热更所有逻辑。
### 7.客户端使用C#热更新，热更新一键切换  
可以使用csharp.lua或者ILRuntime稍加改造即可做客户端热更。再也不用使用狗屎lua了，客户端可以实现所有逻辑热更新，包括协议，config，ui等等。  
### 8.客户端热重载  
开发不用重启客户端即可修改客户端逻辑代码，开发极其方便  

### 9.客户端服务端用同一种语言，并且共享代码  
下载ET框架，打开服务端工程，可以看到服务端引用了客户端很多代码，通过引用客户端代码的方式实现了双端共享代码。例如客户端服务端之间的网络消息两边完全共用一个文件即可，添加一个消息只需要修改一遍。  
### 10.KCP ENET TCP Websocket协议无缝切换  
ET框架不但支持TCP，而且支持可靠的UDP协议（ENET跟KCP），ENet是英雄联盟所使用的网络库，其特点是快速，并且网络丢包的情况下性能也非常好，这个我们做过测试TCP在丢包5%的情况下，moba游戏就卡的不行了，但是使用ENet，丢包20%仍然不会感到卡。非常强大。框架还支持使用KCP协议，KCP也是可靠UDP协议，据说比ENET性能更好，使用kcp请注意，需要自己加心跳机制，否则20秒没收到包，服务端将断开连接。协议可以无缝切换。  
### 11. 3D Recast寻路功能  
可以Unity导出场景数据，给服务端做recast寻路。做MMO非常方便，demo演示了服务端3d寻路功能  
### 12. 服务端支持repl，也可以动态执行一段新代码  
这样就可以打印出进程中任何数据，大大简化了服务端查找问题的难度，开启repl方法，直接在console中输入repl回车即可进入repl模式  
### 13.提供客户端机器人框架支持  
几行代码即可创建机器人登录游戏。机器人压测轻而易举，机器人跟正常的玩家完全一样，上线前用机器人做好压测，大大降低上线崩溃几率   
### 14.AI框架  
ET的AI框架让AI编写比UI还简单     
### 15.测试用例框架  
跟市面上的测试用例不同，ET的测试用例都是一个完整的游戏环境，针对协议级别，不需要搞各种接口去mock。写起来非常快速    

### 16.还有很多很多功能，我就不详细介绍了  
a.及其方便检查CPU占用和内存泄漏检查，vs自带分析工具，不用再为性能和内存泄漏检查而烦恼  
b.使用NLog库，打log及其方便，平常开发时，可以将所有服务器log打到一个文件中，再也不用一个个文件搜索log了  
c.统一使用Mongodb的bson做序列化，消息和配置文件全部都是bson或者json，并且以后使用mongodb做数据库，再也不用做格式转换了。  
d.提供一个同步工具  

ET框架是一个强大灵活的分布式服务端架构，完全可以满足绝大部分大型游戏需求。使用这套框架，客户端开发者就可以自己完成双端开发，节省大量人力物力，节省大量沟通时间。  

使用方法：  
[运行指南](https://github.com/egametang/ET/blob/master/Book/1.1%E8%BF%90%E8%A1%8C%E6%8C%87%E5%8D%97.md)  
  
相关网站:  
[ET论坛](https://et-framework.cn)  

群友分享：  
[行为树与fgui分支(Duke Chiang开发维护)](https://github.com/DukeChiang/ET.git)   
[ET学习笔记系列(烟雨迷离半世殇写)](https://www.lfzxb.top/)   
[ET学习笔记系列(咲夜詩写)](https://acgmart.com/unity/)   
[框架服务端运行流程](http://www.cnblogs.com/fancybit/p/et1.html)  
[ET启动配置](http://www.cnblogs.com/fancybit/p/et2.html)  
[框架demo介绍](http://www.jianshu.com/p/f2ea0d26c7c1)  
[linux部署](http://gad.qq.com/article/detail/35973)  
[linux部署，mongo安装，资源服搭建](http://www.tinkingli.com/?p=25)  
[ET框架心跳包组件开发](http://www.tinkingli.com/?p=111)  
[ET框架Actor使用与心得](http://www.tinkingli.com/?p=117)  
[基于ET框架和UGUI的简单UI框架实现（渐渐写）](http://www.tinkingli.com/?p=124)  
[ET框架笔记 (笑览世界写)](http://www.tinkingli.com/?p=76)  
[ET框架如何用MAC开发](http://www.tinkingli.com/?p=147)  
[ET的动态添加事件和触发组件](http://www.tinkingli.com/?p=145)  

商业项目:  
1. [千古风流](https://www.qiangu.com/)  
2. [魔法点点2](https://www.taptap.com/app/227804)  
3. [养不大](https://www.taptap.com/app/71064)  
4. 天天躲猫猫2（ios2019春节下载排行19）  
5. [牛虎棋牌](https://gitee.com/ECPS_admin/PlanB)  
6. [五星麻将](https://github.com/wufanjoin/fivestar)  

群友demo：  
1. [斗地主（客户端服务端）](https://github.com/Viagi/LandlordsCore)  
2. [背包系统](https://gitee.com/ECPS_admin/planc)  
3. [ET小游戏合集](https://github.com/Acgmart/ET-MultiplyDemos)  



视频教程：  
[字母哥ET6.0教程](https://edu.uwa4d.com/course-intro/1/375)   
[肉饼老师主讲](http://www.taikr.com/my/course/972)  
[官剑铭主讲](https://edu.manew.com/course/796)  
[ET新手教程-初见主讲](https://pan.baidu.com/s/1a5-j2R5QctZpC9n3sMC9QQ) 密码: ru1j  
[ET新手教程新版-初见主讲](https://www.bilibili.com/video/av33280463/?redirectFrom=h5)  
[ET在Mac上运行指南-L主讲](https://pan.baidu.com/s/1VUQbdd1Yio7ULFXwAv7X7A) 密码: l3e3  
[ET框架系列教程-烟雨主讲-6.0版本](https://space.bilibili.com/33595745/favlist?fid=759596845&ftype=create)  

.net core 游戏资源分享  
[各种dotnet core项目收集](https://github.com/thangchung/awesome-dotnet-core)  

__讨论QQ群 : 474643097__


# 支付宝捐赠  
![使用支付宝对该项目进行捐赠](https://github.com/egametang/ET/blob/master/Book/donate.png)

# 友情链接  
[Box2DSharp](https://github.com/Zonciu/Box2DSharp)  box2d的C#移植版，性能很强  
[xasset](https://github.com/xasset/xasset) 致力于为 Unity 项目提供了一套 精简稳健 的资源管理环境  
[QFramework](https://github.com/liangxiegame/QFramework) Your first K.I.S.S Unity3d Framework  
[ET UI框架](https://github.com/zzjfengqing/ET-EUI) 字母哥实现的UI框架，ET风格，各种事件分发  
[ETCsharpToXLua](https://github.com/zzjfengqing/ETCsharpToXLua) 字母哥使用csharp.lua实现的ET客户端热更新  
[et-6-with-ilruntime](https://www.lfzxb.top/et-6-with-ilruntime) 烟雨使用ILRuntime实现的ET客户端热更新  
[Luban](https://github.com/focus-creative-games/luban) 适用于大中型项目的游戏配置解决方案  

